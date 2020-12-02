using System.Text;

namespace DataExportImport
{
	public static partial class DataExportImport
	{
		internal partial class TableCommand
		{
			protected virtual void GenDynamicMethod(StringBuilder sb)
			{
			}

			protected void GenerateDynamicClass()
			{
				var sb = new StringBuilder();

				sb.AppendLine(
					@"using System;
				using System.IO;
				using System.Data;
				using System.Data.SqlClient;
				using DataExportImport;
				public static class GeneratedClass
				{");

				GenDynamicMethod(sb);

				sb.AppendLine("}");

				var generatedCode = sb.ToString();

				DynamicMethod = Compile(generatedCode);
			}
		}
	}
}