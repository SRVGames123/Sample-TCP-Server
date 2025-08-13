using ModalStrike.Protobuf2;
using ModalStrikeServer.RpcServer.Core;

namespace ModalStrikeServer.RpcServer.Extensions {
    public static class RpcRequestExtension {
        public static T GetValue<T>(this RpcRequest request, int position) {
            var type = typeof(T);
            return type.IsEnum
                ? (T)new EnumFromByteMethod(type).FromBytes(request.Params[position - 1])
                : (T)new FromByteMethod(type).FromBytes(request.Params[position - 1]);
        }

        public static T GetValue<T>(this BinaryValue[] values, int position) {
            var type = typeof(T);
            Type? elementType = null;
            if(type.IsArray) elementType = type.GetElementType();

            return elementType is { IsEnum: true } || type.IsEnum
                ? (T)new EnumFromByteMethod(type).FromBytes(values[position])
                : (T)new FromByteMethod(type).FromBytes(values[position]);
        }
    }
}