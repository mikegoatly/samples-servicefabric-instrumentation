using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace MyStatelessService.Contracts
{
    public interface IMyStatelessService : IService
    {
        Task<MyData> GetMyData();
    }
}