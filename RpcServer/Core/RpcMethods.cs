using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Reflection;
using Google.Protobuf;
using ModalStrike.Protobuf2;

namespace ModalStrikeServer.RpcServer.Core {
    public class FromByteMethod {
        // Token: 0x0600020C RID: 524 RVA: 0x0000661C File Offset: 0x0000481C
        public FromByteMethod(Type parameterType) {
            _parameterType = ProtoReflectionUtils.ToProtoType(parameterType);
            parameterType = _parameterType;
            if(parameterType.IsArray) {
                parameterType = parameterType.GetElementType();
            }
            PropertyInfo property = parameterType.GetProperty("Parser");
            if(property == null) {
                throw new ArgumentException("Parse property not found in Type " + parameterType);
            }
            _parser = (MessageParser)property.GetValue(null, null);
        }
        
        // Token: 0x0600020D RID: 525 RVA: 0x000039DD File Offset: 0x00001BDD
        public object FromBytes(BinaryValue bytes) {
            if(bytes.IsNull) {
                return null;
            }
            if(_parameterType.IsArray) {
                return FromBytesArray(_parameterType.GetElementType(), bytes);
            }
            return FromBytesOne(bytes.One, _parameterType);
        }

        // Token: 0x0600020E RID: 526 RVA: 0x00006688 File Offset: 0x00004888
        protected virtual object FromBytesArray(Type type, BinaryValue binaryValue) {
            Array array = Array.CreateInstance(type, binaryValue.Array.Count);
            for(int i = 0; i < array.Length; i++) {
                object value = FromBytesOne(binaryValue.Array[i], type);
                array.SetValue(value, i);
            }
            return array;
        }

        // Token: 0x0600020F RID: 527 RVA: 0x00003A1B File Offset: 0x00001C1B
        protected virtual object FromBytesOne(ByteString bytes, Type elementType) {
            return ProtoReflectionUtils.ToOrigValue(_parser.ParseFrom(bytes), elementType);
        }

        // Token: 0x0400009C RID: 156
        private readonly MessageParser _parser;

        // Token: 0x0400009D RID: 157
        private readonly Type _parameterType;
    }
    public class ProtoReflectionUtils {
        // Token: 0x06000220 RID: 544 RVA: 0x00003AAD File Offset: 0x00001CAD
        public static ToByteMethod[] GetParamToBytesMethods(MethodInfo method) {
            return (from parameterInfo in method.GetParameters()
                    select CreateToByteMethod(parameterInfo.ParameterType)).ToArray();
        }

        // Token: 0x06000221 RID: 545 RVA: 0x000066D8 File Offset: 0x000048D8
        public static ToByteMethod GetReturnToBytesMethod(MethodInfo method) {
            Type returnType = method.ReturnType;
            if(returnType == typeof(void)) {
                return null;
            }
            return CreateToByteMethod(returnType);
        }

        // Token: 0x06000222 RID: 546 RVA: 0x00003ADE File Offset: 0x00001CDE
        public static ToByteMethod CreateToByteMethod(Type type) {
            if(IsEnumType(type)) {
                return new EnumToByteMethod(type);
            }
            return new ToByteMethod(type);
        }

        // Token: 0x06000223 RID: 547 RVA: 0x00003AF5 File Offset: 0x00001CF5
        public static FromByteMethod[] GetParamFromBytesMethods(MethodInfo method) {
            return (from parameterInfo in method.GetParameters()
                    select CreateFromByteMethod(parameterInfo.ParameterType)).ToArray();
        }

        // Token: 0x06000224 RID: 548 RVA: 0x00006704 File Offset: 0x00004904
        public static FromByteMethod GetReturnFromBytesMethod(MethodInfo method) {
            Type returnType = method.ReturnType;
            if(returnType == typeof(void)) {
                return null;
            }
            return CreateFromByteMethod(returnType);
        }

        // Token: 0x06000225 RID: 549 RVA: 0x00003B26 File Offset: 0x00001D26
        private static FromByteMethod CreateFromByteMethod(Type type) {
            if(IsEnumType(type)) {
                return new EnumFromByteMethod(type);
            }
            return new FromByteMethod(type);
        }

        // Token: 0x06000226 RID: 550 RVA: 0x00003B3D File Offset: 0x00001D3D
        public static bool IsEnumType(Type type) {
            if(type.IsArray) {
                type = type.GetElementType();
            }
            return type.IsEnum;
        }

        // Token: 0x06000227 RID: 551 RVA: 0x00006730 File Offset: 0x00004930
        public static bool IsSupportedType(Type type) {
            if(type.IsArray) {
                type = type.GetElementType();
            }
            if(type == typeof(void)) {
                return true;
            }
            if(type.IsEnum) {
                foreach(FieldInfo fieldInfo in type.GetFields()) {
                    if(!fieldInfo.Name.Equals("value__") && !Attribute.IsDefined(fieldInfo, typeof(OriginalNameAttribute))) {
                        return false;
                    }
                }
                return true;
            }
            return type == typeof(int) || type == typeof(int?) || type.IsArray && type.GetElementType() == typeof(int) || type == typeof(float) || type == typeof(float?) || type.IsArray && type.GetElementType() == typeof(float) || type == typeof(long) || type == typeof(long?) || type.IsArray && type.GetElementType() == typeof(long) || type == typeof(string) || type.IsArray && type.GetElementType() == typeof(string) || type == typeof(double) || type == typeof(double?) || type.IsArray && type.GetElementType() == typeof(double) || type == typeof(bool) || type == typeof(bool?) || type.IsArray && type.GetElementType() == typeof(bool) || type == typeof(byte) || type == typeof(byte?) || type.IsArray && type.GetElementType() == typeof(byte) || typeof(IMessage).IsAssignableFrom(type);
        }

        // Token: 0x06000228 RID: 552 RVA: 0x00006940 File Offset: 0x00004B40
        public static object ToProtoValue(object o) {
            if(o.GetType().IsArray && o.GetType().GetElementType().IsEnum) {
                return o;
            }
            if(o is int) {
                return new Integer {
                    Value = (int)o
                };
            }
            if(o is int[]) {
                IntegerArray integerArray = new IntegerArray();
                integerArray.Value.AddRange((int[])o);
                return integerArray;
            }
            if(o is float) {
                return new Float {
                    Value = (float)o
                };
            }
            if(o is float[]) {
                FloatArray floatArray = new FloatArray();
                floatArray.Value.AddRange((float[])o);
                return floatArray;
            }
            if(o is long) {
                return new Long {
                    Value = (long)o
                };
            }
            if(o is long[]) {
                LongArray longArray = new LongArray();
                longArray.Value.AddRange((long[])o);
                return longArray;
            }
            if(o is string) {
                return new ModalStrike.Protobuf2.String {
                    Value = (string)o
                };
            }
            if(o is string[]) {
                StringArray stringArray = new StringArray();
                stringArray.Value.AddRange((string[])o);
                return stringArray;
            }
            if(o is double) {
                return new ModalStrike.Protobuf2.Double {
                    Value = (double)o
                };
            }
            if(o is double[]) {
                DoubleArray doubleArray = new DoubleArray();
                doubleArray.Value.AddRange((double[])o);
                return doubleArray;
            }
            if(o is bool) {
                return new ModalStrike.Protobuf2.Boolean {
                    Value = (bool)o
                };
            }
            if(o is bool[]) {
                BooleanArray booleanArray = new BooleanArray();
                booleanArray.Value.AddRange((bool[])o);
                return booleanArray;
            }
            if(o is byte) {
                return new ModalStrike.Protobuf2.Byte {
                    Value = ByteString.CopyFrom(new byte[]
                    {
                        (byte)o
                    })
                };
            }
            if(o is byte[]) {
                return new ModalStrike.Protobuf2.Byte {
                    Value = ByteString.CopyFrom((byte[])o)
                };
            }
            return o;
        }

        // Token: 0x06000229 RID: 553 RVA: 0x00006B10 File Offset: 0x00004D10
        public static object ToOrigValue(object protoValue, Type origType) {
            if(protoValue is Integer) {
                return ((Integer)protoValue).Value;
            }
            if(protoValue is IntegerArray) {
                return ((IntegerArray)protoValue).Value.ToArray();
            }
            if(protoValue is Float) {
                return ((Float)protoValue).Value;
            }
            if(protoValue is FloatArray) {
                return ((FloatArray)protoValue).Value.ToArray();
            }
            if(protoValue is Long) {
                return ((Long)protoValue).Value;
            }
            if(protoValue is LongArray) {
                return ((LongArray)protoValue).Value.ToArray();
            }
            if(protoValue is ModalStrike.Protobuf2.String) {
                return ((ModalStrike.Protobuf2.String)protoValue).Value;
            }
            if(protoValue is StringArray) {
                return ((StringArray)protoValue).Value.ToArray();
            }
            if(protoValue is ModalStrike.Protobuf2.Double) {
                return ((ModalStrike.Protobuf2.Double)protoValue).Value;
            }
            if(protoValue is DoubleArray) {
                return ((DoubleArray)protoValue).Value.ToArray();
            }
            if(protoValue is ModalStrike.Protobuf2.Boolean) {
                return ((ModalStrike.Protobuf2.Boolean)protoValue).Value;
            }
            if(protoValue is BooleanArray) {
                return ((BooleanArray)protoValue).Value.ToArray();
            }
            if(protoValue is ModalStrike.Protobuf2.Byte) {
                return ((ModalStrike.Protobuf2.Byte)protoValue).Value.ToByteArray()[0];
            }
            if(protoValue is ByteArray) {
                return ((ByteArray)protoValue).Value.ToByteArray();
            }
            return protoValue;
        }

        // Token: 0x0600022A RID: 554 RVA: 0x00006C80 File Offset: 0x00004E80
        public static Type ToProtoType(Type origType) {
            if(origType == typeof(int) || origType == typeof(int?)) {
                return typeof(Integer);
            }
            if(origType.IsArray && origType.GetElementType() == typeof(int)) {
                return typeof(IntegerArray);
            }
            if(origType == typeof(float) || origType == typeof(float?)) {
                return typeof(Float);
            }
            if(origType.IsArray && origType.GetElementType() == typeof(float)) {
                return typeof(FloatArray);
            }
            if(origType == typeof(long) || origType == typeof(long?)) {
                return typeof(Long);
            }
            if(origType.IsArray && origType.GetElementType() == typeof(long)) {
                return typeof(LongArray);
            }
            if(origType == typeof(string)) {
                return typeof(ModalStrike.Protobuf2.String);
            }
            if(origType.IsArray && origType.GetElementType() == typeof(string)) {
                return typeof(StringArray);
            }
            if(origType == typeof(double) || origType == typeof(double?)) {
                return typeof(ModalStrike.Protobuf2.Double);
            }
            if(origType.IsArray && origType.GetElementType() == typeof(double)) {
                return typeof(DoubleArray);
            }
            if(origType == typeof(bool) || origType == typeof(bool?)) {
                return typeof(ModalStrike.Protobuf2.Boolean);
            }
            if(origType.IsArray && origType.GetElementType() == typeof(bool)) {
                return typeof(BooleanArray);
            }
            if(origType == typeof(byte) || origType == typeof(byte?)) {
                return typeof(ModalStrike.Protobuf2.Byte);
            }
            if(origType.IsArray && origType.GetElementType() == typeof(byte)) {
                return typeof(ByteArray);
            }
            return origType;
        }
    }
    public class EnumFromByteMethod : FromByteMethod {
        // Token: 0x060001FB RID: 507 RVA: 0x00006290 File Offset: 0x00004490
        public EnumFromByteMethod(Type parameterType) : base(!parameterType.IsArray ? typeof(ModalStrike.Protobuf2.Enum) : typeof(ModalStrike.Protobuf2.Enum[])) {
            if(parameterType == null) {
                throw new ArgumentNullException("parameterType");
            }
            _enumType = parameterType;
            if(_enumType.IsArray) {
                _enumType = _enumType.GetElementType();
            }
        }

        // Token: 0x060001FC RID: 508 RVA: 0x000038E0 File Offset: 0x00001AE0
        protected override object FromBytesArray(Type type, BinaryValue binaryValue) {
            return base.FromBytesArray(_enumType, binaryValue);
        }

        // Token: 0x060001FD RID: 509 RVA: 0x000062F4 File Offset: 0x000044F4
        protected override object FromBytesOne(ByteString bytes, Type elementType) {
            ModalStrike.Protobuf2.Enum @enum = (ModalStrike.Protobuf2.Enum)base.FromBytesOne(bytes, elementType);
            return System.Enum.ToObject(_enumType, @enum.Value);
        }

        // Token: 0x04000093 RID: 147
        private readonly Type _enumType;
    }
    public class ToByteMethod {
        // Token: 0x06000261 RID: 609 RVA: 0x00003D80 File Offset: 0x00001F80
        public ToByteMethod(Type parameterType) {
            _parameterType = ProtoReflectionUtils.ToProtoType(parameterType);
        }

        // Token: 0x06000262 RID: 610 RVA: 0x00003D94 File Offset: 0x00001F94
        public virtual BinaryValue ToBytes(object arg) {
            if(arg == null) {
                return new BinaryValue {
                    IsNull = true
                };
            }
            if(_parameterType.IsArray) {
                return ToBytesArray(arg);
            }
            return new BinaryValue {
                IsNull = false,
                One = ToBytesOne(arg)
            };
        }

        // Token: 0x06000263 RID: 611 RVA: 0x00007638 File Offset: 0x00005838
        private BinaryValue ToBytesArray(object arg) {
            BinaryValue binaryValue = new BinaryValue {
                IsNull = false
            };
            IEnumerable enumerable = arg as IEnumerable;
            if(enumerable != null) {
                IEnumerator enumerator = enumerable.GetEnumerator();
                try {
                    while(enumerator.MoveNext()) {
                        object arg2 = enumerator.Current;
                        binaryValue.Array.Add(ToBytesOne(arg2));
                    }
                }
                finally {
                    IDisposable disposable;
                    if((disposable = enumerator as IDisposable) != null) {
                        disposable.Dispose();
                    }
                }
            }
            return binaryValue;
        }

        // Token: 0x06000264 RID: 612 RVA: 0x00003DD4 File Offset: 0x00001FD4
        protected virtual ByteString ToBytesOne(object arg) {
            return ((IMessage)ProtoReflectionUtils.ToProtoValue(arg)).ToByteString();
        }

        // Token: 0x040000C3 RID: 195
        private readonly Type _parameterType;
    }
    public class EnumToByteMethod : ToByteMethod {
        // Token: 0x060001FE RID: 510 RVA: 0x000038EF File Offset: 0x00001AEF
        public EnumToByteMethod(Type parameterType) : base(!parameterType.IsArray ? typeof(ModalStrike.Protobuf2.Enum) : typeof(ModalStrike.Protobuf2.Enum[])) {
        }

        // Token: 0x060001FF RID: 511 RVA: 0x00006320 File Offset: 0x00004520
        protected override ByteString ToBytesOne(object arg) {
            if(arg == null) {
                throw new ArgumentNullException("arg");
            }
            ModalStrike.Protobuf2.Enum arg2 = new ModalStrike.Protobuf2.Enum {
                Value = (int)arg
            };
            return base.ToBytesOne(arg2);
        }
    }
}
