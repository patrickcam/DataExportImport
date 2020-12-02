using System.Data;
using System.IO;
using System.IO.Compression;

namespace DataExportImport
{
	public static partial class DataExportImport
	{
		internal partial class TableCommand
		{
			protected void ReadFileHeader()
			{
				using (var fileStream = new FileStream(DataFileName, FileMode.Open))
				using (var gZipStream = new GZipStream(fileStream, CompressionMode.Decompress))
				using (var bufferedStream = new BufferedStream(gZipStream, BufferSize))
				using (var br = new BinaryReader(bufferedStream))
				{
					var fileCheckMark = br.ReadInt64();
					if (fileCheckMark != FileCheckMark)
					{
						throw new TraceLog.InternalException("The input file is not a valid export file");
					}

					var checkTableName = br.ReadString();

					var colCnt = br.ReadInt32();

					for (var cnt = 1; cnt <= colCnt; cnt++)
					{
						var newCol = Column.ReadFromHeader(br);
						HeaderColumns.Add(newCol);
					}

					fileCheckMark = br.ReadInt64();
					if (fileCheckMark != FileCheckMark)
					{
						throw new TraceLog.InternalException("Missing end of header mark");
					}
				}
			}

			protected static void SkipFileHeader(BinaryReader br)
			{
				{
					var fileCheckMark = br.ReadInt64();
					if (fileCheckMark != FileCheckMark)
					{
						throw new TraceLog.InternalException("The input file is not a valid export file");
					}

					var checkTableName = br.ReadString();

					var colCnt = br.ReadInt32();

					for (var cnt = 1; cnt <= colCnt; cnt++)
					{
						Column.ReadFromHeader(br);
					}

					fileCheckMark = br.ReadInt64();
					if (fileCheckMark != FileCheckMark)
					{
						throw new TraceLog.InternalException("Missing end of header mark");
					}
				}
			}

			public void WriteFileHeader(BinaryWriter bw)
			{
				bw.Write(FileCheckMark);
				bw.Write(TableName);
				bw.Write(TableColumns.Count);

				TableColumns.ForEach(c => c.WriteToHeader(bw));

				bw.Write(FileCheckMark);
			}

			public void CheckFileHeader(BinaryReader br, DataTable datatable)
			{
				var fileCheck = br.ReadInt64();
				if (fileCheck != FileCheckMark)
				{
					throw new TraceLog.InternalException("The input file is not a valid export file");
				}

				var checkTableName = br.ReadString();
				var colCnt = br.ReadInt32();

				foreach (DataColumn col in datatable.Columns)
				{
					var colName = br.ReadString();

					if (colName != col.ColumnName)
					{
						throw new TraceLog.InternalException($"Column name mismatch, tab:{col.ColumnName}, file:{colName}");
					}
				}

				if (colCnt != datatable.Columns.Count)
				{
					TraceLog.Console("Columns in table:");
					foreach (DataColumn col in datatable.Columns)
					{
						TraceLog.Console(col.ColumnName);
					}

					TraceLog.Console("Columns in file:");
					for (var idx = 0; idx < colCnt; idx++)
					{
						TraceLog.Console(br.ReadString());
					}

					throw new TraceLog.InternalException($"Column count mismatch, tab:{datatable.Columns.Count}, file:{colCnt}");
				}

				fileCheck = br.ReadInt64();
				if (fileCheck != FileCheckMark)
				{
					throw new TraceLog.InternalException("Missing end of header mark");
				}
			}
		}
	}
}