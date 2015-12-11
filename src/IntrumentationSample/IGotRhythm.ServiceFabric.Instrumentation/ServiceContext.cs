using System.Security.Claims;
using System.Threading;

namespace IGotRhythm.ServiceFabric.Instrumentation
{
    public sealed class ServiceContext
    {
        private static readonly AsyncLocal<ServiceContext> _current = new AsyncLocal<ServiceContext>();

        public static ServiceContext Current => _current.Value;

        internal static void SetCurrent(ServiceContext context)
        {
            _current.Value = context;
        }

        public ClaimsPrincipal Principal { get; }

        internal ServiceContext(ClaimsPrincipal principal)
        {
            Principal = principal;
        }
    }
}