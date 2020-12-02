using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;

namespace DataExportImport
{
	public static partial class DataExportImport
	{
		public static string CommandFileName;

		public static int TimeoutSecRead = 300;
		public static int TimeoutSecWrite = 300;

		public static int DefaultSqlBulkCopyBatchSize = 0;

		public static int CommitEvery = 20000;
		public static int LogRecordCountEvery = 10000;

		public const SqlBulkCopyOptions DefaultSqlBulkCopyOptions = SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.KeepIdentity;

		public static void SetupConfig()
		{
			const string configSectionMame = "appSettings";

			var TraceFilePath = string.Empty;
			var TraceFileName = string.Empty;

			var section = (NameValueCollection)ConfigurationManager.GetSection(configSectionMame);
			if (section != null)
			{
				CommandFileName = section[nameof(CommandFileName)];
				TraceFilePath = section[nameof(TraceFilePath)];
				TraceFileName = section[nameof(TraceFileName)];
				TimeoutSecRead = int.Parse(section[nameof(TimeoutSecRead)]);
				TimeoutSecWrite = int.Parse(section[nameof(TimeoutSecWrite)]);
				CommitEvery = int.Parse(section[nameof(CommitEvery)]);
				DefaultSqlBulkCopyBatchSize = int.Parse(section[nameof(DefaultSqlBulkCopyBatchSize)]);
			}

			TraceLog.Start(TraceFilePath, TraceFileName);
		}

		static int Main(string[] args)
		{
			var status = Status.Success;

			SetupConfig();

			if (args == null || args.Length == 0 || string.IsNullOrEmpty(args[0]))
			{
				TraceLog.Console($"Using default command file [{CommandFileName}]");
			}
			else
			{
				CommandFileName = args[0];
			}

			if (string.IsNullOrEmpty(CommandFileName))
			{
				Console.WriteLine("Argument [specification-file-name] is required");
				return (int)Status.Fail;
			}

			try
			{
				var command = Parse(CommandFileName);

				command.Setup();

				var timer = new Stopwatch();
				timer.Start();

				command.Execute();

				TraceLog.Console(timer);
				TraceLog.Console("---------------------------------------------------------------------------");
			}
			catch (Exception exc)
			{
				TraceLog.Exception(exc);
				status = Status.Fail;
			}
			finally
			{
				TraceLog.End();
			}

			return (int)status;
		}
	}
}