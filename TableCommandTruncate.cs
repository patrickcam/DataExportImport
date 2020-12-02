namespace DataExportImport
{
	public static partial class DataExportImport
	{
		internal class TableCommandTruncate : TableCommandDelete
		{
			protected override string Keyword => "Truncate";

			public TableCommandTruncate()
			{
			}

			public TableCommandTruncate(string databaseName, string tableName, string dataFileName, string path) : base(databaseName, tableName, dataFileName, path)
			{
			}

			protected override void SetupSqlStatement()
			{
				SqlStatement = $"TRUNCATE TABLE {FullyQualifiedTableName}";
			}
		}
	}
}