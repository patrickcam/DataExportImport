using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataExportImport
{
	public static class ExtensionsDataTable
	{
		public static string Unquote(this string tableName)
		{
			//TODO Handle name with dot [x.y]
			var isQuoted = tableName.IndexOfAny(new[] { '[', ']' }) != -1;
			if (!isQuoted)
			{
				return tableName;
			}

			var elements = tableName.Split('.');
			for (var idx = 0; idx < elements.Length; idx++)
			{
				var element = elements[idx];
				elements[idx] = element.Trim('[', ']');
			}

			var res = string.Join(".", elements);

			return res;
		}


		public static string Quote(this string tableName)
		{
			var unquoted = tableName.Replace("[", null).Replace("]", null);

			var elements = unquoted.Split('.');
			for (var idx = 0; idx < elements.Length; idx++)
			{
				var element = elements[idx];
				elements[idx] = $"[{element}]";
			}

			var res = string.Join(".", elements);

			return res;
		}

		internal static DataTable SetupForInsert(string qualifiedTableName, SqlConnection sqlCcn, List<Column> columns)
		{
			var stmt = $"SET FMTONLY ON; SELECT TOP 0 * FROM {qualifiedTableName}; SET FMTONLY OFF;";

			using (var dataAdapter = new SqlDataAdapter(stmt, sqlCcn))
			using (var dataSet = new DataSet())
			{
				var results = dataAdapter.FillSchema(dataSet, SchemaType.Source);

				var datatable = results[0];

				datatable.TableName = qualifiedTableName;

				RemoveComputedColumns(datatable, columns);

				return datatable;
			}
		}

		public static void SetupOrClear(this DataTable dataTable, SqlConnection sqlCcn)
		{
			if (dataTable.Columns.Count != 0)
			{
				dataTable.Clear();
				return;
			}

			var stmt = $"SET FMTONLY ON; SELECT TOP 0 * FROM {dataTable.TableName}; SET FMTONLY OFF;";

			var dataSet = new DataSet();
			var dataAdapter = new SqlDataAdapter(stmt, sqlCcn);
			var results = dataAdapter.FillSchema(dataSet, SchemaType.Source);

			using (var useCmd = new SqlCommand(stmt, sqlCcn) { CommandTimeout = DataExportImport.TimeoutSecRead })
			using (var useDap = new SqlDataAdapter(useCmd))
			{
				useDap.Fill(dataTable);
				dataTable.Clear();
			}

			RemoveComputedColumns(dataTable, sqlCcn);
		}

		public static void SetupFromTypeOrClear(this DataTable dataTable, SqlConnection sqlCcn)
		{
			if (dataTable.Columns.Count != 0)
			{
				dataTable.Clear();
				return;
			}

			var stmt = $"DECLARE @loc {dataTable.TableName};\n";
			stmt += "SET FMTONLY ON; SELECT TOP 0 * FROM @loc; SET FMTONLY OFF;";

			using (var useCmd = new SqlCommand(stmt, sqlCcn) { CommandTimeout = DataExportImport.TimeoutSecRead })
			using (var useDap = new SqlDataAdapter(useCmd))
			{
				useDap.Fill(dataTable);
				dataTable.Clear(); // Redundant, just in case
			}
		}

		public static void RemoveColumn(this DataTable dataTable, string name)
		{
			var locCol = dataTable.Columns[name];
			if (locCol != null)
			{
				dataTable.Columns.Remove(name);
			}
		}

		internal static void RemoveComputedColumns(this DataTable dataTable, List<Column> columns)
		{
			foreach (var column in columns)
			{
				if (column.NoInsert)
				{
					dataTable.RemoveColumn(column.Name);
				}
			}
		}

		public static void RemoveComputedColumns(this DataTable dataTable, SqlConnection sqlCcn)
		{
			var stmt = $"SELECT [name] FROM [sys].[computed_columns] WHERE object_id = OBJECT_ID('{dataTable.TableName}')";
			using (var useCmd = new SqlCommand(stmt, sqlCcn) { CommandTimeout = DataExportImport.TimeoutSecRead })
			using (var reader = useCmd.ExecuteReader())
			{
				while (reader.Read())
				{
					reader.Set(out string columnName, 0);
					dataTable.RemoveColumn(columnName);
				}

				reader.Close();
			}
		}

		public static int GetOrdinal(this DataTable dataTable, string name, bool predicate = true)
		{
			if (predicate)
			{
				var column = dataTable.Columns[name];
				return column?.Ordinal ?? ExtensionsDataReader.UndefinedIndex;
			}

			return ExtensionsDataReader.UndefinedIndex;
		}

		public static int GetOrdinal(this DataTable dataTable, string name, out int length, bool predicate = true)
		{
			if (predicate)
			{
				var column = dataTable.Columns[name];

				if (column != null)
				{
					length = column.MaxLength;
					return column.Ordinal;
				}

				length = 0;
				return ExtensionsDataReader.UndefinedIndex;
			}

			length = 0;
			return ExtensionsDataReader.UndefinedIndex;
		}

		public static int AddOrdinal(this DataTable dataTable, string name, Type type)
		{
			return dataTable.Columns.Add(name, type).Ordinal;
		}

		private static void WriteToTableMessage(string destTableName, DataTable dataTable)
		{
			TraceLog.Console($"{destTableName} write count: {dataTable.Rows.Count}");
		}

		public static int WriteToServer(this DataTable dataTable, SqlTransaction sqlTxn,
			List<SqlBulkCopyColumnMapping> columnMappings, SqlBulkCopyOptions bulkOptions = DataExportImport.DefaultSqlBulkCopyOptions)
		{
			if (dataTable.Rows.Count == 0)
			{
				return 0;
			}

			using (var useCpy = new SqlBulkCopy(sqlTxn.Connection, bulkOptions, sqlTxn)
			{
				DestinationTableName = dataTable.TableName,
				BulkCopyTimeout = DataExportImport.TimeoutSecWrite,
				BatchSize = DataExportImport.DefaultSqlBulkCopyBatchSize
			})
			{
				foreach (var mapping in columnMappings)
				{
					useCpy.ColumnMappings.Add(mapping);
				}

				useCpy.WriteToServer(dataTable);
			}

			return dataTable.Rows.Count;
		}

		public static int Update(this DataTable dataTable, SqlCommand sqlCmd, SqlTransaction sqlTxn, string typeName)
		{
			if (dataTable.Rows.Count == 0)
			{
				return 0;
			}

			sqlCmd.Transaction = sqlTxn;

			sqlCmd.SetParameter(dataTable, "@prm", typeName);

			return sqlCmd.ExecuteNonQuery();
		}
	}
}

