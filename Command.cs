using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace DataExportImport
{
	using Lines = List<string>;

	public static partial class DataExportImport
	{
		public enum Status
		{
			Success = 0,
			Fail
		}

		internal abstract class Command
		{
			public Status Status;

			protected static SqlConnection SqlCcn;

			protected string ConnectionString;
			protected string ServerName;
			protected string DatabaseName;
			protected string DataFilePath;

			protected string SqlConstraintsFileName => $"{DatabaseName}_Add_Constraints.sql";
			protected string SqlSavedConstraintsFileName => $"{DatabaseName}_Add_Constraints_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
			protected string SqlConstraintsFullFileName => Path.Combine(DataFilePath, SqlConstraintsFileName);
			protected string SqlSavedConstraintsFullFileName => Path.Combine(DataFilePath, SqlSavedConstraintsFileName);

			public abstract void Parse(Dictionary<string, Lines> keywords);

			public abstract void Setup();

			public abstract int Execute();

			public SqlConnection GetSqlConnection()
			{
				return SqlCcn ?? (SqlCcn = SetupSqlConnection());
			}

			public SqlConnection SetupSqlConnection()
			{
				ConnectionString = SetupConnectionString();

				if (string.IsNullOrEmpty(ConnectionString))
				{
					throw new TraceLog.InternalException("Missing connection string");
				}

				var sqlConnection = new SqlConnection(ConnectionString);
				sqlConnection.Open();

				return sqlConnection;
			}

			protected string SetupConnectionString()
			{
				var builder = new SqlConnectionStringBuilder(ConnectionString)
				{
					["Data Source"] = string.IsNullOrEmpty(ServerName) ? "(local)" : ServerName,
					["Integrated Security"] = true,
					["Initial Catalog"] = DatabaseName
				};

				return builder.ConnectionString;
			}
		}
	}
}