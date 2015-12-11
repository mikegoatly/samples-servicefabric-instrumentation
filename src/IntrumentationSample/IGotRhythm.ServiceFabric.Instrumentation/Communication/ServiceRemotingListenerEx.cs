using System;
using System.Fabric;
using IGotRhythm.ServiceFabric.Instrumentation.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace IGotRhythm.ServiceFabric.Instrumentation.Communication
{
    public static class ServiceRemotingListenerEx
    {
        public static ServiceRemotingListener<T> Create<T>(ServiceInitializationParameters parameters, T instance)
            where T : class, IService
        {
            return Create(parameters, instance,
                (initializationParameters, settings, service) =>
                    new WcfCommunicationListener(initializationParameters, typeof(IServiceRemotingContract), service)
                    {
                        EndpointResourceName = settings.EndpointResourceName,
                        Binding = WcfUtil.CreateTcpListenBinding(settings.MaxMessageSize)
                    });
        }
        
        private static ServiceRemotingListener<T> Create<T>(ServiceInitializationParameters parameters, T instance,
            Func<ServiceInitializationParameters, ServiceRemotingListenerSettings, object, ICommunicationListener> factory)
            where T : class, IService
        {
            return new ServiceRemotingListener<T>(parameters, instance, new ServiceRemotingListenerSettings(),
                (initializationParameters, settings, handler) =>
                    new WcfServiceRemotingListener(handler, instance.GetType(), service => factory(initializationParameters, settings, service)));
        }
    }
}