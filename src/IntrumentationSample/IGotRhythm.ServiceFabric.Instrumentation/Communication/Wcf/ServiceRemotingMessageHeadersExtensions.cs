using Microsoft.ServiceFabric.Services.Remoting;

namespace IGotRhythm.ServiceFabric.Instrumentation.Communication.Wcf
{
    internal static class ServiceRemotingMessageHeadersExtensions
    {
        public static byte[] TryGetHeader(this ServiceRemotingMessageHeaders headers, string name)
        {
            byte[] data;
            headers.TryGetHeaderValue(name, out data);
            return data;
        }
    }
}