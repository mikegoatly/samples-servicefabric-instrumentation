using System;
using System.Security.Claims;
using System.Threading;
using IGotRhythm.ServiceFabric.Instrumentation.Communication;
using MyStatelessService.Contracts;

namespace MyServiceTest
{
    internal static class Program
    {
        private static void Main()
        {
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("MyClaim", "MyClaimValue") }));

            var myService = ServiceProxyEx.Create<IMyStatelessService>(new Uri("fabric:/IntrumentationSample/MyStatelessService"));

            var result = myService.GetMyData().Result;

            Console.WriteLine(result.A);
        }
    }
}
