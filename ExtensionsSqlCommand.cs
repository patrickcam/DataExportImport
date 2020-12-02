using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Xml;

namespace DataExportImport
{
	public static class ExtensionsSqlCommand
	{
		public static List<Column> SetupColumns(this SqlCommand sqlCmd)
		{
			var columns = new List<Column>(100);

			using (var reader = sqlCmd.ExecuteReader(CommandBehavior.SchemaOnly))
			using (var schemaTable = reader.GetSchemaTable())
			{
				if (schemaTable == null)
				{
					throw new TraceLog.InternalException("Error getting metadata of SqlCommand");
				}

				var idxColumnName = schemaTable.GetOrdinal("ColumnName");
				var idxDataType = schemaTable.GetOrdinal("DataType");
				var idxAllowDBNull = schemaTable.GetOrdinal("AllowDBNull");
				var idxColumnSize = schemaTable.GetOrdinal("ColumnSize");
				var idxColumnOrdinal = schemaTable.GetOrdinal("ColumnOrdinal");

				var idxNumericPrecision = schemaTable.GetOrdinal("NumericPrecision");
				var idxNumericScale = schemaTable.GetOrdinal("NumericScale");
				var idxIsUnique = schemaTable.GetOrdinal("IsUnique");
				var idxIsKey = schemaTable.GetOrdinal("IsKey");
				var idxIsExpression = schemaTable.GetOrdinal("IsExpression");
				var idxIsIdentity = schemaTable.GetOrdinal("IsIdentity");
				var idxIsAutoIncrement = schemaTable.GetOrdinal("IsAutoIncrement");
				var idxIsHidden = schemaTable.GetOrdinal("IsHidden");
				var idxIsLong = schemaTable.GetOrdinal("IsLong");
				var idxIsReadOnly = schemaTable.GetOrdinal("IsReadOnly");
				var idxDataTypeName = schemaTable.GetOrdinal("DataTypeName");

				foreach (DataRow dataRow in schemaTable.Rows)
				{
					var type = dataRow.GetValueType(idxDataType);
					var extendedTypeCode = type.GetExtendedTypeCode();

					var newColumn = new Column
					{
						Name = dataRow.GetValueString(idxColumnName),
						DataTypeName = dataRow.GetValueString(idxDataTypeName),
						IsNullable = dataRow.GetValueBoolean(idxAllowDBNull),
						ColumnSize = dataRow.GetValueInt32(idxColumnSize),
						TableOrdinal = dataRow.GetValueInt32(idxColumnOrdinal),
						Type = type,
						ExtendedTypeCode = extendedTypeCode,
						TypeCategory = extendedTypeCode.GetTypeCategory(),
						NumericPrecision = dataRow.GetValueInt16(idxNumericPrecision),
						NumericScale = dataRow.GetValueInt16(idxNumericScale),
						IsUnique = dataRow.GetValueBoolean(idxIsUnique),
						IsIdentity = dataRow.GetValueBoolean(idxIsIdentity),
						IsAutoIncrement = dataRow.GetValueBoolean(idxIsAutoIncrement),
						IsLong = dataRow.GetValueBoolean(idxIsLong),
						IsReadOnly = dataRow.GetValueBoolean(idxIsReadOnly)
					};

					columns.Add(newColumn);

					//newColumn.Dump();
				}
			}

			return columns;
		}

		public static SqlParameter GetOrAddParameter(this SqlCommand sqlCmd, string name, SqlDbType sqlType)
		{
			SqlParameter prm;
			var idx = sqlCmd.Parameters.IndexOf(name);
			if (idx == ExtensionsDataReader.UndefinedIndex)
			{
				prm = new SqlParameter(name, sqlType);
				sqlCmd.Parameters.Add(prm);
			}
			else
			{
				prm = sqlCmd.Parameters[idx];
			}

			return prm;
		}

		public static SqlParameter GetOrAddStructuredParameter(this SqlCommand sqlCmd, string name, string typeName)
		{
			SqlParameter prm;
			var idx = sqlCmd.Parameters.IndexOf(name);
			if (idx == ExtensionsDataReader.UndefinedIndex)
			{
				prm = new SqlParameter(name, SqlDbType.Structured)
				{
					TypeName = typeName
				};
				sqlCmd.Parameters.Add(prm);
			}
			else
			{
				prm = sqlCmd.Parameters[idx];
			}

			return prm;
		}

		public static void SetParameter(this SqlCommand sqlCmd, bool value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.Bit);
			prm.Value = value;
		}

		public static void SetParameterNullIfFalse(this SqlCommand sqlCmd, bool value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.Bit);
			prm.Value = value ? (object)true : DBNull.Value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, bool? value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.Bit);
			prm.Value = value != null && value.Value ? (object)true : DBNull.Value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, char value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.Char);
			prm.Value = value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, byte value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.TinyInt);
			prm.Value = value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, byte? value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.TinyInt);
			prm.Value = value != null ? (object)value : DBNull.Value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, short value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.SmallInt);
			prm.Value = value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, short? value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.SmallInt);
			prm.Value = value != null ? (object)value : DBNull.Value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, int value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.Int);
			prm.Value = value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, int? value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.Int);
			prm.Value = value != null ? (object)value : DBNull.Value;
		}

		public static void SetParameterNullIfNeg(this SqlCommand sqlCmd, int? value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.Int);
			prm.Value = value != null && value >= 0 ? (object)value : DBNull.Value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, long value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.BigInt);
			prm.Value = value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, long? value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.BigInt);
			prm.Value = value != null ? (object)value : DBNull.Value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, float value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.Real);
			prm.Value = value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, float? value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.Real);
			prm.Value = value != null ? (object)value : DBNull.Value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, DateTime? value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.DateTime2);
			prm.Value = value != null ? (object)value : DBNull.Value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, DateTime value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.DateTime2);
			prm.Value = (DateTime)SqlDateTime.MinValue <= value && value <= (DateTime)SqlDateTime.MaxValue
				? (object)value
				: DBNull.Value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, string value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.NVarChar);
			prm.Value = value != null ? (object)value : DBNull.Value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, Guid value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.UniqueIdentifier);
			prm.Value = value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, Guid? value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.UniqueIdentifier);
			prm.Value = value != null ? (object)value : DBNull.Value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, XmlDocument value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.Xml);
			prm.Value = value != null ? (object)value : DBNull.Value;
		}

		public static void SetParameterNullIfEmpty(this SqlCommand sqlCmd, string value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.NVarChar);
			prm.Value = string.IsNullOrEmpty(value) ? null : value;
		}

		public static void SetParameterNullIfEmpty(this SqlCommand sqlCmd, string value, string name, int length)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.NVarChar);
			prm.Value = string.IsNullOrEmpty(value) ? null : value.Length <= length ? value : value.Substring(0, length);
		}

		public static void SetParameter(this SqlCommand sqlCmd, decimal? value, string name)
		{
			var prm = GetOrAddParameter(sqlCmd, name, SqlDbType.Decimal);
			prm.Value = value != null ? (object)value : DBNull.Value;
		}

		public static void SetParameter(this SqlCommand sqlCmd, DataTable dataTable, string name, string typeName)
		{
			var prm = GetOrAddStructuredParameter(sqlCmd, name, typeName);
			prm.Value = dataTable;
		}
	}
}
