using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Microsoft.ServiceFabric.Services;

namespace IGotRhythm.ServiceFabric.Instrumentation.Communication
{
    internal class ServiceClassDescription
    {
        public ServiceClassDescription(Type serviceType)
        {
            Interfaces = new ReadOnlyDictionary<int, ServiceInterfaceDescription>(
                ServiceTypeInformation.Get(serviceType).InterfaceTypes
                    .Select(x => new ServiceInterfaceDescription(x)).ToDictionary(x => x.Id));
        }

        public IReadOnlyDictionary<int, ServiceInterfaceDescription> Interfaces { get; }

        public ServiceInterfaceDescription this[int key]
        {
            get
            {
                ServiceInterfaceDescription description;
                Interfaces.TryGetValue(key, out description);
                return description;
            }
        }
    }

    internal class ServiceInterfaceDescription
    {
        public int Id { get; }

        public Type InterfaceType
        {
            get;
            private set;
        }

        public IReadOnlyDictionary<int, ServiceMethodDescription> Methods { get; }

        public ServiceMethodDescription this[int key]
        {
            get
            {
                ServiceMethodDescription description;
                Methods.TryGetValue(key, out description);
                return description;
            }
        }

        public ServiceInterfaceDescription(Type serviceInterfaceType)
        {
            InterfaceType = serviceInterfaceType;
            Methods = new ReadOnlyDictionary<int, ServiceMethodDescription>(
                serviceInterfaceType.GetMethods()
                .Select(methodInfo => new ServiceMethodDescription(methodInfo)).ToDictionary(x => x.Id));
            Id = IdUtil.ComputeId(serviceInterfaceType);
        }
    }

    internal class ServiceMethodDescription
    {
        public int Id { get; }

        public string Name => MethodInfo.Name;

        public Type ReturnType => MethodInfo.ReturnType;

        public MethodInfo MethodInfo { get; }

        internal ServiceMethodDescription(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
            Id = IdUtil.ComputeId(methodInfo);
        }
    }

    internal static class IdUtil
    {
        internal static int ComputeId(MethodInfo methodInfo)
        {
            var hash = methodInfo.Name.GetHashCode();
            if (methodInfo.DeclaringType != null)
            {
                if (methodInfo.DeclaringType.Namespace != null)
                {
                    hash = HashCombine(methodInfo.DeclaringType.Namespace.GetHashCode(), hash);
                }
                hash = HashCombine(methodInfo.DeclaringType.Name.GetHashCode(), hash);
            }
            return hash;
        }

        internal static int ComputeId(Type type)
        {
            int num = type.Name.GetHashCode();
            if (type.Namespace != null)
            {
                num = HashCombine(type.Namespace.GetHashCode(), num);
            }
            return num;
        }

        internal static int ComputeId(string typeName, string typeNamespace)
        {
            int num = typeName.GetHashCode();
            if (typeNamespace != null)
            {
                num = HashCombine(typeNamespace.GetHashCode(), num);
            }
            return num;
        }

        internal static int HashCombine(int newKey, int currentKey)
        {
            return currentKey * -1521134295 + newKey;
        }
    }
}