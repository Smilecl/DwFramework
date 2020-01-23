﻿using System.Collections.Generic;

using Autofac.Builder;
using Autofac.Extensions.DependencyInjection;
using AutoFac.Extras.NLog.DotNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DwFramework.Core.Extensions
{
    public static class AutofacExtension
    {
        /// <summary>
        /// 注入配置
        /// </summary>
        /// <param name="host"></param>
        /// <param name="basePath"></param>
        /// <param name="jsonFile"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<IConfiguration, SimpleActivatorData, SingleRegistrationStyle> RegisterConfiguration(this ServiceHost host, string basePath, string jsonFile)
        {
            return host.RegisterInstance(new ConfigurationBuilder().SetBasePath(basePath).AddJsonFile(jsonFile).Build()).As<IConfiguration>().SingleInstance();
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="I"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static T GetService<I, T>(this AutofacServiceProvider provider) where T : class where I : class
        {
            var services = provider.GetServices<I>();
            foreach (var item in services)
            {
                if (item.GetType() == typeof(T))
                    return item as T;
            }
            return default;
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="I"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IEnumerable<I> GetAllServices<I>(this AutofacServiceProvider provider) where I : class
        {
            return provider.GetServices<I>();
        }

        /// <summary>
        /// 注册NLog服务
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ServiceHost RegisterNLog(this ServiceHost host)
        {
            host.RegisterModule<NLogModule>();
            return host;
        }
    }
}
