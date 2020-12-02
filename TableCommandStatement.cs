using System.Data.SqlClient;
using System.Reflection;

namespace DataExportImport
{
	public static partial class DataExportImport
	{
		internal abstract class TableCommandStatement : TableCommand
		{
			protected SqlCommand SqlCommand;
			protected string SqlStatement;

			protected abstract string Keyword { get; }

			protected TableCommandStatement()
			{
			}

			protected TableCommandStatement(string databaseName, string tableName, string dataFileName, string path) : base(databaseName, tableName, dataFileName, path)
			{
			}

			public override void Setup()
			{
				SetupNames();
				SetupSqlStatement();
				SetupSqlCommand();
			}

			protected abstract void SetupSqlStatement();

			protected virtual void SetupSqlCommand()
			{
				SqlCommand = new SqlCommand(SqlStatement, GetSqlConnection()) { CommandTimeout = TimeoutSecRead };
			}

			public override int ExecInner(MethodInfo methodInfo)
			{
				TraceLog.Console($"{Keyword} table {FullyQualifiedTableName}");

				int cnt;

				cnt = SqlCommand.ExecuteNonQuery();

				return cnt;
			}
		}
	}
}
