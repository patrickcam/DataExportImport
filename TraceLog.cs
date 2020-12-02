using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace DataExportImport
{
	public static class TraceLog
	{
		private const string ConfigSectionMame = "appSettings";

		private const string DefaultFileName = "TraceLog";
		private const string FileExtension = "log";

		private static StreamWriter _logfile = StreamWriter.Null;

		public static void Start(string traceFilePath, string traceFileName)
		{
			try
			{
				if (string.IsNullOrEmpty(traceFilePath))
				{
					traceFilePath = Path.GetDirectoryName(typeof(TraceLog).Assembly.Location);
				}

				if (string.IsNullOrEmpty(traceFileName))
				{
					traceFileName = typeof(TraceLog).Assembly.GetName().Name;
				}

				var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
				var fileName = string.Concat(traceFileName, "_", timestamp, ".", FileExtension);

				var fullFileName = Path.Combine(traceFilePath, fileName);

				_logfile = new StreamWriter(new FileStream(fullFileName, FileMode.CreateNew, FileAccess.ReadWrite), Encoding.UTF8);
			}
			catch
			{
				System.Console.WriteLine("*** Error starting TraceLog, no tracefile will be written");
				_logfile = StreamWriter.Null;
			}
		}

		public static void End()
		{
			_logfile.Close();
		}

		public static void Flush()
		{
			_logfile.Flush();
		}

		public static void WriteLine(string message)
		{
			try
			{
				_logfile.WriteLine(message);
			}
			catch // Trace
			{
			}
		}

		public static void WriteLine(XmlDocument xmlDoc)
		{
			try
			{
				using (var sw = new StringWriter())
				using (var xw = XmlWriter.Create(sw))
				{
					xmlDoc.WriteTo(xw);
					xw.Flush();
					var msg = sw.GetStringBuilder().ToString();

					_logfile.WriteLine(msg);
				}
			}
			catch // Trace
			{
			}
		}

		public static void Console(string message)
		{
			try
			{
				System.Console.WriteLine(message);
				_logfile.WriteLine($"[{DateTime.Now}] *** CONSOLE {message}");
			}
			catch // Trace
			{
			}
		}


		public static void Console(Stopwatch timer)
		{
			try
			{
				var ts = timer.Elapsed;
				var msg = $"Elapsed: {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
				Console(msg);
			}
			catch // Trace
			{
			}
		}

		public static void Info(string message)
		{
			try
			{
				_logfile.WriteLine($"[{DateTime.Now}] *** INFO {message}");
			}
			catch // Trace
			{
			}
		}

		public static void Warning(string message)
		{
			try
			{
				_logfile.WriteLine($"[{DateTime.Now}] *** WARNING {message}");
			}
			catch // Trace
			{
			}
		}

		public static void Error(string message)
		{
			try
			{
				_logfile.WriteLine($"[{DateTime.Now}] *** ERROR {message}");
			}
			catch // Trace
			{
			}
		}

		public static void Exception(Exception exc, string message = null)
		{
			_logfile.WriteLine($"[{DateTime.Now}] *** EXCEPTION {message}");
			_logfile.WriteLine(exc);
		}

		public class InternalException : Exception
		{
			public InternalException(string message, Exception inner = null) : base(message, inner)
			{
			}
		}
	}
}