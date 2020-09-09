using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorFormManager.Demo.Server.Extensions
{
    public static class HttpContextExtensions
    {
        private const BindingFlags InvocationFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

        public static bool CreateController(this HttpContext context, string fullName, out ControllerBase result, bool throwOnError = false, bool ignoreCase = true)
        {
            result = null;
            var controllerType = Type.GetType(fullName, throwOnError, ignoreCase);
            if (controllerType != null)
            {
                var argsList = new List<object>();
                foreach (var pi in controllerType.GetConstructors().First().GetParameters())
                {
                    var arg = context.RequestServices.GetService(pi.ParameterType);
                    if (arg != null) argsList.Add(arg);
                }
                result = (ControllerBase)Activator.CreateInstance(controllerType, argsList.ToArray());
            }
            return result != null;
        }

        public static Task<TResult> InvokeControllerActionAsync<TResult>(this HttpContext context, string controllerFullName, string actionName, params object[] args)
        {
            if (context.CreateController(controllerFullName, out ControllerBase controller, throwOnError: true))
                return controller.InvokeActionAsync<TResult>(actionName, args);
            throw new TypeLoadException($"Cannot create an instance of the type {controllerFullName}.");
        }

        private static Task<TResult> InvokeActionAsync<TResult>(this ControllerBase instance, string name, params object[] args)
        {
            var type = instance.GetType();
            var methodInfo = type.GetMethod(name, InvocationFlags);
            if (methodInfo != null)
            {
                var invokeResult = methodInfo.Invoke(instance, args);

                if (invokeResult is Task<TResult> task) return task;
                if (invokeResult is TResult result) return Task.FromResult(result);

                throw new InvalidCastException($"Cannot convert {invokeResult.GetType().FullName} to {typeof(TResult).FullName}.");
            }
            throw new TargetInvocationException($"Method {name} cannot be invoked on controller {type.FullName}.", null);
        }
    }
}
