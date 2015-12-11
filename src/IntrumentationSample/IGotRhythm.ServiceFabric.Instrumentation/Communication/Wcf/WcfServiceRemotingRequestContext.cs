using System.ServiceModel;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace IGotRhythm.ServiceFabric.Instrumentation.Communication.Wcf
{
    internal sealed class WcfServiceRemotingRequestContext : IServiceRemotingRequestContext
    {
        public IServiceRemotingCallbackClient GetCallbackClient()
        {
            return new WcfServiceRemotingCommunicationCallback(OperationContext.Current.GetCallbackChannel<IServiceRemotingCallbackContract>());
        }
    }
}