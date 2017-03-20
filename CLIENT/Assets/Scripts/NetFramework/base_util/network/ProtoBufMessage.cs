#define PROTOBUF_OPTIMIZE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace BaseUtil
{
    public sealed class ProtoBufAttribute : System.Attribute
    {
        private int m_index;
        public int Index { get { return m_index; } set { m_index = value; } }
        public ProtoBufAttribute()
        {
            m_index = -1;
        }
    }

    internal class ProtoBufSerializer
    {
        protected enum WireType
        {
            LengthDelimited = 0,
            VarInt = 1,
            Data32 = 2,
            Data64 = 3,
            //Data8 =  4,
            //Data16 = 5,
        }

        private const ushort InheritFlag = 0xFEFE;

        // -------------- begin var int ---------------------
        private static bool USE_VAR_INT32 = true;
        private static bool USE_VAR_INT64 = true;

        public static void to_var_int(long n, NetOutStream outs)
        {
            ulong v = (ulong)((n << 1) ^ (n >> 63)); //zigzag
            for (; ; )
            {
                byte abyte = (byte)(v & 0x7f);
                v = v >> 7;
                if (v == 0)
                {
                    outs.Write(abyte);
                    break;
                }
                else
                {
                    abyte |= 0x80;
                    outs.Write(abyte);
                }
            }
        }

        public static long from_var_int(NetInStream ins)
        {
            long n = 0;
            int shift = 0;
            for (; ; )
            {
                byte abyte = 0;
                ins.Read(ref abyte);
                long bvar = ((long)abyte) & 0x7f;
                n += bvar << shift;
                shift += 7;
                if ((abyte & 0x80) == 0)
                {
                    break;
                }
            }

            return (long)(((ulong)n >> 1) ^ (ulong)(-(n & 1)));//unzigzag
        }
        // --------------  end var int ----------------------

        static ushort make_field_desc(int fsindex, WireType wire_type)
        {
            ushort desc = (ushort)((fsindex << 3) | (int)wire_type);
            return desc;
        }
        static int parse_field_index(ushort desc)
        {
            return (ushort)(desc >> 3);
        }
        static WireType parse_wire_type(ushort desc)
        {
            return (WireType)(desc & 0x0007);
        }
        static void save_field_desc(int fsindex, WireType wire_type, NetOutStream outs)
        {
            if (fsindex != -1)
            {
                ushort desc = make_field_desc(fsindex, wire_type);
                outs.Write(desc);
            }
        }

        static void save_protobuf_lenth_delimited(int fsindex, NetBuffer buff, NetOutStream outs)
        {
            save_field_desc(fsindex, WireType.LengthDelimited, outs);
            outs.Write(buff);
        }

        static void save_protobuf_string(int fsindex, string val, NetOutStream outs)
        {
            save_field_desc(fsindex, WireType.LengthDelimited, outs);

            int len_offset = outs.Offset;
            outs.Seek(len_offset + 4);

            outs.Write(val, false);

            outs.Seek(len_offset);
            outs.Write(outs.GetBuffer().length - len_offset - 4);
            outs.Seek(outs.GetBuffer().length);
        }

        static void save_protobuf_array(int fsindex, byte[] val, NetOutStream outs)
        {
            save_field_desc(fsindex, WireType.LengthDelimited, outs);

            int len_offset = outs.Offset;
            outs.Seek(len_offset + 4);

            outs.Write(val, false);

            outs.Seek(len_offset);
            outs.Write(outs.GetBuffer().length - len_offset - 4);
            outs.Seek(outs.GetBuffer().length);
        }

        static void save_protobuf_buff(int fsindex, NetBuffer val, NetOutStream outs)
        {
            save_field_desc(fsindex, WireType.LengthDelimited, outs);

            int len_offset = outs.Offset;
            outs.Seek(len_offset + 4);

            outs.Write(val, false);

            outs.Seek(len_offset);
            outs.Write(outs.GetBuffer().length - len_offset - 4);
            outs.Seek(outs.GetBuffer().length);
        }

        static void save_protobuf_int32(int fsindex, int val, NetOutStream outs, WireType wire_type)
        {
            if (wire_type == WireType.VarInt)
            {
                save_field_desc(fsindex, WireType.VarInt, outs);
                to_var_int(val, outs);
            }
            else
            {
                save_field_desc(fsindex, WireType.Data32, outs);
                outs.Write(val);
            }
        }

        static void save_protobuf_int64(int fsindex, long val, NetOutStream outs, WireType wire_type)
        {
            if (wire_type == WireType.VarInt)
            {
                save_field_desc(fsindex, WireType.VarInt, outs);
                to_var_int(val, outs);
            }
            else
            {
                save_field_desc(fsindex, WireType.Data64, outs);
                outs.Write(val);
            }
        }

        static void save_protobuf_float32(int fsindex, float val, NetOutStream outs)
        {
            save_field_desc(fsindex, WireType.Data32, outs);
            outs.Write(val);
        }

        static void save_protobuf_float64(int fsindex, double val, NetOutStream outs)
        {
            save_field_desc(fsindex, WireType.Data64, outs);
            outs.Write(val);
        }

        static void save_type(Type ft, NetOutStream outs)
        {
            ValueType vt = is_tss_type(ft) ? get_tss_value_type(ft) : get_value_type(ft);
            WireType wt = get_wire_type(vt);
            byte w = (byte)wt;
            outs.Write(w);
        }
        static WireType read_type(NetInStream ins)
        {
            byte w = 0;
            ins.Read(ref w);
            return (WireType)w;
        }

        static void save_protobuf_list(int fsindex, object listobj, NetOutStream outs)
        {
            IList list = listobj as IList;
            if (null == list) return;

            save_field_desc(fsindex, WireType.LengthDelimited, outs);

            int len_offset = outs.Offset;
            outs.Seek(len_offset + 4);

            int count = list.Count;
            outs.Write(count);
            if (count > 0)
            {
                save_type(list[0].GetType(), outs);
                foreach (var item in list)
                {
                    _encode_item_to_stream(-1, item, outs);
                }
            }

            outs.Seek(len_offset);
            outs.Write(outs.GetBuffer().length - len_offset - 4);
            outs.Seek(outs.GetBuffer().length);
        }
        static void load_protobuf_list(Type item_type, ref object listobj, NetInStream ins)
        {
            NetInStream tempins = load_protobuf_lenth_delimited(ins);
            IList list = listobj as IList;
            if (null == list) return;
            list.Clear();

            int count = 0;
            tempins.Read(ref count);
            if (count > 0)
            {
                WireType wt = read_type(tempins);
                for (int i = 0; i < count; ++i)
                {
                    object obj = DecodeItemRaw(item_type, wt, tempins);
                    list.Add(obj);
                }
            }
        }
        static void save_protobuf_dictionary(int fsindex, object dictobj, NetOutStream outs)
        {

            IDictionary dict = dictobj as IDictionary;
            if (null == dict) return;

            save_field_desc(fsindex, WireType.LengthDelimited, outs);

            int len_offset = outs.Offset;
            outs.Seek(len_offset + 4);

            int count = dict.Count;
            outs.Write(count);
            if (count > 0)
            {
                bool saveindex = true;
                foreach (var key in dict.Keys)
                {
                    if (saveindex)
                    {
                        save_type(key.GetType(), outs);
                        save_type(dict[key].GetType(), outs);
                        saveindex = false;
                    }

                    _encode_item_to_stream(-1, key, outs);
                    _encode_item_to_stream(-1, dict[key], outs);
                }
            }

            outs.Seek(len_offset);
            outs.Write(outs.GetBuffer().length - len_offset - 4);
            outs.Seek(outs.GetBuffer().length);
        }
        static void save_protobuf_hashset(int fsindex, object hashsetobj, NetOutStream outs)
        {
            IEnumerable enumerable = hashsetobj as IEnumerable;
            PropertyInfo prop = hashsetobj.GetType().GetProperty("Count");
            if((null == enumerable) || (null == prop)) return;

            save_field_desc(fsindex, WireType.LengthDelimited, outs);

            int len_offset = outs.Offset;
            outs.Seek(len_offset + 4);

            int count = (int)prop.GetValue(hashsetobj, null);//.Count;
            outs.Write(count);
            if (count > 0)
            {
                int index = 0;
                IEnumerator enumerator = enumerable.GetEnumerator();
                while( enumerator.MoveNext() )
                {
                    var elem = enumerator.Current;
                    if (index == 0) save_type(elem.GetType(), outs);
                    _encode_item_to_stream(-1, elem, outs);
                    ++index;
                }
            }

            outs.Seek(len_offset);
            outs.Write(outs.GetBuffer().length - len_offset - 4);
            outs.Seek(outs.GetBuffer().length);
        }
        static void load_protobuf_hashset(Type ft, Type elem_type, ref object hashsetobj, NetInStream ins)
        {
            NetInStream tempins = load_protobuf_lenth_delimited(ins);
            MethodInfo Add = ft.GetMethod("Add");            

            int count = 0;
            tempins.Read(ref count);
            if(count > 0)
            {
                WireType wt = read_type(tempins);
                for(int i = 0; i < count; ++i)
                {
                    object elem = DecodeItemRaw(elem_type, wt, tempins);
                    Add.Invoke(hashsetobj, new object[]{elem});
                }
            }
        }
        static void load_protobuf_dictionary(Type key_type, Type value_type, ref object dictobj, NetInStream ins)
        {
            NetInStream tempins = load_protobuf_lenth_delimited(ins);
            IDictionary dict = dictobj as IDictionary;
            if (null == dict) return;

            dict.Clear();

            int count = 0;
            tempins.Read(ref count);
            if (count > 0)
            {
                WireType wt_k = read_type(tempins);
                WireType wt_v = read_type(tempins);

                for (int i = 0; i < count; ++i)
                {
                    object key = DecodeItemRaw(key_type, wt_k, tempins);
                    object value = DecodeItemRaw(value_type, wt_v, tempins);
                    dict.Add(key, value);
                }
            }
        }
        static NetInStream load_protobuf_lenth_delimited(NetInStream ins)
        {
            int length = 0;
            ins.Read(ref length);
            int start = ins.Offset;
            ins.Skip(length);

            NetBuffer buff = new NetBuffer(ins.GetBuffer().buffer);
            buff.SetLength(ins.Offset);
            NetInStream tempins = new NetInStream(buff);
            tempins.Skip(start);
            return tempins;
        }
        static object load_protobuf_string(NetInStream ins, WireType wire_type)
        {
            if (wire_type == WireType.LengthDelimited)
            {
                string val = "";
                NetInStream tempins = load_protobuf_lenth_delimited(ins);

                tempins.Read(ref val, tempins.BytesLeft() - 1);
                return val;
            }
            return null;
        }
        static object load_protobuf_array(NetInStream ins, WireType wire_type)
        {
            if (wire_type == WireType.LengthDelimited)
            {
                byte[] val = new byte[0];
                NetInStream tempins = load_protobuf_lenth_delimited(ins);

                tempins.Read(ref val, tempins.BytesLeft());
                return val;
            }
            return null;
        }
        static object load_protobuf_buff(NetInStream ins, WireType wire_type)
        {
            if (wire_type == WireType.LengthDelimited)
            {
                NetBuffer val = new NetBuffer();
                NetInStream tempins = load_protobuf_lenth_delimited(ins);

                tempins.Read(ref val, tempins.BytesLeft());
                return val;
            }
            return null;
        }
        static void load_protobuf_compound(object obj, NetInStream ins)
        {
            NetInStream tempins = load_protobuf_lenth_delimited(ins);

            List<FieldInfo[]> fields_list = GetProtobufFields(obj.GetType());
            int fields_index = 0;

            while (tempins.BytesLeft() >= 2 && fields_index < fields_list.Count)
            {
                ushort field_desc = 0;
                tempins.Read(ref field_desc);

                if (field_desc == InheritFlag)
                {
                    fields_index++;
                    continue;
                }

                int fsindex = parse_field_index(field_desc);
                WireType wire_type = parse_wire_type(field_desc);
                bool res = DecodeItem(obj, fields_list[fields_index], wire_type, fsindex, tempins);
                if (!res)
                {
                    skip_buffer(wire_type, tempins);
                    LogWrapper.LogWarning("LoadItemFromStream " + obj.GetType().Name + "[" + fsindex + "] skiped!");
                }
                else
                {
                    //LogWrapper.LogInfo("LoadItemFromStream " + fsindex + " ok");
                }
            }
        }

        static object load_protobuf_int32(NetInStream ins, WireType wire_type)
        {
            if (wire_type == WireType.VarInt)
            {
                long v = from_var_int(ins);
                return (int)v;
            }
            else if (wire_type == WireType.Data32)
            {
                int val = 0;
                ins.Read(ref val);
                return val;
            }
            return null;
        }
        static object load_protobuf_int64(NetInStream ins, WireType wire_type)
        {
            long val = 0;
            if (wire_type == WireType.VarInt)
            {
                val = from_var_int(ins);
                return val;
            }
            else if (wire_type == WireType.Data64)
            {
                ins.Read(ref val);
                return val;
            }
            return null;
        }
        static object load_protobuf_float32(NetInStream ins, WireType wire_type)
        {
            float val = 0f;
            if (wire_type == WireType.Data32)
            {
                ins.Read(ref val);
                return val;
            }
            return null;
        }
        static object load_protobuf_float64(NetInStream ins, WireType wire_type)
        {
            double val = 0f;
            if (wire_type == WireType.Data64)
            {
                ins.Read(ref val);
                return val;
            }
            return null;
        }

        static void skip_buffer(WireType wire_type, NetInStream ins)
        {
            int len = 0;
            switch (wire_type)
            {
                case WireType.LengthDelimited:
                    ins.Read(ref len);
                    ins.Skip(len);
                    break;
                case WireType.VarInt:
                    from_var_int(ins);
                    //ins.Skip(4);//这里需要等实际实现后改，现在没用到。
                    break;
                case WireType.Data32:
                    ins.Skip(4);
                    break;
                case WireType.Data64:
                    ins.Skip(8);
                    break;
                default:
                    break;
            }
        }
        static void save_protobuf_compound(int fsindex, object obj, NetOutStream outs)
        {
            
            save_field_desc(fsindex, WireType.LengthDelimited, outs);

            int len_offset = outs.Offset;
            outs.Seek(len_offset + 4);

            _encode_compound_to_stream(obj, outs);

            outs.Seek(len_offset);
            outs.Write(outs.GetBuffer().length - len_offset - 4);
            outs.Seek(outs.GetBuffer().length);
        }
        static void _encode_compound_to_stream(object obj, NetOutStream outs)
        {

            Type type = obj.GetType();
            List<FieldInfo[]> fields_list = GetProtobufFields(type);
            int fields_index = 0;

            while (fields_index < fields_list.Count)
            {
                for (int index = 0; index < fields_list[fields_index].Length; ++index)
                {
                    FieldInfo field = fields_list[fields_index][index];
                    if (field == null)
                        continue;
                    object val = field.GetValue(obj);
                    if (val != null)
                    {
                        _encode_item_to_stream(index, val, outs);
                    }
                    else
                    {
                        LogWrapper.LogError("value is null: " + type.Name + "." + field.Name);
                    }
                }

                if (++fields_index < fields_list.Count)
                {
                    outs.Write(InheritFlag);
                }
            }
        }
        protected enum ValueType
        {
            NotSupported,
            Int32,
            Int64,
            Float32,
            Float64,
            String,
            ByteArray,
            NetBuffer,
            Time64,
            List,
            Dictionary,
            Compound,
            HashSet,
        }

        static bool is_tss_type(Type ft)
        {
            if(ft.IsPrimitive)
            {
                return false;
            }
            return get_tss_value_type(ft) != ValueType.NotSupported;
        }
        static ValueType get_tss_value_type(Type ft)
        {
            if (ft == typeof(TssSdtByte)
                || ft == typeof(TssSdtShort)
                || ft == typeof(TssSdtUshort)
                || ft == typeof(TssSdtInt)
                || ft == typeof(TssSdtUint)
                )
            {
                return ValueType.Int32;
            }
            else if (ft == typeof(TssSdtLong)
                || ft == typeof(TssSdtUlong)
                )
            {
                return ValueType.Int64;
            }
            else if (ft == typeof(TssSdtFloat))
            {
                return ValueType.Float32;
            }
            else if (ft == typeof(TssSdtDouble))
            {
                return ValueType.Float64;
            }
            return ValueType.NotSupported;
        }

        #if CONSOLE_CLIENT
        [ThreadStatic] static Dictionary<Type, ValueType> value_type_cache = null;
        #else
        static Dictionary<Type, ValueType> value_type_cache = null;
        #endif
        static void ensure_value_type_cache_valid()
        {
            if (null != value_type_cache) return;
            value_type_cache = new Dictionary<Type, ValueType>();
        }
        static ValueType get_value_type(Type ft)
        {
            ensure_value_type_cache_valid();
            if (value_type_cache.ContainsKey(ft))
            {
                return value_type_cache[ft];
            }

            if (ft == typeof(int)
                || ft == typeof(uint)
                || ft == typeof(byte)
                || ft == typeof(sbyte)
                || ft == typeof(short)
                || ft == typeof(ushort)
                || ft == typeof(bool)
                )
            {
                value_type_cache[ft] = ValueType.Int32;
                return ValueType.Int32;
            }
            else if (ft == typeof(char))
            {
                throw new NotImplementedException("char not supported");
            }
            else if (ft == typeof(long)
                || ft == typeof(ulong))
            {
                value_type_cache[ft] = ValueType.Int64;
                return ValueType.Int64;
            }
            else if (ft == typeof(float))
            {
                value_type_cache[ft] = ValueType.Float32;
                return ValueType.Float32;
            }
            else if (ft == typeof(double))
            {
                value_type_cache[ft] = ValueType.Float64;
                return ValueType.Float64;
            }
            else if (ft == typeof(string))
            {
                value_type_cache[ft] = ValueType.String;
                return ValueType.String;
            }
            else if (ft == typeof(byte[]))
            {
                value_type_cache[ft] = ValueType.ByteArray;
                return ValueType.ByteArray;
            }
            else if (ft == typeof(NetBuffer))
            {
                value_type_cache[ft] = ValueType.NetBuffer;
                return ValueType.NetBuffer;
            }
            else if (ft == typeof(time_t))
            {
                value_type_cache[ft] = ValueType.Time64;
                return ValueType.Time64;
            }
            else if (ft.IsGenericType)
            {
                if (null != ft.GetInterface("IList"))
                {
                    value_type_cache[ft] = ValueType.List;
                    return ValueType.List;
                }
                else if (null != ft.GetInterface("IDictionary"))
                {
                    value_type_cache[ft] = ValueType.Dictionary;
                    return ValueType.Dictionary;
                }
                else if (ft.Name == "HashSet`1")
                {
                    value_type_cache[ft] = ValueType.HashSet;
                    return ValueType.HashSet;
                }
                else if (ft.Name == "LinkedList`1")
                {
                    throw new NotImplementedException("LinkedList not supported");
                }
            }
            value_type_cache[ft] = ValueType.Compound;
            return ValueType.Compound;
        }
        static WireType get_wire_type(ValueType vt)
        {
            if (vt == ValueType.Int32)
            {
                if (USE_VAR_INT32)
                    return WireType.VarInt;
                else
                    return WireType.Data32;
            }
            else if (vt == ValueType.Int64
                || vt == ValueType.Time64
                )
            {
                if (USE_VAR_INT64)
                    return WireType.VarInt;
                else
                    return WireType.Data64;
            }
            else if (vt == ValueType.Float32)
            {
                return WireType.Data32;
            }
            else if (vt == ValueType.Float64)
            {
                return WireType.Data64;
            }
            else
            {
                return WireType.LengthDelimited;
            }
        }

        static void _encode_tss_item_to_stream(int index, object val, NetOutStream outs)
        {
            Type ft = val.GetType();

            if (ft == typeof(TssSdtByte))
            {
                save_protobuf_int32(index, (int)(val as TssSdtByte), outs, WireType.Data32);
            }
            else if (ft == typeof(TssSdtShort))
            {
                save_protobuf_int32(index, (int)(val as TssSdtShort), outs, WireType.Data32);
            }
            else if (ft == typeof(TssSdtUshort))
            {
                save_protobuf_int32(index, (int)(val as TssSdtUshort), outs, WireType.Data32);
            }
            else if (ft == typeof(TssSdtInt))
            {
                save_protobuf_int32(index, (int)(val as TssSdtInt), outs, WireType.Data32);
            }
            else if (ft == typeof(TssSdtUint))
            {
                save_protobuf_int32(index, (int)(uint)(val as TssSdtUint), outs, WireType.Data32);
            }
            else if (ft == typeof(TssSdtLong))
            {
                save_protobuf_int64(index, (long)(val as TssSdtLong), outs, WireType.Data64);
            }
            else if (ft == typeof(TssSdtUlong))
            {
                save_protobuf_int64(index, (int)(ulong)(val as TssSdtUlong), outs, WireType.Data64);
            }
            else if (ft == typeof(TssSdtFloat))
            {
                save_protobuf_float32(index, (float)(val as TssSdtFloat), outs);
            }
            else if (ft == typeof(TssSdtDouble))
            {
                save_protobuf_float64(index, (double)(val as TssSdtDouble), outs);
            }
        }

        static void _encode_item_to_stream(int index, object val, NetOutStream outs)
        {
            Type ft = val.GetType();
            if (is_tss_type(ft))
            {
                _encode_tss_item_to_stream(index, val, outs);
                return;
            }
            ValueType vt = get_value_type(ft);
            WireType wt = get_wire_type(vt);

            if (vt == ValueType.Int32)
            {
                int intval = Convert.ToInt32(val);
                save_protobuf_int32(index, intval, outs, wt);
            }
            else if (vt == ValueType.Int64)
            {
                save_protobuf_int64(index, (long)val, outs, wt);
            }
            else if (vt == ValueType.Float32)
            {
                save_protobuf_float32(index, (float)val, outs);
            }
            else if (vt == ValueType.Float64)
            {
                save_protobuf_float64(index, (double)val, outs);
            }
            else if (vt == ValueType.String)
            {
                save_protobuf_string(index, (string)val, outs);
            }
            else if (vt == ValueType.ByteArray)
            {
                save_protobuf_array(index, (byte[])val, outs);
            }
            else if (vt == ValueType.NetBuffer)
            {
                save_protobuf_buff(index, (NetBuffer)val, outs);
            }
            else if (vt == ValueType.Time64)
            {
                time_t time = (time_t)val;
                save_protobuf_int64(index, time.AsLong(), outs, wt);
            }
            else if (vt == ValueType.List)
            {
                save_protobuf_list(index, val, outs);
            }
            else if (vt == ValueType.Dictionary)
            {
                save_protobuf_dictionary(index, val, outs);
            }
            else if(vt == ValueType.HashSet)
            {
                save_protobuf_hashset(index, val, outs);
            }
            else
            {
                save_protobuf_compound(index, val, outs);
            }
        }

        private static object DecodeTssItemRaw(Type ft, WireType wire_type, NetInStream ins)
        {
            if (ft == typeof(TssSdtByte))
            {
                object val = load_protobuf_int32(ins, wire_type);
                if (val != null)
                {
                    return (TssSdtByte)(int)val;
                }
            }
            else if (ft == typeof(TssSdtShort))
            {
                object val = load_protobuf_int32(ins, wire_type);
                if (val != null)
                {
                    return (TssSdtShort)(int)val;
                }
            }
            else if (ft == typeof(TssSdtUshort))
            {
                object val = load_protobuf_int32(ins, wire_type);
                if (val != null)
                {
                    return (TssSdtUshort)(int)val;
                }
            }
            else if (ft == typeof(TssSdtInt))
            {
                object val = load_protobuf_int32(ins, wire_type);
                if (val != null)
                {
                    return (TssSdtInt)(int)val;
                }
            }
            else if (ft == typeof(TssSdtUint))
            {
                object val = load_protobuf_int32(ins, wire_type);
                if (val != null)
                {
                    return (TssSdtUint)(uint)(int)val;
                }
            }
            else if (ft == typeof(TssSdtLong))
            {
                object val = load_protobuf_int64(ins, wire_type);
                if (val != null)
                {
                    return (TssSdtLong)(long)val;
                }
            }
            else if (ft == typeof(TssSdtUlong))
            {
                object val = load_protobuf_int64(ins, wire_type);
                if (val != null)
                {
                    return (TssSdtUlong)(ulong)(long)val;
                }
            }
            else if (ft == typeof(TssSdtFloat))
            {
                object val = load_protobuf_float32(ins, wire_type);
                if (val != null)
                {
                    return (TssSdtFloat)(float)val;
                }
            }
            else if (ft == typeof(TssSdtDouble))
            {
                object val = load_protobuf_float64(ins, wire_type);
                if (val != null)
                {
                    return (TssSdtDouble)(double)val;
                }
            }
            LogWrapper.LogWarning("type does not match " + ft + " vs " + wire_type);
            return null;
        }

        private static object DecodeItemRaw(Type ft, WireType wire_type, NetInStream ins)
        {
            if (is_tss_type(ft))
            {
                return DecodeTssItemRaw(ft, wire_type, ins);
            }
            ValueType vt = get_value_type(ft);

            object ret = null;
            if (vt == ValueType.Int32)
            {
                ret = load_protobuf_int32(ins, wire_type);
            }
            else if (vt == ValueType.Int64)
            {
                ret = load_protobuf_int64(ins, wire_type);
            }
            else if (vt == ValueType.Float32)
            {
                ret = load_protobuf_float32(ins, wire_type);
            }
            else if (vt == ValueType.Float64)
            {
                ret = load_protobuf_float64(ins, wire_type);
            }
            else if (vt == ValueType.Time64)
            {
                ret = load_protobuf_int64(ins, wire_type);
                if (ret != null)
                    ret = new time_t(Convert.ToInt64(ret));
            }
            else if (vt == ValueType.String)
            {
                ret = load_protobuf_string(ins, wire_type);
            }
            else if (vt == ValueType.ByteArray)
            {
                ret = load_protobuf_array(ins, wire_type);
            }
            else if (vt == ValueType.NetBuffer)
            {
                ret = load_protobuf_buff(ins, wire_type);
            }
            else if (vt == ValueType.List)
            {
                ret = System.Activator.CreateInstance(ft);
                Type[] args = ft.GetGenericArguments();
                load_protobuf_list(args[0], ref ret, ins);
            }
            else if (vt == ValueType.Dictionary)
            {
                ret = System.Activator.CreateInstance(ft);
                Type[] args = ft.GetGenericArguments();
                load_protobuf_dictionary(args[0], args[1], ref ret, ins);
            }
            else if(vt == ValueType.HashSet)
            {
                ret = System.Activator.CreateInstance(ft);
                Type[] args = ft.GetGenericArguments();
                load_protobuf_hashset(ft, args[0], ref ret, ins);
            }
            else if (vt == ValueType.Compound)
            {
                //LogWrapper.LogInfo("loading " + ft.Name);
                ret = System.Activator.CreateInstance(ft);
                load_protobuf_compound(ret, ins);
            }

            if (ret != null)
            {
                return Convert.ChangeType(ret, ft);
            }
            LogWrapper.LogWarning("type does not match " + ft + " vs " + wire_type);
            return null;
        }

        // 要求ProtoBufAttribute索引>=0，可以是不连续的
        [ThreadStatic]
        private static Dictionary<Type, List<FieldInfo[]>> _ProtoBufFieldCache = null;
        private static Dictionary<Type, List<FieldInfo[]>> ProtoBufFieldCache
        {
            get
            {
                if (null == _ProtoBufFieldCache)
                {
                    _ProtoBufFieldCache = new Dictionary<Type, List<FieldInfo[]>>();
                }
                return _ProtoBufFieldCache;
            }
        }
        private static List<FieldInfo[]> GetProtobufFields(Type type)
        {
            List<FieldInfo[]> cached_fields;
            ProtoBufFieldCache.TryGetValue(type, out cached_fields);
            if (cached_fields != null)
                return cached_fields;

            cached_fields = new List<FieldInfo[]>();
            Type curr_type = type;
            while ((curr_type != null) && (curr_type.FullName != "System.ValueType") && (curr_type.FullName != "System.Object"))
            {
                FieldInfo[] field_list = curr_type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
                Dictionary<int, FieldInfo> fields_dict = new Dictionary<int, FieldInfo>();
                int max_index = 0;
                foreach (var field in field_list)
                {
                    object[] fieldattrs = field.GetCustomAttributes(false);
                    foreach (ProtoBufAttribute pa in fieldattrs)
                    {
                        fields_dict.Add(pa.Index, field);
                        if (pa.Index > max_index)
                            max_index = pa.Index;
                    }
                }

                //if (fields_dict.Count == 0)
                //{
                //    curr_type = curr_type.BaseType;
                //    continue; 
                //    //break;
                //}

                var fields_array = new FieldInfo[max_index + 1];
                foreach (var pf in fields_dict)
                {
                    fields_array.SetValue(pf.Value, pf.Key);
                }

                cached_fields.Add(fields_array);
                curr_type = curr_type.BaseType;
            }
            //加入缓存
            ProtoBufFieldCache.Add(type, cached_fields);
            return cached_fields;
        }

        private static bool DecodeItem(object obj, FieldInfo[] fields, WireType wire_type, int fsindex, NetInStream ins)
        {
            Type type = obj.GetType();
            if (fsindex < 0 || fsindex >= fields.Length)
                return false;
            FieldInfo field = fields[fsindex];
            if (field == null)
                return false;
            //LogWrapper.LogDebug(string.Format("Decode Item :{0}, {1}", field.ToString(), fsindex));
            Type ft = field.GetValue(obj).GetType();
            object val = DecodeItemRaw(ft, wire_type, ins);
            if (val != null)
            {
                field.SetValue(obj, val);
                return true;
            }
            return false;
        }
        public static T DecodeItem<T>(ref T obj, int fsindex, NetInStream nis)
        {
            ushort field_desc = 0;
            nis.Read(ref field_desc);
            WireType wire = parse_wire_type(field_desc);
            obj = (T)DecodeItemRaw(obj.GetType(), wire, nis);
            return obj;
        }
        public static void SaveStream(object obj, NetOutStream outs)
        {
            save_protobuf_compound(-1, obj, outs);
        }
        public static void LoadStream(object obj, NetInStream ins)
        {
            load_protobuf_compound(obj, ins);
        }
    }

    public abstract class ProtoBufMessage : NetMessage
    {
        public static void Encrypt(uint key, byte[] bytes, int offset, int length)
        {
            int end = length - length % 4;
            int pos = offset;
            for (; pos < offset + end; pos += 4)
            {
                bytes[pos + 0] ^= (byte)(key);
                bytes[pos + 1] ^= (byte)(key >> 8);
                bytes[pos + 2] ^= (byte)(key >> 16);
                bytes[pos + 3] ^= (byte)(key >> 24);

                key = (uint)CRC32.GetCRC32(bytes, pos, 4);
            }
            if (pos < offset + length) bytes[pos++] ^= (byte)(key);
            if (pos < offset + length) bytes[pos++] ^= (byte)(key >> 8);
            if (pos < offset + length) bytes[pos++] ^= (byte)(key >> 16);
        }
        public static void Decrypt(uint key, byte[] bytes, int offset, int length)
        {
            int end = length - length % 4;
            int pos = offset;

            for (; pos < offset + end; pos += 4)
            {
                uint newkey = (uint)CRC32.GetCRC32(bytes, pos, 4);
                bytes[pos + 0] ^= (byte)(key);
                bytes[pos + 1] ^= (byte)(key >> 8);
                bytes[pos + 2] ^= (byte)(key >> 16);
                bytes[pos + 3] ^= (byte)(key >> 24);

                key = newkey;
            }
            if (pos < offset + length) bytes[pos++] ^= (byte)(key);
            if (pos < offset + length) bytes[pos++] ^= (byte)(key >> 8);
            if (pos < offset + length) bytes[pos++] ^= (byte)(key >> 16);
        }

        public static void ToStream(NetOutStream outs, NetMessage msg, uint key)
        {
            uint security_flag = 0xacabdeaf;
            int checksum = 0;

            int start = outs.Offset;

            outs.Write(security_flag);
            outs.Write(0);
            outs.Write(0);
            ProtoBufSerializer.SaveStream(msg, outs);

            int end = outs.Offset;

            int buff_start = start + 12;
            int buff_length = end - buff_start;

            if (msg.ENCRYPT)
            {
                Encrypt(key, outs.GetBuffer().buffer, buff_start, buff_length);
                checksum = CRC32.GetCRC32(outs.GetBuffer().buffer, buff_start, buff_length);
            }

            outs.Seek(start + 4);
            outs.Write(checksum);
            outs.Write(buff_length);
            outs.Seek(end);
        }
        public static void FromStream(NetInStream ins, NetMessage msg, uint key)
        {
            uint security_flag = 0xacabdeaf;
            int checksum = 0;
            ins.Read(ref security_flag);
            ins.Read(ref checksum);

            int buff_length = 0;
            ins.Read(ref buff_length);

            if (msg.ENCRYPT)
            {
                int c = CRC32.GetCRC32(ins.GetBuffer().buffer, ins.Offset, buff_length);
                //LogWrapper.LogInfo("checksum of buffer len " + buff.length + " = " + c + " vs " + checksum);
                if (checksum != c)
                {
                    throw new NetStreamException("checksum error", checksum);
                }
                Decrypt(key, ins.GetBuffer().buffer, ins.Offset, buff_length);
            }
            ProtoBufSerializer.LoadStream(msg, ins);
        }

        public override void ToStream(NetOutStream outs)
        {
            ToStream(outs, this, 12345678);
        }
        public override void FromStream(NetInStream ins)
        {
            FromStream(ins, this, 12345678);
        }
    }
}
