using System.Collections.Concurrent;
using System.Reflection;
using ModalStrike.Protobuf2;
using ModalStrikeServer.RpcServer.Utilities;

namespace ModalStrikeServer.RpcServer.Core {
    public static class ServiceInvoker {
        private static readonly ConcurrentDictionary<string, MethodInfo> _cachedMethodInfos = new();

        public async static Task<bool> Invoke(object service, string methodName, BinaryValue[] args) {
            if(service is null) throw new ArgumentNullException(nameof(service), "Service object cannot be null.");

            var parameterTypes = args.Length > 0
                 ? new Type[] { typeof(BinaryValue[]) }
                 : Type.EmptyTypes;

            var key = $"{service.GetType().FullName}.{methodName}";

            var methodInfo = _cachedMethodInfos.GetOrAdd(key, k => {
                var serviceType = service.GetType();
                var method = serviceType.GetMethod(methodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null, parameterTypes, null);

                if(method is null) {
                    Logger.Error($"Method '{methodName}' with signature ({string.Join(", ", parameterTypes.Select(t => t.Name))}) not found, for type '{serviceType.FullName}'.");
                    throw new ArgumentException($"Метод '{methodName}' с сигнатурой ({string.Join(", ", parameterTypes.Select(t => t.Name))}) не найден в типе '{serviceType.FullName}'.");
                }

                return method;
            });

            try {
                
                var result = args.Length > 0 ? methodInfo.Invoke(service, new object[] { args }) : methodInfo.Invoke(service, Array.Empty<object>());

                if(result is Task task) {
                    await task;

                    if(task.GetType().IsGenericType) {
                        var resultProperty = task.GetType().GetProperty("Result");
                        var taskResult = resultProperty?.GetValue(task);
                    }
                }

                return true;
            }
            catch(System.Exception ex) {
                Logger.Error($"Error invoking method {methodName}: {ex}");
                return false;
            }
        }
    }
}
