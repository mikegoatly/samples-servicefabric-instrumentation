using System;
using IGotRhythm.ServiceFabric.Instrumentation.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace IGotRhythm.ServiceFabric.Instrumentation.Communication
{
    public static class ServiceProxyEx
    {
        public static TServiceInterface Create<TServiceInterface>(Uri serviceName, string listenerName = null) where TServiceInterface : IService
        {
            return ServiceProxy.Create<TServiceInterface>(serviceName, listenerName, CreateCommunicationClientFactory);
        }

        public static TServiceInterface Create<TServiceInterface>(long partitionKey, Uri serviceName,
            string listenerName = null) where TServiceInterface : IService
        {
            return ServiceProxy.Create<TServiceInterface>(partitionKey, serviceName, listenerName, CreateCommunicationClientFactory);

        }

        public static TServiceInterface Create<TServiceInterface>(string partitionKey, Uri serviceName,
            string listenerName = null) where TServiceInterface : IService
        {
            return ServiceProxy.Create<TServiceInterface>(partitionKey, serviceName, listenerName, CreateCommunicationClientFactory);
        }

        private static IServiceRemotingClientFactory CreateCommunicationClientFactory(IServiceRemotingCallbackClient client, ServiceRemotingSettings settings)
        {
            return new WcfServiceRemotingClientFactory(client, WcfUtil.CreateTcpClientBinding(settings.MaxMessageSize));
        }
    }
}