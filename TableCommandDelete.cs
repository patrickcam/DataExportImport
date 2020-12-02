namespace DataExportImport
{
	public static partial class DataExportImport
	{
		internal class TableCommandDelete : TableCommandStatement
		{
			protected override string Keyword => "Delete";

			public TableCommandDelete()
			{
			}

			public TableCommandDelete(string databaseName, string tableName, string dataFileName, string path) : base(databaseName, tableName, dataFileName, path)
			{
			}

			protected override void SetupSqlStatement()
			{
				SqlStatement = $"DELETE FROM {FullyQualifiedTableName}";
			}
		}
	}
}
