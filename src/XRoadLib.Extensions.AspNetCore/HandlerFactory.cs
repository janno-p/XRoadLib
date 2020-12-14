using System;
using System.Threading.Tasks;

namespace XRoadLib.Extensions.AspNetCore
{
    internal class HandlerFactory
    {
        public Func<IServiceProvider, Func<object, Task<object>>> Factory { get; }

        public HandlerFactory(Func<IServiceProvider, Func<object, Task<object>>> factory)
        {
            Factory = factory;
        }
    }
}