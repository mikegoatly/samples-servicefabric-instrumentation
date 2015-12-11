using System.Fabric;
using System.Security.Claims;
using System.Threading.Tasks;
using IGotRhythm.ServiceFabric.Instrumentation.Annotations;
using IGotRhythm.ServiceFabric.Instrumentation.Serialization;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace IGotRhythm.ServiceFabric.Instrumentation.Communication.Wcf
{
    internal class WcfServiceRemotingClient : IServiceRemotingClient
    {
        public WcfCommunicationClient<IServiceRemotingContract> WcfClient { get; }

        public virtual ResolvedServicePartition ResolvedServicePartition
        {
            get { return WcfClient.ResolvedServicePartition; }
            set { WcfClient.ResolvedServicePartition = value; }
        }

        protected virtual IServiceRemotingContract Channel => WcfClient.Channel;
        
        public WcfServiceRemotingClient([NotNull] WcfCommunicationClient<IServiceRemotingContract> wcfClient)
        {
            WcfClient = wcfClient;
        }

        public Task<byte[]> RequestResponseAsync(ServiceRemotingMessageHeaders headers, byte[] requestBody)
        {
            AddHeaders(headers);
            return Channel.RequestResponseAsync(headers, requestBody).ContinueWith(t => t.GetAwaiter().GetResult(), TaskScheduler.Default);
        }

        public void SendOneWay(ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
        {
            AddHeaders(messageHeaders);
            Channel.OneWayMessage(messageHeaders, requestBody);
        }

        private static void AddHeaders(ServiceRemotingMessageHeaders headers)
        {
            byte[] value;
            if (!headers.TryGetHeaderValue(ServiceRemotingHeaders.Identity, out value))
            {
                headers.AddHeader(ServiceRemotingHeaders.Identity, SerializationHelper.SerializeToBytes(ClaimsPrincipal.Current));
            }
        }
    }
}