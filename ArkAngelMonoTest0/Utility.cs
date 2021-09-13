using System.Runtime.InteropServices;
using Kernys.Bson;
namespace MainNamespace
{
    public static class Utility
    {
        public static string BsonFormat(BSONObject obj, string indent_s = "")
        {
			int i = 0;
			string s = "{\n\t";
			UnhollowerBaseLib.Il2CppArrayBase<string> ikeys = new UnhollowerBaseLib.Il2CppStringArray(obj.Keys.Count);
			//UnhollowerBaseLib.Il2CppArrayBase<string> keys = obj.Keys;
			//std.vector < std.string> keys_c = Il2CppDictUtil.GetKeysStringConverted(map);
			obj.Keys.CopyTo(ikeys, 0);

			string[] keys = ikeys;
			for (i = 0; i < keys.Length; i++)
			{
				s += indent_s + keys[i] + ": ";
				BSONValue value = obj.mMap[keys[i]];// map->GetValue(keys->vector[i]);

				switch (value.valueType)
				{
					case BSONValue.ValueType.Int32:
						s += value.int32Value.ToString();
						break;
					case BSONValue.ValueType.Int64:
						s += value.int64Value.ToString();
						break;
					case BSONValue.ValueType.UTCDateTime:
						{
							s += value.dateTimeValue.ToString();
							break;
						}
					case BSONValue.ValueType.String:
						s += value.stringValue;
						break;
					case BSONValue.ValueType.Double:
						s += value.doubleValue.ToString();
						break;
					case BSONValue.ValueType.Boolean:
						s += value.boolValue.ToString();
						break;
					case BSONValue.ValueType.Object:
						{
							indent_s += "\t";
							s += BsonFormat(value.Cast<BSONObject>(), indent_s);
							indent_s = indent_s.Remove(indent_s.Length - 1, 1);
							break;
						}
					case BSONValue.ValueType.Binary:
						{
							s += "[\n\t\t";
							for (int j = 0; j < value.binaryValue.Length; j++)
							{
								s += value.binaryValue[j].ToString();
								if (j + 1 < value.binaryValue.Length)
									s += "\n\t\t";
							}
							s += "\n\t]";
							break;
						}
					case BSONValue.ValueType.Array:
						{
							//Il2CppSystem.Collections.Generic.List<string> l = value.stringListValue;
							s += $"array({value.stringListValue.Count})";
							//s += "[\n\t\t";
							//for (int j = 0; j < v.Count; j++)
							//{
							//	string vf = BsonValueFormat(v.mList[j], indent_s);
							//	if (vf.Length == 0)
							//	{
							//		break;
							//	}
							//	s += vf;
							//	if (j + 1 < v.Count && vf.Length != 0)
							//		s += ",\n\t\t";
							//}
							//s += "\n\t]";
							break;
						}
				}

				if (i + 1 < keys.Length)
					s += ",\n\t";
			}

			s += "\n" + indent_s + "}";

			return s;
		}

		public static string BsonValueFormat(BSONValue val, string indent_s = "")
        {
			string s = "";
			BSONValue value = val;// map->GetValue(keys->vector[i]);

			switch (value.valueType)
			{
				case BSONValue.ValueType.Int32:
					s += value.int32Value.ToString();
					break;
				case BSONValue.ValueType.Int64:
					s += value.int64Value.ToString();
					break;
				case BSONValue.ValueType.UTCDateTime:
					{
						s += value.dateTimeValue.ToString();
						break;
					}
				case BSONValue.ValueType.String:
					s += value.stringValue;
					break;
				case BSONValue.ValueType.Double:
					s += value.doubleValue.ToString();
					break;
				case BSONValue.ValueType.Boolean:
					s += value.boolValue.ToString();
					break;
				case BSONValue.ValueType.Object:
					{
						indent_s += "\t";
						s += BsonFormat(value.Cast<BSONObject>(), indent_s);
						indent_s = indent_s.Remove(indent_s.Length - 1, 1);
						break;
					}
				case BSONValue.ValueType.Binary:
					{
						s += "[\n\t\t";
						for (int j = 0; j < value.binaryValue.Length; j++)
						{
							s += value.binaryValue[j].ToString();
							if (j + 1 < value.binaryValue.Length)
								s += "\n\t\t";
						}
						s += "\n\t]";
						break;
					}
				case BSONValue.ValueType.Array:
					{
						//Il2CppSystem.Collections.Generic.List<string> l = value.stringListValue;
						Il2CppSystem.Collections.Generic.List<BSONValue> v = value.Cast<BSONArray>().mList;

						s += "[\n\t\t";
						for (int j = 0; j < v.Count; j++)
						{
							string vf = BsonValueFormat(v[j], indent_s);
							if (vf.Length == 0)
							{
								break;
							}
							s += vf;
							if (j + 1 < v.Count && vf.Length != 0)
								s += ",\n\t\t";
						}
						s += "\n\t]";
						break;
					}
			
			}

			return s;
		}
    }
}

