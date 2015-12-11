using System;
using System.Fabric;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using IGotRhythm.ServiceFabric.Instrumentation.Threading;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace IGotRhythm.ServiceFabric.Instrumentation.Communication.Wcf
{
    internal class WcfServiceRemotingClientFactory : IServiceRemotingClientFactory
    {
        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientConnected;

        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientDisconnected;

        public WcfServiceRemotingClientFactory(IServiceRemotingCallbackClient callbackImplementation, Binding clientBinding)
        {
            WcfFactory =
                new WcfCommunicationClientFactory<IServiceRemotingContract>(ServicePartitionResolver.GetDefault(),
                    clientBinding, new CallbackReciver(callbackImplementation), new IExceptionHandler[]
                    {
                        new FaultExceptionHandler(this)
                    });
            WcfFactory.ClientConnected += OnClientConnected;
            WcfFactory.ClientDisconnected += OnClientDisconnected;
        }

        protected WcfCommunicationClientFactory<IServiceRemotingContract> WcfFactory { get; }

        public virtual async Task<IServiceRemotingClient> GetClientAsync(ResolvedServicePartition previousRsp, string listenerName, CancellationToken cancellationToken)
        {
            var wcfClient = await WcfFactory.GetClientAsync(previousRsp, listenerName, cancellationToken).ConfigureAwait(false);
            return new WcfServiceRemotingClient(wcfClient);
        }

        public virtual async Task<IServiceRemotingClient> GetClientAsync(Uri serviceUri, long partitionKey, string listenerName, CancellationToken cancellationToken)
        {
            var wcfClient = await WcfFactory.GetClientAsync(serviceUri, partitionKey, listenerName, cancellationToken).ConfigureAwait(false);
            return new WcfServiceRemotingClient(wcfClient);
        }

        public virtual async Task<IServiceRemotingClient> GetClientAsync(Uri serviceUri, string partitionKey, string listenerName, CancellationToken cancellationToken)
        {
            var wcfClient = await WcfFactory.GetClientAsync(serviceUri, partitionKey, listenerName, cancellationToken).ConfigureAwait(false);
            return new WcfServiceRemotingClient(wcfClient);
        }

        public virtual async Task<IServiceRemotingClient> GetClientAsync(Uri serviceUri, string listenerName, CancellationToken cancellationToken)
        {
            var wcfClient = await WcfFactory.GetClientAsync(serviceUri, listenerName, cancellationToken).ConfigureAwait(false);
            return new WcfServiceRemotingClient(wcfClient);
        }

        public virtual Task<OperationRetryControl> ReportOperationExceptionAsync(IServiceRemotingClient client, Exception exception, string listenerName, CancellationToken cancellationToken)
        {
            return WcfFactory.ReportOperationExceptionAsync(((WcfServiceRemotingClient)client).WcfClient, exception, listenerName, cancellationToken);
        }

        protected bool HandleRemoteException(Exception e, out ExceptionHandlingResult result)
        {
            if (e is FabricTransientException)
            {
                result = new ExceptionHandlingRetryResult
                {
                    IsTransient = true,
                    RetryDelay = TimeSpan.FromMilliseconds(ThreadSafeRandom.Value.NextDouble() * WcfFactory.MaxRetryBackoffIntervalOnTransientErrors.TotalMilliseconds)
                };
                return true;
            }
            if (e is FabricNotPrimaryException)
            {
                result = new ExceptionHandlingRetryResult
                {
                    IsTransient = false,
                    RetryDelay = TimeSpan.FromMilliseconds(ThreadSafeRandom.Value.NextDouble() * WcfFactory.MaxRetryBackoffIntervalOnNonTransientErrors.TotalMilliseconds)
                };
                return true;
            }
            result = new ExceptionHandlingThrowResult
            {
                ExceptionToThrow = new AggregateException(e)
            };
            return true;
        }

        private void OnClientDisconnected(object sender, CommunicationClientEventArgs<WcfCommunicationClient<IServiceRemotingContract>> communicationClientEventArgs)
        {
            var clientDisconnected = ClientDisconnected;
            clientDisconnected?.Invoke(this, new CommunicationClientEventArgs<IServiceRemotingClient>
            {
                Client = new WcfServiceRemotingClient(communicationClientEventArgs.Client)
            });
        }

        private void OnClientConnected(object sender, CommunicationClientEventArgs<WcfCommunicationClient<IServiceRemotingContract>> communicationClientEventArgs)
        {
            var clientConnected = ClientConnected;
            clientConnected?.Invoke(this, new CommunicationClientEventArgs<IServiceRemotingClient>
            {
                Client = new WcfServiceRemotingClient(communicationClientEventArgs.Client)
            });
        }

        [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
        protected class CallbackReciver : IServiceRemotingCallbackContract
        {
            private readonly IServiceRemotingCallbackClient _callbackHandler;

            public CallbackReciver(IServiceRemotingCallbackClient callbackHandler)
            {
                _callbackHandler = callbackHandler;
            }

            public Task<byte[]> RequestResponseAsync(ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
            {
                return _callbackHandler.RequestResponseAsync(messageHeaders, requestBody);
            }

            public void SendOneWay(ServiceRemotingMessageHeaders headers, byte[] msgBody)
            {
                Task.Run(() => _callbackHandler.OneWayMessage(headers, msgBody));
            }
        }

        private class FaultExceptionHandler : IExceptionHandler
        {
            private readonly WcfServiceRemotingClientFactory _owner;

            public FaultExceptionHandler(WcfServiceRemotingClientFactory owner)
            {
                _owner = owner;
            }

            bool IExceptionHandler.HandleException(Exception e, out ExceptionHandlingResult result)
            {
                var faultException = e as FaultException<RemoteExceptionInformation>;
                if (faultException == null)
                {
                    result = null;
                    return false;
                }
                Exception exception;
                if (!RemoteExceptionInformation.ToException(faultException.Detail, out exception))
                {
                    throw new ArgumentException("failed to deserialize and get remote exception", nameof(e));
                }
                return ExceptionUtility.HandleAggregateException(exception, _owner.HandleRemoteException, out result);
            }
        }
    }
}