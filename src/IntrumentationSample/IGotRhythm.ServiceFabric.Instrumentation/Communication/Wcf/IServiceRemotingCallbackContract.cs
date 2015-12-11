using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication;
using Microsoft.ServiceFabric.Services.Remoting;

namespace IGotRhythm.ServiceFabric.Instrumentation.Communication.Wcf
{
    [ServiceContract(Namespace = "urn:ServiceFabric.Communication")]
    internal interface IServiceRemotingCallbackContract
    {
        [FaultContract(typeof(RemoteExceptionInformation)), OperationContract]
        Task<byte[]> RequestResponseAsync(ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody);

        [OperationContract(IsOneWay = true)]
        void SendOneWay(ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody);
    }
}