using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace DataExportImport
{
	internal static partial class DataExportImport1
	{
		public const long FileCheck = 868244108L;
		public const int BufferSize = 0x20000;

		public static tStatus GenExportImport(string tableName, string ccnString, string fileName)
		{
			var sb = new StringBuilder();

			sb.AppendLine(@"
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Xml;
using DataExportImport;
public static class GeneratedExportImport
			{");

			using (var sqlCcn = new SqlConnection(ccnString))
			{
				sqlCcn.Open();

				GenExport(sqlCcn, sb, tableName, ccnString, fileName);
				GenImport(sqlCcn, sb, tableName, ccnString, fileName);
			}

			sb.AppendLine("}");

			return Compile(sb.ToString());
		}

		public static void GenExport(SqlConnection sqlCcn, StringBuilder sb, string tableName, string ccnString, string fileName)
		{
			var datatable = ExtensionsDataTable.SetupForInsert(tableName, sqlCcn);

			sb.AppendLine($@"public static int ExportToBinary()
			{{
				const string tableName = @""{tableName}"";
				const string ccnString = @""{ccnString}"";
				const string fileName = @""{fileName}"";
				int recIdx = 0;

				using (var sqlCcn = new SqlConnection(ccnString))
				{{
					sqlCcn.Open();

					var datatable = ExtensionsDataTable.SetupForInsert(""{tableName}"", sqlCcn);

					var sqlStmt = string.Format(""SELECT * FROM {{0}}"", tableName);

					using (var fileStream = new FileStream(fileName, FileMode.Create))
					using (var gZipStream = new GZipStream(fileStream, CompressionMode.Compress))
					using (var bufferedStream = new BufferedStream(gZipStream, {BufferSize}))
					using (var bw = new BinaryWriter(bufferedStream))
					using (var sqlCmd = new SqlCommand(sqlStmt, sqlCcn))
					using (var runRow = sqlCmd.ExecuteReader())
					{{
						DataExportImport.Callback.WriteFileHeader(bw, datatable, tableName);");
			sb.AppendLine();

			sb.AppendLine(@"while (runRow.Read())
			{");

			sb.AppendLine(@"bw.Write(recIdx++);");

			var idx = 0;
			foreach (DataColumn col in datatable.Columns)
			{
				var idnType = col.DataType.Name;
				if (col.AllowDBNull)
				{
					idnType += "Nullable";
				}
				sb.AppendLine($@"runRow.WriteBinary{idnType}({idx++}, bw); // {col.ColumnName}");
			}

			sb.AppendLine("}");

			sb.AppendLine("bw.Write(int.MaxValue);");

			sb.AppendLine("}");
			sb.AppendLine("}");
			sb.AppendLine("return recIdx;");
			sb.AppendLine("}");
		}

		public static void GenImport(SqlConnection sqlCcn, StringBuilder sb, string tableName, string ccnString, string fileName)
		{
			var datatable = ExtensionsDataTable.SetupForInsert(tableName, sqlCcn);

			//foreach (DataColumn column in datatable.Columns)
			//{
			//	Console.WriteLine(column.ColumnName);
			//}


			//using (var fileStream = new FileStream(fileName, FileMode.Open))
			//using (var gZipStream = new GZipStream(fileStream, CompressionMode.Decompress))
			//using (var bufferedStream = new BufferedStream(gZipStream, BufferSize))
			//using (var br = new BinaryReader(bufferedStream))
			//{

			//	Callback.CheckFileHeader(br, datatable);

			//}

			//return;

			sb.AppendLine($@"public static int ImportFromBinary()
			{{
				const string ccnString = @""{ccnString}"";
				const string fileName = @""{fileName}"";
				int idx = -1;

				using (var sqlCcn = new SqlConnection(ccnString))
				{{
					sqlCcn.Open();

					var datatable = ExtensionsDataTable.SetupForInsert(""{tableName}"", sqlCcn);

					using (var fileStream = new FileStream(fileName, FileMode.Open))
					using (var gZipStream = new GZipStream(fileStream, CompressionMode.Decompress))
					using (var bufferedStream = new BufferedStream(gZipStream, {BufferSize}))
					using (var br = new BinaryReader(bufferedStream))
					{{
						DataExportImport.Callback.CheckFileHeader(br, datatable);
					");

			sb.AppendLine(@"
						while (true)
						{
							var recIdx = br.ReadInt32();

							if (recIdx == int.MaxValue)
							{
								break;
							}

							if (recIdx != ++idx)
							{
								throw new Exception(string.Format(""Record index out of sync, expected:{0}, received:{1}"", idx, recIdx));
							}

							var newRow = datatable.NewRow();");

			var idx = 0;
			foreach (DataColumn col in datatable.Columns)
			{
				var idnType = col.DataType.Name;
				if (col.AllowDBNull)
				{
					idnType += "Nullable";
				}
				sb.AppendLine($@"newRow.ReadBinary{idnType}({idx++}, br); // {col.ColumnName}");
			}

			sb.AppendLine("datatable.Rows.Add(newRow);");

			sb.AppendLine("}");

			sb.AppendLine(@"using (var sqlTxn = sqlCcn.BeginTransaction())
			{
				datatable.WriteToServer(sqlTxn);
				sqlTxn.Commit();
			}");

			sb.AppendLine("}");
			sb.AppendLine("}");
			sb.AppendLine("return idx+1;");
			sb.AppendLine("}");
		}
	}
}
