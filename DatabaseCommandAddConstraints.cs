using System.IO;

namespace DataExportImport
{
	public static partial class DataExportImport
	{
		internal class DatabaseCommandAddConstraints : DatabaseCommand
		{
			public DatabaseCommandAddConstraints(CommandDatabaseOpc opc) : base(opc)
			{
			}

			public override void Setup()
			{
				SetupAddFkConstraintStatements();

				Commands.ForEach(c => c.Setup());
			}

			private void SetupAddFkConstraintStatements()
			{
				if (!File.Exists(SqlConstraintsFullFileName))
				{
					TraceLog.Console($"Creation SQL script not found [{SqlConstraintsFullFileName}]");
					return;
				}

				using (var cmdFile = new System.IO.StreamReader(SqlConstraintsFullFileName))
				{
					string line;
					while ((line = cmdFile.ReadLine()) != null)
					{
						var tableCommand = new TableCommandAddFkConstraint(
							DatabaseName,
							"statement",
							string.Empty,
							string.Empty,
							line);

						Commands.Add(tableCommand);
					}
				}
			}
		}
	}
}