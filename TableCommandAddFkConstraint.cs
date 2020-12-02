namespace DataExportImport
{
	public static partial class DataExportImport
	{
		internal class TableCommandAddFkConstraint : TableCommandStatement
		{
			protected override string Keyword => "Create foreign key constraint for";

			public TableCommandAddFkConstraint()
			{
			}

			public TableCommandAddFkConstraint(string databaseName, string tableName, string dataFileName, string path,
				string sqlStatement) : base(databaseName, tableName, dataFileName, path)
			{
				SqlStatement = sqlStatement;
			}

			protected override void SetupSqlStatement()
			{
			}
		}
	}
}