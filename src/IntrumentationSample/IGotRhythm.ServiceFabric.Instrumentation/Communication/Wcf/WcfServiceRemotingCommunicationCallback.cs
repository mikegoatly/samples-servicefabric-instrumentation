using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace IGotRhythm.ServiceFabric.Instrumentation.Communication.Wcf
{
    internal sealed class WcfServiceRemotingCommunicationCallback : IServiceRemotingCallbackClient
    {
        private readonly IServiceRemotingCallbackContract _callbackChannel;

        public WcfServiceRemotingCommunicationCallback(IServiceRemotingCallbackContract callbackChannel)
        {
            _callbackChannel = callbackChannel;
        }

        public async Task<byte[]> RequestResponseAsync(ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
        {
            var response = await _callbackChannel.RequestResponseAsync(messageHeaders, requestBody).ConfigureAwait(false);
            return response;
        }

        public void OneWayMessage(ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
        {
            _callbackChannel.SendOneWay(messageHeaders, requestBody);
        }
    }
}