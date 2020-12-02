using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Xml;

namespace DataExportImport
{
	public static class ExtensionsDataReader
	{
		public const int UndefinedIndex = -1;

		public static int GetColOrdinal(this SqlDataReader read, string name, bool predicate = true)
		{
			if (predicate)
			{
				return read.GetOrdinal(name);
			}

			return UndefinedIndex;
		}

		public static void Set(this SqlDataReader reader, out byte value, int idx)
		{
			value = !reader.IsDBNull(idx) ? reader.GetByte(idx) : (byte)0;
		}

		public static void Set(this SqlDataReader reader, out byte? value, int idx)
		{
			value = !reader.IsDBNull(idx) ? (byte?)reader.GetByte(idx) : null;
		}

		public static void SetFromByte(this SqlDataReader reader, out int? value, int idx)
		{
			value = !reader.IsDBNull(idx) ? (int?)reader.GetByte(idx) : null;
		}

		public static void SetFromShort(this SqlDataReader reader, out byte? value, int idx)
		{
			value = !reader.IsDBNull(idx) ? (byte?)reader.GetInt16(idx) : null;
		}

		public static void Set(this SqlDataReader reader, out bool value, int idx)
		{
			value = !reader.IsDBNull(idx) && reader.GetBoolean(idx);
		}

		public static void SetNullIf(this SqlDataReader reader, out bool? value, int idx)
		{
			value = reader[idx] != DBNull.Value ? (bool?)reader[idx] : null;
		}

		public static void Set(this SqlDataReader reader, out short value, int idx)
		{
			value = !reader.IsDBNull(idx) ? reader.GetInt16(idx) : (short)0;
		}

		public static void Set(this SqlDataReader reader, out short? value, int idx)
		{
			value = !reader.IsDBNull(idx) ? (short?)reader.GetInt16(idx) : null;
		}

		public static void Set(this SqlDataReader reader, out int value, int idx)
		{
			value = !reader.IsDBNull(idx) ? reader.GetInt32(idx) : 0;
		}

		public static void SetFromLong(this SqlDataReader reader, out int value, int idx)
		{
			value = !reader.IsDBNull(idx) ? (int)reader.GetInt64(idx) : 0;
		}

		public static void SetIf(this SqlDataReader reader, ref int value, int idx)
		{
			if (!reader.IsDBNull(idx))
				value = reader.GetInt32(idx);
		}

		public static void Set(this SqlDataReader reader, out int? value, int idx)
		{
			value = !reader.IsDBNull(idx) ? (int?)reader.GetInt32(idx) : null;
		}

		public static void Set(this SqlDataReader reader, out long value, int idx)
		{
			value = !reader.IsDBNull(idx) ? reader.GetInt64(idx) : 0L;
		}

		public static void SetIf(this SqlDataReader reader, ref long value, int idx)
		{
			if (!reader.IsDBNull(idx))
			{
				value = reader.GetInt64(idx);
			}
		}

		public static void Set(this SqlDataReader reader, out long? value, int idx)
		{
			value = !reader.IsDBNull(idx) ? (long?)reader.GetInt64(idx) : null;
		}

		public static void Set(this SqlDataReader reader, out DateTime value, int idx)
		{
			value = !reader.IsDBNull(idx) ? reader.GetDateTime(idx) : DateTime.MinValue;
		}

		public static void Set(this SqlDataReader reader, out DateTime? value, int idx)
		{
			value = !reader.IsDBNull(idx) ? (DateTime?)reader.GetDateTime(idx) : null;
		}

		public static void Set(this SqlDataReader reader, out decimal value, int idx)
		{
			value = reader.GetDecimal(idx);
		}

		public static void Set(this SqlDataReader reader, out decimal? value, int idx)
		{
			value = !reader.IsDBNull(idx) ? (decimal?)reader.GetDecimal(idx) : null;
		}

		public static void SetMaxIfNullOrMin(this SqlDataReader reader, out DateTime value, int idx)
		{
			value = !reader.IsDBNull(idx) ? reader.GetDateTime(idx) : DateTime.MaxValue;

			if (value == (DateTime)SqlDateTime.MinValue)
			{
				value = DateTime.MaxValue;
			}
		}

		public static void Set(this SqlDataReader reader, out string value, int idx)
		{
			value = !reader.IsDBNull(idx) ? reader.GetString(idx) : string.Empty;
		}

		public static void SetFromGuid(this SqlDataReader reader, out string value, int idx)
		{
			value = !reader.IsDBNull(idx) ? reader.GetGuid(idx).ToString() : string.Empty;
		}

		public static void SetNullIf(this SqlDataReader reader, out string value, int idx)
		{
			value = !reader.IsDBNull(idx) ? reader.GetString(idx) : null;
		}

		public static void Set(this SqlDataReader reader, out Guid value, int idx)
		{
			value = reader[idx] != DBNull.Value ? (Guid)reader[idx] : Guid.Empty;
		}

		public static void Set(this SqlDataReader reader, out XmlDocument value, int idx)
		{
			var xmlReader = reader.GetSqlXml(idx).CreateReader();
			value = new XmlDocument();
			if (reader[idx] != DBNull.Value)
			{
				value.Load(xmlReader);
			}
		}

		//!WRITEBINARY
		private static bool WriteBinaryNullFlag(SqlDataReader reader, int idx, BinaryWriter bw)
		{
			var isNotNull = !reader.IsDBNull(idx);
			bw.Write(isNotNull);
			return isNotNull;
		}

		public static void WriteBinaryBytes(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			var bytes = (byte[])reader[idx];
			var length = bytes.Length;

			bw.Write(length);
			bw.Write(bytes);
		}

		public static void WriteBinaryBytesNullable(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			if (WriteBinaryNullFlag(reader, idx, bw))
			{
				WriteBinaryBytes(reader, idx, bw);
			}
		}

		public static void WriteBinaryByte(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			bw.Write(reader.GetByte(idx));
		}

		public static void WriteBinaryByteNullable(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			if (WriteBinaryNullFlag(reader, idx, bw))
			{
				WriteBinaryByte(reader, idx, bw);
			}
		}

		public static void WriteBinaryBoolean(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			bw.Write(reader.GetBoolean(idx));
		}

		public static void WriteBinaryBooleanNullable(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			if (WriteBinaryNullFlag(reader, idx, bw))
			{
				WriteBinaryBoolean(reader, idx, bw);
			}
		}

		public static void WriteBinaryInt16(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			bw.Write(reader.GetInt16(idx));
		}

		public static void WriteBinaryInt16Nullable(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			if (WriteBinaryNullFlag(reader, idx, bw))
			{
				WriteBinaryInt16(reader, idx, bw);
			}
		}

		public static void WriteBinaryInt32(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			bw.Write(reader.GetInt32(idx));
		}

		public static void WriteBinaryInt32Nullable(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			if (WriteBinaryNullFlag(reader, idx, bw))
			{
				WriteBinaryInt32(reader, idx, bw);
			}
		}

		public static void WriteBinaryInt64(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			bw.Write(reader.GetInt64(idx));
		}

		public static void WriteBinaryInt64Nullable(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			if (WriteBinaryNullFlag(reader, idx, bw))
			{
				WriteBinaryInt64(reader, idx, bw);
			}
		}

		public static void WriteBinaryDateTime(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			bw.Write(reader.GetDateTime(idx).ToBinary());
		}

		public static void WriteBinaryDateTimeNullable(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			if (WriteBinaryNullFlag(reader, idx, bw))
			{
				WriteBinaryDateTime(reader, idx, bw);
			}
		}


		public static void WriteBinaryDateTimeOffset(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			var dateTimeOffset = reader.GetDateTimeOffset(idx);
			var dateTime = dateTimeOffset.DateTime;
			var minutes = (short)dateTimeOffset.Offset.TotalMinutes;

			bw.Write(dateTime.ToBinary());
			bw.Write(minutes);
		}

		public static void WriteBinaryDateTimeOffsetNullable(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			if (WriteBinaryNullFlag(reader, idx, bw))
			{
				WriteBinaryDateTimeOffset(reader, idx, bw);
			}
		}

		public static void WriteBinaryDecimal(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			bw.Write(reader.GetDecimal(idx));
		}

		public static void WriteBinaryDecimalNullable(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			if (WriteBinaryNullFlag(reader, idx, bw))
			{
				WriteBinaryDecimal(reader, idx, bw);
			}
		}

		public static void WriteBinaryString(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			bw.Write(reader.GetString(idx));
		}

		public static void WriteBinaryStringNullable(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			if (WriteBinaryNullFlag(reader, idx, bw))
			{
				WriteBinaryString(reader, idx, bw);
			}
		}

		public static void WriteBinaryGuid(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			bw.Write(reader.GetGuid(idx).ToByteArray());
		}

		public static void WriteBinaryGuidNullable(this SqlDataReader reader, int idx, BinaryWriter bw)
		{
			if (WriteBinaryNullFlag(reader, idx, bw))
			{
				WriteBinaryGuid(reader, idx, bw);
			}
		}
	}
}

