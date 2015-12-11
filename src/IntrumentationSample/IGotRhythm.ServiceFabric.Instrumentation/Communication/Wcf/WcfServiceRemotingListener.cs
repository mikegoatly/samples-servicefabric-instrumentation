using System;
using System.Security.Claims;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using IGotRhythm.ServiceFabric.Instrumentation.Serialization;
using Microsoft.ServiceFabric.Services.Communication;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace IGotRhythm.ServiceFabric.Instrumentation.Communication.Wcf
{
    internal sealed class WcfServiceRemotingListener : ICommunicationListener
    {
        private readonly ICommunicationListener _innerListener;

        public WcfServiceRemotingListener(IServiceRemotingMessageHandler remotingMessageHandler, Type serviceType, Func<object, ICommunicationListener> factory)
        {
            _innerListener = factory(new WcfRemotingService(remotingMessageHandler, serviceType));
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            return _innerListener.OpenAsync(cancellationToken);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return _innerListener.CloseAsync(cancellationToken);
        }

        public void Abort()
        {
            _innerListener.Abort();
        }

        [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
        private class WcfRemotingService : IServiceRemotingContract
        {
            private readonly IServiceRemotingMessageHandler _messageHandler;

            private readonly WcfServiceRemotingRequestContext _requestContext;
            private readonly ServiceClassDescription _serviceDescription;

            public WcfRemotingService(IServiceRemotingMessageHandler messageHandler, Type serviceType)
            {
                _serviceDescription = new ServiceClassDescription(serviceType);
                _messageHandler = messageHandler;
                _requestContext = new WcfServiceRemotingRequestContext();
            }

            public async Task<byte[]> RequestResponseAsync(ServiceRemotingMessageHeaders headers, byte[] requestBody)
            {
                byte[] result;
                var method = _serviceDescription?[headers.InterfaceId]?[headers.MethodId];
                try
                {
                    if (method != null) LogMethodStart(method.Name);

                    SetServiceContext(headers);

                    result = await _messageHandler.RequestResponseAsync(_requestContext, headers, requestBody)
                        .ConfigureAwait(false);

                    if (method != null) LogMethodStop(method.Name);
                }
                catch (Exception exception)
                {
                    if (method != null) LogMethodFailed(method.Name, exception);

                    throw new FaultException<RemoteExceptionInformation>(
                        RemoteExceptionInformation.FromException(exception));
                }
                finally
                {
                    ServiceContext.SetCurrent(null);
                }
                return result;
            }

            private static void SetServiceContext(ServiceRemotingMessageHeaders headers)
            {
                byte[] bytes;
                if (headers.TryGetHeaderValue(ServiceRemotingHeaders.Identity, out bytes))
                {
                    ServiceContext.SetCurrent(new ServiceContext(
                        SerializationHelper.DeserializeFromBytes<ClaimsPrincipal>(bytes)));
                }
            }

            private void LogMethodFailed(string name, Exception exception)
            {
                // TOOD: add log
            }

            private void LogMethodStop(string name)
            {
                // TOOD: add log
            }

            private void LogMethodStart(string name)
            {
                // TOOD: add log
            }

            public void OneWayMessage(ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
            {
                var method = _serviceDescription?[messageHeaders.InterfaceId]?[messageHeaders.MethodId];
                try
                {
                    if (method != null) LogMethodStart(method.Name);

                    SetServiceContext(messageHeaders);

                    _messageHandler.HandleOneWay(_requestContext, messageHeaders, requestBody);

                    if (method != null) LogMethodStop(method.Name);
                }
                catch (Exception exception)
                {
                    if (method != null) LogMethodFailed(method.Name, exception);

                    throw;
                }
                finally
                {
                    SetServiceContext(null);
                }
            }
        }
    }
}