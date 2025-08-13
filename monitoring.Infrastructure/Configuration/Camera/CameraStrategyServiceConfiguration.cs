using Microsoft.Extensions.DependencyInjection;
using Monitoring.Domain.Services.Camera;
using Monitoring.Infrastructure.Camera;
using Monitoring.Infrastructure.Camera.Strategies;
using System;
using System.Net.Http;

namespace Monitoring.Infrastructure.Configuration.Camera
{
    /// <summary>
    /// تنظیمات DI برای Camera Strategies و Factory Pattern
    /// </summary>
    public static class CameraStrategyServiceConfiguration
    {
        /// <summary>
        /// ثبت تمام سرویس‌های مربوط به Camera Strategy ها
        /// </summary>
        public static IServiceCollection AddCameraStrategies(this IServiceCollection services)
        {
            // ثبت HttpClient برای استراتژی‌ها
            services.AddHttpClient<RTSPCameraStrategy>();
            services.AddHttpClient<HTTPCameraStrategy>();
            services.AddHttpClient<USBCameraStrategy>();
            services.AddHttpClient<ONVIFCameraStrategy>();
            services.AddHttpClient<HikvisionSDKStrategy>();
            services.AddHttpClient<DahuaSDKStrategy>();

            // ثبت استراتژی‌های اتصال دوربین
            services.AddTransient<ICameraConnectionStrategy, RTSPCameraStrategy>();
            services.AddTransient<ICameraConnectionStrategy, HTTPCameraStrategy>();
            services.AddTransient<ICameraConnectionStrategy, USBCameraStrategy>();
            services.AddTransient<ICameraConnectionStrategy, ONVIFCameraStrategy>();
            services.AddTransient<ICameraConnectionStrategy, HikvisionSDKStrategy>();
            services.AddTransient<ICameraConnectionStrategy, DahuaSDKStrategy>();

            // ثبت Factory و Selector
            services.AddSingleton<ICameraStrategyFactory, CameraStrategyFactory>();
            services.AddTransient<CameraStrategySelector>();

            // ثبت Domain Service
            services.AddTransient<CameraConnectionDomainService>();

            return services;
        }

        /// <summary>
        /// ثبت استراتژی‌های اضافی (ONVIF, SDK-specific)
        /// </summary>
        public static IServiceCollection AddAdvancedCameraStrategies(this IServiceCollection services)
        {
            // ONVIF Strategy (اکنون در دسترس است)
            services.AddTransient<ICameraConnectionStrategy, ONVIFCameraStrategy>();
            
            // SDK-specific strategies (اکنون در دسترس است)
            services.AddTransient<ICameraConnectionStrategy, HikvisionSDKStrategy>();
            services.AddTransient<ICameraConnectionStrategy, DahuaSDKStrategy>();

            return services;
        }

        /// <summary>
        /// تنظیمات HttpClient برای استراتژی‌های شبکه‌ای
        /// </summary>
        public static IServiceCollection ConfigureCameraHttpClients(this IServiceCollection services)
        {
            services.AddHttpClient("CameraStrategy", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "Monitoring-System/1.0");
            });

            services.AddHttpClient<RTSPCameraStrategy>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15);
            });

            services.AddHttpClient<HTTPCameraStrategy>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(20);
            });

            services.AddHttpClient<USBCameraStrategy>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(20);
            });

            services.AddHttpClient<ONVIFCameraStrategy>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(25);
                client.DefaultRequestHeaders.Add("SOAPAction", "\"\"");
            });

            services.AddHttpClient<HikvisionSDKStrategy>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "Hikvision-Monitor/1.0");
            });

            services.AddHttpClient<DahuaSDKStrategy>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "Dahua-Monitor/1.0");
            });

            return services;
        }
    }
}
