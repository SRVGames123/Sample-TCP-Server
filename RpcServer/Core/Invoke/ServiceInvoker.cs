using System.Reflection;
/*using ModalStrikeServer.RpcServer.Attributes;

namespace ModalStrikeServer.RpcServer.Core.Invoke
{
    public class ServiceInvoker : ISystemInvoke
    {
        private readonly Dictionary<string, object> _services = new();

        public ServiceInvoker() => RegisterServices();

        private void RegisterServices()
        {
            var serviceTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<ServiceAttribute>() != null && !t.IsAbstract && !t.IsInterface);

            foreach (var type in serviceTypes)
            {
                var attr = type.GetCustomAttribute<ServiceAttribute>();
                var service = CreateServiceInstance(type);
                _services.Add(attr.Name, service);
            }
        }

        private object CreateServiceInstance(Type serviceType)
        {
            try
            {
                return Activator.CreateInstance(serviceType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create instance of {serviceType.Name}", ex);
            }
        }

        public async Task<object> InvokeAsync(string serviceName, string methodName, params object[] args)
        {
            if (!_services.TryGetValue(serviceName, out var service))
                throw new ArgumentException($"Service '{serviceName}' not found");

            var method = FindMethod(service, methodName, args);
            
            try
            {
                var result = method.Invoke(service, args);
                
                if (result is Task task)
                {
                    await task.ConfigureAwait(false);
                    return task.GetType().GetProperty("Result")?.GetValue(task);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error invoking method '{methodName}'", ex);
            }
        }

        private MethodInfo FindMethod(object service, string methodName, object[] args)
        {
            var methods = service.GetType().GetMethods()
                .Where(m => m.GetCustomAttribute<ServiceMethodAttribute>() != null);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<ServiceMethodAttribute>();
                var nameToCompare = attr.Name ?? method.Name;
                
                if (string.Equals(nameToCompare, methodName, StringComparison.OrdinalIgnoreCase))
                {
                    if (ParametersMatch(method.GetParameters(), args))
                    {
                        return method;
                    }
                }
            }

            throw new ArgumentException($"Method '{methodName}' with {args.Length} parameters not found in service '{service}'");
        }

        private bool ParametersMatch(ParameterInfo[] parameters, object[] args)
        {
            if (parameters.Length != args.Length)
                return false;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (args[i] != null && !parameters[i].ParameterType.IsInstanceOfType(args[i]))
                    return false;
            }

            return true;
        }
    }
}*/