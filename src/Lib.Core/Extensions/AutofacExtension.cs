﻿using System;
using System.Linq;
using System.Collections.Generic;

using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

using DwFramework.Core;
using DwFramework.Core.Plugins;

namespace DwFramework.Core.Extensions
{
    public static class AutofacExtension
    {
        /// <summary>
        /// 服务是否注册
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static bool IsRegistered<T>(this IServiceProvider provider) where T : class
        {
            var services = provider.GetServices<T>();
            return services.Count() > 0;
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static T GetService<T>(this IServiceProvider provider) where T : class
        {
            return provider.GetService(typeof(T)) as T;
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetServices<T>(this IServiceProvider provider) where T : class
        {
            return provider.GetServices(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static T GetService<T>(this ILifetimeScope scope) where T : class
        {
            return scope.Resolve<T>();
        }

        #region Plugins
        /// <summary>
        /// 注册Log服务
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ServiceHost RegisterLog(this ServiceHost host)
        {
            host.RegisterService(services => services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddNLog();
            }));
            return host;
        }

        /// <summary>
        /// 获取Log服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static ILogger<T> GetLogger<T>(this IServiceProvider provider)
        {
            return provider.GetService<ILogger<T>>();
        }

        /// <summary>
        /// 注册MemoryCache服务
        /// </summary>
        /// <param name="host"></param>
        /// <param name="storeCount"></param>
        /// <param name="isGlobal"></param>
        /// <returns></returns>
        public static ServiceHost RegisterMemoryCache(this ServiceHost host, int storeCount = 6, bool isGlobal = true)
        {
            var builder = host.Register(context => new MemoryCache(storeCount)).As<ICache>();
            if (isGlobal) builder.SingleInstance();
            return host;
        }
        #endregion
    }
}