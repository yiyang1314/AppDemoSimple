using Rxbit.BrigadeApp.Services;
using Rxbit.Brigade.CanDevices.IServices;
using Rxbit.Brigade.CanDevices.Services;
using Rxbit.Brigade.Models.Models;
using Rxbit.Brigade.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using Rxbit.BrigadeApp.Models;

namespace Rxbit.BrigadeApp.ViewModels
{
    public class ViewModelLocator
    {
        private static IServiceProvider _serviceProvider;

        public ViewModelLocator()
        {
            Initialize();
        }

        private static void Initialize()
        {
            if (_serviceProvider != null) return;

            var services = IocContainer.CreateServiceCollection();

            // 注册 ViewModel 为单例
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<CanDeviceViewModel>();
            services.AddSingleton<BirdsEyeViewModel>();
            services.AddSingleton<CanLiveDataViewModel>();
            services.AddSingleton<CoordinateViewModel>();
            services.AddSingleton<NetworkSummaryViewModel>();
            services.AddSingleton<RadarDisplayViewModel>();
            services.AddSingleton<CanLiveService>();
            services.AddSingleton<CanReplayService>();
            services.AddSingleton<Models.BrigadeConfigBus>();
            services.AddSingleton<DataMediatorService<RadarTargetDto>>();
            services.AddSingleton<CanCommService>();
            services.AddSingleton<DetectionSensorInfoViewModel>();
            services.AddSingleton<SensorConfigurationViewModel>();
            services.AddSingleton<BrigadeCanMessageParser>();
            services.AddSingleton<AboutViewModel>();
            //
            // 配置服务提供者
            IocContainer.Configure(services);
            _serviceProvider = IocContainer.Services;
        }

        public MainWindowViewModel MainVM => IocContainer.GetRequiredService<MainWindowViewModel>();

        public CanDeviceViewModel CanDeviceVM => IocContainer.GetRequiredService<CanDeviceViewModel>();

        public BirdsEyeViewModel BirdsEyeVM => IocContainer.GetRequiredService<BirdsEyeViewModel>();

        public CanLiveDataViewModel CanDataVM => IocContainer.GetRequiredService<CanLiveDataViewModel>();

        public CoordinateViewModel Coordinate => IocContainer.GetRequiredService<CoordinateViewModel>();

        public NetworkSummaryViewModel NetworkVM => IocContainer.GetRequiredService<NetworkSummaryViewModel>();

        public RadarDisplayViewModel RadarDisplayVM => IocContainer.GetRequiredService<RadarDisplayViewModel>();


        public SensorConfigurationViewModel SensorConfigVM => IocContainer.GetRequiredService<SensorConfigurationViewModel>();

        public AboutViewModel AboutVM => IocContainer.GetRequiredService<AboutViewModel>();

        public static T GetService<T>()
        {
            return IocContainer.GetRequiredService<T>();
        }
        // DetectionSensorInfoViewModel 使用无参数构造函数
        public DetectionSensorInfoViewModel DetectionVM => IocContainer.GetRequiredService<DetectionSensorInfoViewModel>();
        public BrigadeConfigBus BusConfigVM => IocContainer.GetRequiredService<BrigadeConfigBus>();
        public static void Cleanup()
        {
            _serviceProvider = null;
            IocContainer.Reset();
        }
    }
}