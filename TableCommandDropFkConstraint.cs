namespace DataExportImport
{
	public static partial class DataExportImport
	{
		internal class TableCommandDropFkConstraint : TableCommandStatement
		{
			protected override string Keyword => "Drop foreign key constraint from";

			private readonly string _constraintName;

			public TableCommandDropFkConstraint()
			{
			}

			public TableCommandDropFkConstraint(string databaseName, string tableName, string dataFileName, string path, string constraintName) : base(databaseName, tableName, dataFileName, path)
			{
				_constraintName = constraintName;
			}

			protected override void SetupSqlStatement()
			{
				SqlStatement = $"ALTER TABLE {FullyQualifiedTableName} DROP CONSTRAINT [{_constraintName}]";
			}
		}
	}
}