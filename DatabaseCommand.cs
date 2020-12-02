using System;
using System.Collections.Generic;

namespace DataExportImport
{
	using Lines = List<string>;

	public static partial class DataExportImport
	{
		internal enum CommandDatabaseOpc
		{
			Import,
			Export,
			Delete,
			Truncate,
			AddConstraints,
			DropConstraints
		}

		internal partial class DatabaseCommand : Command
		{
			protected readonly CommandDatabaseOpc Opc;

			protected class Table
			{
				public string SchemaName;
				public string Name;
				public int DependencyLevel;
			}

			protected readonly List<Table> Tables = new List<Table>(100);
			protected readonly List<TableCommand> Commands = new List<TableCommand>(100);

			public DatabaseCommand(CommandDatabaseOpc opc)
			{
				Opc = opc;
			}

			public override void Parse(Dictionary<string, Lines> keywords)
			{
				ConnectionString = GetOneValue(keywords, KwdConnection, false);
				ServerName = GetOneValue(keywords, KwdServer, false);
				DatabaseName = GetOneValue(keywords, KwdDatabase, false);
				DataFilePath = GetOneValue(keywords, KwdPath, false);
			}

			public override void Setup()
			{
				GetTables();

				foreach (var table in Tables)
				{
					TableCommand cmd;

					switch (Opc)
					{
						case CommandDatabaseOpc.Import:
							cmd = new TableCommandImport(DatabaseName, table.Name, string.Empty, DataFilePath);
							break;
						case CommandDatabaseOpc.Export:
							cmd = new TableCommandExport(DatabaseName, table.Name, string.Empty, DataFilePath);
							break;
						case CommandDatabaseOpc.Delete:
							cmd = new TableCommandDelete(DatabaseName, table.Name, string.Empty, string.Empty);
							break;
						case CommandDatabaseOpc.Truncate:
							cmd = new TableCommandTruncate(DatabaseName, table.Name, string.Empty, string.Empty);
							break;
						default:
							throw new TraceLog.InternalException("Invalid opc");
					}

					TraceLog.Console($"Setting up {Opc} command for {table.Name}");

					try
					{
						cmd.Setup();
						Commands.Add(cmd);
					}
					catch (Exception exc)
					{
						TraceLog.Exception(exc);
					}
				}
			}

			public override int Execute()
			{
				foreach (var command in Commands)
				{
					try
					{
						command.Execute();
					}
					catch (Exception exc)
					{
						TraceLog.Exception(exc);
					}
				}

				return 0;
			}
		}
	}
}