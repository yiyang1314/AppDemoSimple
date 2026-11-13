using Microsoft.Extensions.DependencyInjection;
using System;

namespace Rxbit.BrigadeApp.ViewModels
{
    public static class IocContainer
    {
        private static IServiceProvider _serviceProvider;

        public static IServiceProvider Services => _serviceProvider;

        public static void Configure(IServiceCollection services)
        {
            _serviceProvider = services.BuildServiceProvider();
        }

        public static IServiceCollection CreateServiceCollection()
        {
            return new ServiceCollection();
        }

        public static T GetService<T>()
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("Service provider is not configured. Call Configure() first.");
            }
            return _serviceProvider.GetService<T>();
        }

        public static T GetRequiredService<T>()
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("Service provider is not configured. Call Configure() first.");
            }
            return _serviceProvider.GetRequiredService<T>();
        }

        public static object GetService(Type serviceType)
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("Service provider is not configured. Call Configure() first.");
            }
            return _serviceProvider.GetService(serviceType);
        }

        public static object GetRequiredService(Type serviceType)
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("Service provider is not configured. Call Configure() first.");
            }
            return _serviceProvider.GetRequiredService(serviceType);
        }

        public static bool IsConfigured => _serviceProvider != null;

        public static void Reset()
        {
            _serviceProvider = null;
        }
    }
}
