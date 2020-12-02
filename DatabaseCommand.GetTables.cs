using System.Data.SqlClient;

namespace DataExportImport
{
	public static partial class DataExportImport
	{
		internal partial class DatabaseCommand
		{
			protected void GetTables()
			{
				var orderDesc = string.Empty;
				if (Opc == CommandDatabaseOpc.Delete)
				{
					orderDesc = " DESC";
				}

				// Get tables with dependency level, used for correct load and delete order
				var stmt = $@"
WITH cte(
	[level], 
	[object_id], 
	[name], 
	[schema_Name])
	  AS (SELECT 
				1, 
				[t].[object_id], 
				[t].[name], 
				[s].[name] AS [schema_name]
			FROM 
				[sys].[tables] [t]
				JOIN [sys].[schemas] [s] ON [t].[schema_id] = [s].[schema_id]
			WHERE [t].[type_desc] = 'USER_TABLE'
					AND [t].[is_ms_shipped] = 0
			UNION ALL
			SELECT 
				[cte].[level] + 1, 
				[t].[object_id], 
				[t].[name], 
				[s].[name] AS [schema_name]
			FROM 
				[cte]
				JOIN [sys].[tables] AS [t] ON EXISTS
			(
				 SELECT 
					 NULL
				 FROM 
					 [sys].[foreign_keys] AS [fk]
				 WHERE [fk].[parent_object_id] = [t].[object_id]
						 AND [fk].[referenced_object_id] = [cte].[object_id]
			)
				JOIN [sys].[schemas] AS [S] ON [t].[schema_id] = [s].[schema_id]
														 AND [t].[object_id] <> [cte].[object_id]
														 AND [cte].[level] < 30
			WHERE [t].[type_desc] = 'USER_TABLE'
					AND [t].[is_ms_shipped] = 0)
	  SELECT 
		  [cte].[schema_name], 
		  [cte].[name], 
		  MAX([cte].[level]) AS [dependency_level]
	  FROM 
		  [cte]
	  WHERE [cte].[name] NOT IN ('Version')
	  GROUP BY 
		  [cte].[schema_name], 
		  [cte].[name]
	  ORDER BY 
		  [dependency_level]{orderDesc}, 
		  [cte].[schema_name], 
		  [cte].[name]";

				using (var useCmd = new SqlCommand(stmt, GetSqlConnection(), null) { CommandTimeout = TimeoutSecRead })
				using (var useReader = useCmd.ExecuteReader())
				{
					while (useReader.Read())
					{
						var table = new Table
						{
							SchemaName = useReader.GetString(0),
							Name = useReader.GetString(1),
							DependencyLevel = useReader.GetInt32(2)
						};

						Tables.Add(table);
					}
				}
			}
		}
	}
}