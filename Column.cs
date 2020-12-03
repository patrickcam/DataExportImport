using System;
using System.Data;
using System.IO;
using System.Text;

namespace DataExportImport
{
	public class Column
	{
		public string Name;
		public Type Type;
		public ExtendedTypeCode ExtendedTypeCode;
		public TypeCategory TypeCategory;
		public string DataTypeName;

		public bool IsNullable;
		public bool IsComputed;

		public int ColumnSize;
		public int TableOrdinal;

		public short NumericPrecision;
		public short NumericScale;
		public bool IsUnique;
		public bool IsKey;
		public bool IsExpression;
		public bool IsIdentity;
		public bool IsAutoIncrement;
		public bool IsHidden;
		public bool IsLong;
		public bool IsReadOnly;

		public Column MatchedColumn;
		public bool IsMatched => MatchedColumn != null;

		public string TypeName => ExtendedTypeCode.ToString();

		public bool NoInsert => IsReadOnly && !IsIdentity;

		public int GetDestDataTableOrdinal(DataTable destDataTable)
		{
			var ordinal = destDataTable.GetOrdinal(Name);

			if (ordinal == ExtensionsDataReader.UndefinedIndex)
			{
				throw new TraceLog.InternalException($"Column [{Name}] not found in import datatable");
			}

			return ordinal;
		}


		public static Column ReadFromHeader(BinaryReader br)
		{
			var name = br.ReadString();
			var extendedTypeCode = (ExtendedTypeCode)br.ReadInt32();

			//!DBG! Older header version
			if (extendedTypeCode == ExtendedTypeCode.Object)
			{
				extendedTypeCode = ExtendedTypeCode.Guid;
			}

			var isNullable = br.ReadBoolean();

			var type = extendedTypeCode.GetTypeFromExtendedTypeCode();
			var typeCategory = extendedTypeCode.GetTypeCategory();

			var newCol = new Column
			{
				Name = name,
				Type = type,
				ExtendedTypeCode = extendedTypeCode,
				TypeCategory = typeCategory,
				IsNullable = isNullable
			};

			newCol.ReadSpecificsFromHeader(br);

			return newCol;
		}


		protected void ReadSpecificsFromHeader(BinaryReader br)
		{
			switch (TypeCategory)
			{
				case TypeCategory.Char:
					break;
				case TypeCategory.String:
					ColumnSize = br.ReadInt32();
					break;
				case TypeCategory.DateTime:
					break;
				case TypeCategory.Integer:
					break;
				case TypeCategory.Decimal:
					NumericPrecision = br.ReadInt16();
					NumericScale = br.ReadInt16();
					break;
				case TypeCategory.Float:
					break;
				case TypeCategory.Boolean:
					break;
				case TypeCategory.Guid:
					break;
				case TypeCategory.Bytes:
					break;
				default:
					throw new TraceLog.InternalException($"Unsupported type category [{TypeCategory}]");
			}
		}

		public void WriteToHeader(BinaryWriter bw)
		{
			bw.Write(Name);

			bw.Write((int)ExtendedTypeCode);
			bw.Write(IsNullable);

			switch (TypeCategory)
			{
				case TypeCategory.Char:
					break;
				case TypeCategory.String:
					bw.Write(ColumnSize);
					break;
				case TypeCategory.DateTime:
					break;
				case TypeCategory.Integer:
					break;
				case TypeCategory.Decimal:
					bw.Write(NumericPrecision);
					bw.Write(NumericScale);
					break;
				case TypeCategory.Float:
					break;
				case TypeCategory.Boolean:
					break;
				case TypeCategory.Guid:
					break;
				case TypeCategory.Bytes:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void Dump()
		{
			TraceLog.WriteLine($"Column: [{Name}]");

			TraceLog.WriteLine($"\tDataTypeName: [{DataTypeName}]");
			TraceLog.WriteLine($"\tType: [{Type}]");
			TraceLog.WriteLine($"\tTypeCode: [{ExtendedTypeCode}]");
			TraceLog.WriteLine($"\tTypeCategory: [{TypeCategory}]");
			TraceLog.WriteLine($"\tOrdinal: [{TableOrdinal}]");
			TraceLog.WriteLine($"\tIsNullable: [{IsNullable}]");
			TraceLog.WriteLine($"\tColumnSize: [{ColumnSize}]");
			TraceLog.WriteLine($"\tNumericPrecision: [{NumericPrecision}]");
			TraceLog.WriteLine($"\tNumericScale: [{NumericScale}]");
			TraceLog.WriteLine($"\tIsUnique: [{IsUnique}]");
			TraceLog.WriteLine($"\tIsIdentity: [{IsIdentity}]");
			TraceLog.WriteLine($"\tIsAutoIncrement: [{IsAutoIncrement}]");
			TraceLog.WriteLine($"\tIsLong: [{IsLong}]");
			TraceLog.WriteLine($"\tIsReadOnly: [{IsReadOnly}]");

			TraceLog.WriteLine($"\tNoInsert: [{NoInsert}]");
		}

		public void GenWriteBinary(StringBuilder sb)
		{
			var nullable = IsNullable ? "Nullable" : null;
			var comment = IsIdentity ? " [Identity]" : null;

			sb.AppendLine($"reader.WriteBinary{TypeName}{nullable}({TableOrdinal}, bw); // {Name}{comment}");
		}

		public void GenWriteDefaultValueIfNotMatched(StringBuilder sb)
		{
			if (IsMatched || NoInsert)
			{
				return;
			}

			if (IsNullable)
			{
				sb.AppendLine($"newRow[{TableOrdinal}] = DBNull.Value; // Missing {Name}");
			}
			else
			{
				sb.AppendLine($"newRow[{TableOrdinal}] = {ExtendedTypeCode.GenDefaultValue()}; // Missing {Name}");
			}
		}

		public void GenReadBinary(StringBuilder sb, DataTable destDataTable)
		{
			if (IsMatched && !MatchedColumn.NoInsert)
			{
				var nullable = IsNullable ? "Nullable" : null; //TODO Write to header
				var comment = MatchedColumn.IsIdentity ? " [Identity]" : null;

				sb.AppendLine($@"newRow.ReadBinary{MatchedColumn.TypeName}{nullable}({MatchedColumn.GetDestDataTableOrdinal(destDataTable)}, br); // {Name}{comment}");
			}
			else
			{
				if (IsNullable)
				{
					sb.AppendLine("if (br.ReadBoolean())");
					sb.AppendLine("{");
				}

				sb.AppendLine($"br.Read{TypeName}(); // Skip no-insert {Name}");

				if (IsNullable)
				{
					sb.AppendLine("}");
				}
			}
		}
	}
}
