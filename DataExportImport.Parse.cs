using System.Collections.Generic;
using System.IO;

namespace DataExportImport
{
	using Lines = List<string>;

	public static partial class DataExportImport
	{
		private const string KwdServer = "server";
		private const string KwdDatabase = "database";
		private const string KwdSchema = "schema";
		private const string KwdCommand = "command";
		private const string KwdComment = "comment";
		private const string KwdConnection = "connection";
		private const string KwdFilename = "filename";
		private const string KwdTable = "table";
		private const string KwdColumns = "columns";
		private const string KwdCompute = "compute";
		private const string KwdWhere = "where";
		private const string KwdSql = "sql";
		private const string KwdPath = "path";

		private const string MarkComment = "!";
		private const string MarkKeyword = "#";
		private const char MarkKeywordChar = '#';

		private static Command Parse(string fileName)
		{
			TraceLog.Console($"Parsing command file [{fileName}]");

			Lines currentKeywordLines = null;

			var keywords = SetupKeywords();

			using (var sr = new StreamReader(fileName))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					if (string.IsNullOrEmpty(line))
					{
						continue;
					}

					line = line.Trim();

					if (string.IsNullOrEmpty(line) || line.StartsWith(MarkComment))
					{
						continue;
					}

					if (line.StartsWith(MarkKeyword))
					{
						var kwd = line.Trim(MarkKeywordChar, ' ').ToLower();

						if (!keywords.TryGetValue(kwd, out currentKeywordLines))
						{
							throw new TraceLog.InternalException($"No such keyword: [{kwd}]");
						}
					}
					else
					{
						currentKeywordLines?.Add(line);
					}
				}
			}

			Command command;

			var cmd = GetOneValue(keywords, KwdCommand, true);
			switch (cmd.ToLower())
			{
				case "import":
					command = new TableCommandImport();
					break;
				case "export":
					command = new TableCommandExport();
					break;
				case "delete":
					command = new TableCommandDelete();
					break;
				case "importall":
					command = new DatabaseCommand(CommandDatabaseOpc.Import);
					break;
				case "exportall":
					command = new DatabaseCommand(CommandDatabaseOpc.Export);
					break;
				case "deleteall":
					command = new DatabaseCommand(CommandDatabaseOpc.Delete);
					break;
				case "truncateall":
					command = new DatabaseCommand(CommandDatabaseOpc.Truncate);
					break;
				case "addconstraints":
					command = new DatabaseCommandAddConstraints(CommandDatabaseOpc.AddConstraints);
					break;
				case "dropconstraints":
					command = new DatabaseCommandDropConstraints(CommandDatabaseOpc.DropConstraints);
					break;
				default:
					throw new TraceLog.InternalException($"Invalid command [{cmd}]");
			}

			command.Parse(keywords);

			return command;
		}

		public static string GetOneValue(Dictionary<string, Lines> keywords, string kwd, bool isReqired)
		{
			var list = keywords[kwd];
			if (list.Count == 1)
			{
				TraceLog.WriteLine($"{kwd}: [{list[0]}]");

				return list[0];
			}

			if (isReqired)
			{
				throw new TraceLog.InternalException($"Exactly one [{kwd}] is required in command");
			}

			TraceLog.WriteLine($"{kwd}: not specified");
			return string.Empty;
		}

		private static Dictionary<string, Lines> SetupKeywords()
		{
			var keywords = new Dictionary<string, Lines>
			{
				{KwdServer, new Lines()},
				{KwdDatabase, new Lines()},
				{KwdSchema, new Lines()},
				{KwdCommand, new Lines()},
				{KwdComment, new Lines()},
				{KwdConnection, new Lines()},
				{KwdFilename, new Lines()},
				{KwdTable, new Lines()},
				{KwdColumns, new Lines()},
				{KwdCompute, new Lines()},
				{KwdWhere, new Lines()},
				{KwdSql, new Lines()},
				{KwdPath, new Lines()}
			};
			return keywords;
		}
	}
}