using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Reflection;

namespace DataExportImport
{
	public static partial class DataExportImport
	{
		public static MethodInfo Compile(string generatedCode)
		{
			TraceLog.WriteLine("=== Generated code ===");

			TraceLog.WriteLine(generatedCode);

			TraceLog.WriteLine("======================");

			var provider = new CSharpCodeProvider();

			// If compiler version is required:
			//var provider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.8" } });

			var parameters = new CompilerParameters
			{
				TempFiles = new TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), true),
				GenerateInMemory = false,
				GenerateExecutable = false,
				IncludeDebugInformation = true
				//CompilerOptions = "/optimize+"
			};

			var hostAssembly = Assembly.GetExecutingAssembly();
			parameters.ReferencedAssemblies.Add(hostAssembly.Location);

			parameters.ReferencedAssemblies.Add("System.dll");
			parameters.ReferencedAssemblies.Add("System.Data.dll");
			parameters.ReferencedAssemblies.Add("System.Xml.dll");

			var results = provider.CompileAssemblyFromSource(parameters, generatedCode);

			if (results.Errors.HasErrors)
			{
				foreach (CompilerError error in results.Errors)
				{
					TraceLog.Console($"Error ({error.ErrorNumber}): {error.ErrorText}");
				}
				return null;
			}

			var assembly = results.CompiledAssembly;
			var typeName = "GeneratedClass";
			var type = assembly.GetType(typeName);
			if (type == null)
			{
				TraceLog.Console($"No such type: [{typeName}]");
				return null;
			}

			var methodName = "DynamicMethod";
			var dynamicMethod = type.GetMethod(methodName);
			if (dynamicMethod == null)
			{
				TraceLog.Console($"No such method: [{methodName}]");
				return null;
			}

			return dynamicMethod;
		}
	}
}
