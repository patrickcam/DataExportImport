
using System;

namespace DataExportImport
{
	public enum ExtendedTypeCode
	{
		Empty = TypeCode.Empty,
		Object = TypeCode.Object,
		DBNull = TypeCode.DBNull,
		Boolean = TypeCode.Boolean,
		Char = TypeCode.Char,
		SByte = TypeCode.SByte,
		Byte = TypeCode.Byte,
		Int16 = TypeCode.Int16,
		UInt16 = TypeCode.UInt16,
		Int32 = TypeCode.Int32,
		UInt32 = TypeCode.UInt32,
		Int64 = TypeCode.Int64,
		UInt64 = TypeCode.UInt64,
		Single = TypeCode.Single,
		Double = TypeCode.Double,
		Decimal = TypeCode.Decimal,
		DateTime = TypeCode.DateTime,
		String = TypeCode.String,
		Guid,
		DateTimeOffset,
		Bytes
	}

	public enum TypeCategory
	{
		Char,
		String,
		DateTime,
		Integer,
		Decimal,
		Float,
		Boolean,
		Guid,
		Bytes
	}

	internal static class TypeCategoryExtensions
	{
		internal static ExtendedTypeCode GetExtendedTypeCode(this Type type)
		{
			var typeCode = Type.GetTypeCode(type);
			switch (typeCode)
			{
				case TypeCode.Object:
					switch (type.Name)
					{
						case "Guid":
							return ExtendedTypeCode.Guid;
						case "DateTimeOffset":
							return ExtendedTypeCode.DateTimeOffset;
						case "Byte[]":
							return ExtendedTypeCode.Bytes;
					}

					break;
				case TypeCode.Boolean: return ExtendedTypeCode.Boolean;
				case TypeCode.Char: return ExtendedTypeCode.Char;
				case TypeCode.SByte: return ExtendedTypeCode.SByte;
				case TypeCode.Byte: return ExtendedTypeCode.Byte;
				case TypeCode.Int16: return ExtendedTypeCode.Int16;
				case TypeCode.UInt16: return ExtendedTypeCode.UInt16;
				case TypeCode.Int32: return ExtendedTypeCode.Int32;
				case TypeCode.UInt32: return ExtendedTypeCode.UInt32;
				case TypeCode.Int64: return ExtendedTypeCode.Int64;
				case TypeCode.UInt64: return ExtendedTypeCode.UInt64;
				case TypeCode.Single: return ExtendedTypeCode.Single;
				case TypeCode.Double: return ExtendedTypeCode.Double;
				case TypeCode.Decimal: return ExtendedTypeCode.Decimal;
				case TypeCode.DateTime: return ExtendedTypeCode.DateTime;
				case TypeCode.String: return ExtendedTypeCode.String;
			}

			throw new TraceLog.InternalException($"Unsupported type [{type.Name}]");
		}

		internal static TypeCategory GetTypeCategory(this ExtendedTypeCode extendedTypeCode)
		{
			switch (extendedTypeCode)
			{
				case ExtendedTypeCode.Boolean: return TypeCategory.Boolean;
				case ExtendedTypeCode.Char: return TypeCategory.Char;
				case ExtendedTypeCode.SByte:
				case ExtendedTypeCode.Byte:
				case ExtendedTypeCode.Int16:
				case ExtendedTypeCode.UInt16:
				case ExtendedTypeCode.Int32:
				case ExtendedTypeCode.UInt32:
				case ExtendedTypeCode.Int64:
				case ExtendedTypeCode.UInt64: return TypeCategory.Integer;
				case ExtendedTypeCode.Single:
				case ExtendedTypeCode.Double: return TypeCategory.Float;
				case ExtendedTypeCode.Decimal: return TypeCategory.Decimal;
				case ExtendedTypeCode.DateTimeOffset:
				case ExtendedTypeCode.DateTime: return TypeCategory.DateTime;
				case ExtendedTypeCode.String: return TypeCategory.String;
				case ExtendedTypeCode.Guid: return TypeCategory.Guid;
				case ExtendedTypeCode.Bytes: return TypeCategory.Bytes;
			}

			throw new TraceLog.InternalException($"Unsupported type code [{extendedTypeCode}]");
		}

		internal static Type GetTypeFromExtendedTypeCode(this ExtendedTypeCode extendedTypeCode)
		{
			switch (extendedTypeCode)
			{
				case ExtendedTypeCode.Boolean: return typeof(Boolean);
				case ExtendedTypeCode.Char: return typeof(Char);
				case ExtendedTypeCode.SByte: return typeof(SByte);
				case ExtendedTypeCode.Byte: return typeof(Byte);
				case ExtendedTypeCode.Int16: return typeof(Int16);
				case ExtendedTypeCode.UInt16: return typeof(UInt16);
				case ExtendedTypeCode.Int32: return typeof(Int32);
				case ExtendedTypeCode.UInt32: return typeof(UInt32);
				case ExtendedTypeCode.Int64: return typeof(Int64);
				case ExtendedTypeCode.UInt64: return typeof(UInt64);
				case ExtendedTypeCode.Single: return typeof(Single);
				case ExtendedTypeCode.Double: return typeof(Double);
				case ExtendedTypeCode.Decimal: return typeof(Decimal);
				case ExtendedTypeCode.DateTime: return typeof(DateTime);
				case ExtendedTypeCode.DateTimeOffset: return typeof(DateTimeOffset);
				case ExtendedTypeCode.String: return typeof(String);
				case ExtendedTypeCode.Guid: return typeof(Guid);
				case ExtendedTypeCode.Bytes: return typeof(Byte[]);
			}

			throw new TraceLog.InternalException($"Unsupported typecode [{extendedTypeCode}]");
		}

		internal static string GenDefaultValue(this ExtendedTypeCode typeCode)
		{
			switch (typeCode)
			{
				case ExtendedTypeCode.Boolean: return "false";
				case ExtendedTypeCode.Char: return "(char)0";
				case ExtendedTypeCode.SByte: return "(SByte)0";
				case ExtendedTypeCode.Byte: return "(byte)0";
				case ExtendedTypeCode.Int16: return "(short)0";
				case ExtendedTypeCode.UInt16: return "(UInt16)0";
				case ExtendedTypeCode.Int32: return "0";
				case ExtendedTypeCode.UInt32: return "0U";
				case ExtendedTypeCode.Int64: return "0L";
				case ExtendedTypeCode.UInt64: return "0UL";
				case ExtendedTypeCode.Single: return "0F";
				case ExtendedTypeCode.Double: return "0D";
				case ExtendedTypeCode.Decimal: return "0M";
				case ExtendedTypeCode.DateTime: return "DateTime.MinVal";
				case ExtendedTypeCode.String: return "string.Empty";
				case ExtendedTypeCode.DateTimeOffset: return "DateTimeOffset.MinVal";
				case ExtendedTypeCode.Bytes: return "new byte[0]";
			}

			throw new TraceLog.InternalException($"Unsupported typecode [{typeCode}]");
		}
	}
}