using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DataExportImport
{
	using Lines = List<string>;

	public static partial class DataExportImport
	{
		internal abstract partial class TableCommand : Command
		{
			protected const long FileCheckMark = 868244108L;
			protected const int EofIndex = int.MaxValue;
			protected const int BufferSize = 0x20000;

			protected string SchemaName;
			protected string TableName;
			protected string FullyQualifiedTableName;

			protected string DataFileName;

			protected MethodInfo DynamicMethod;

			protected List<Column> TableColumns;
			protected readonly List<Column> HeaderColumns = new List<Column>(100);

			protected TableCommand()
			{
			}

			protected TableCommand(string databaseName, string tableName, string dataFileName, string path)
			{
				DatabaseName = databaseName;
				TableName = tableName;
				DataFileName = dataFileName;
				DataFilePath = path;
			}

			protected bool HasSqlStatement(Dictionary<string, Lines> keywords)
			{
				return keywords.TryGetValue(KwdSql, out var sqlLines) && sqlLines.Any();
			}

			public override void Parse(Dictionary<string, Lines> keywords)
			{
				ConnectionString = GetOneValue(keywords, KwdConnection, false);

				DatabaseName = GetOneValue(keywords, KwdDatabase, false);
				SchemaName = GetOneValue(keywords, KwdSchema, false);
				TableName = GetOneValue(keywords, KwdTable, !HasSqlStatement(keywords));

				DataFilePath = GetOneValue(keywords, KwdPath, false);
				DataFileName = GetOneValue(keywords, KwdFilename, false);

				ParseInner(keywords);

				TraceLog.Flush();
			}

			protected virtual void ParseInner(Dictionary<string, Lines> keywords)
			{
			}

			public abstract int ExecInner(MethodInfo methodInfo);

			public override int Execute()
			{
				var cnt = 0;

				try
				{
					cnt = ExecInner(DynamicMethod);

					TraceLog.Console($"Records: {cnt}");
				}
				catch (Exception exc)
				{
					TraceLog.Exception(exc);
					Status = Status.Fail;
				}

				return cnt;
			}

			protected void SetupNames()
			{
				if (string.IsNullOrEmpty(TableName))
				{
					throw new TraceLog.InternalException("Missing table name");
				}

				var elements = TableName.Split('.');
				switch (elements.Length)
				{
					case 1:
						break;
					case 2:
						SchemaName = elements[0];
						TableName = elements[1];
						break;
					case 3:
						DatabaseName = elements[0];
						SchemaName = elements[1];
						TableName = elements[2];
						break;
					default:
						throw new TraceLog.InternalException($"Invalid table name [{TableName}]");

				}

				if (!string.IsNullOrEmpty(ConnectionString))
				{
					var builder = new SqlConnectionStringBuilder(ConnectionString);

					if (string.IsNullOrEmpty(DatabaseName))
					{
						DatabaseName = builder.InitialCatalog;
					}

					if (string.IsNullOrEmpty(ServerName))
					{
						ServerName = builder.DataSource;
					}
				}

				if (string.IsNullOrEmpty(ServerName))
				{
					ServerName = "(local)";
				}

				if (string.IsNullOrEmpty(DatabaseName))
				{
					throw new TraceLog.InternalException("Missing database or initial catalog name");
				}

				if (string.IsNullOrEmpty(SchemaName))
				{
					SchemaName = "dbo";
				}

				DatabaseName = DatabaseName.Unquote();
				SchemaName = SchemaName.Unquote();
				TableName = TableName.Unquote();

				FullyQualifiedTableName = string.Join(".", DatabaseName.Quote(), SchemaName.Quote(), TableName.Quote());

				if (string.IsNullOrEmpty(DataFileName))
				{
					DataFileName = $"{DatabaseName}_{TableName}.dat";
				}

				if (!string.IsNullOrEmpty(DataFilePath))
				{
					DataFileName = Path.Combine(DataFilePath, DataFileName);
				}
			}

			protected virtual void SetupTableColumns()
			{
			}
		}
	}
}