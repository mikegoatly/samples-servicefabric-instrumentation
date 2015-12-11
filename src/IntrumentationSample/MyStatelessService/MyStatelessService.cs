using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Collections.Generic;
using System.Threading.Tasks;
using IGotRhythm.ServiceFabric.Instrumentation;
using IGotRhythm.ServiceFabric.Instrumentation.Communication;
using MyStatelessService.Contracts;

namespace MyStatelessService
{
    internal sealed class MyStatelessService : StatelessService, IMyStatelessService
    {
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            yield return new ServiceInstanceListener(parameters =>
                ServiceRemotingListenerEx.Create<IMyStatelessService>(parameters, this));
        }
        
        public Task<MyData> GetMyData()
        {
            var myClaim = ServiceContext.Current?.Principal.FindFirst("MyClaim");
            return Task.FromResult(new MyData { A = myClaim?.Value, B = 2 });
        }
    }
}
