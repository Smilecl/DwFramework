﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Grpc.Core;
using Microsoft.Extensions.Logging;

using DwFramework.Core;
using DwFramework.Core.Plugins;

namespace DwFramework.RPC
{
    public sealed class Config
    {
        public string ContentRoot { get; set; }
        public Dictionary<string, string> Listen { get; set; }
    }

    public sealed class RPCService
    {
        private readonly Config _config;
        private readonly ILogger<RPCService> _logger;
        private readonly Server _server;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configKey"></param>
        /// <param name="configPath"></param>
        public RPCService(string configKey = null, string configPath = null)
        {
            _config = ServiceHost.Environment.GetConfiguration<Config>(configKey, configPath);
            if (_config == null) throw new Exception("RPC初始化异常 => 未读取到Rpc配置");
            _logger = ServiceHost.Provider.GetLogger<RPCService>();
            _server = new Server();
        }

        /// <summary>
        /// 添加RPC服务
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public RPCService AddService(object service)
        {
            _server.Services.Add(GetServerServiceDefinition(service));
            return this;
        }

        /// <summary>
        /// 开启服务
        /// </summary>
        /// <returns></returns>
        public Task OpenServiceAsync()
        {
            return TaskManager.CreateTask(() =>
            {
                if (_config.Listen == null || _config.Listen.Count <= 0) throw new Exception("RPC初始化异常 => 缺少Listen配置");
                string listen = "";
                // 监听地址及端口
                if (_config.Listen.ContainsKey("http"))
                {
                    string[] ipAndPort = _config.Listen["http"].Split(":");
                    var ip = string.IsNullOrEmpty(ipAndPort[0]) ? "0.0.0.0" : ipAndPort[0];
                    var port = int.Parse(ipAndPort[1]);
                    _server.Ports.Add(new ServerPort(ip, port, ServerCredentials.Insecure));
                    listen += $"http://{ip}:{port}";
                }
                if (_config.Listen.ContainsKey("https"))
                {
                    string[] addrAndCert = _config.Listen["https"].Split(";");
                    string[] ipAndPort = addrAndCert[0].Split(":");
                    var ip = string.IsNullOrEmpty(ipAndPort[0]) ? "0.0.0.0" : ipAndPort[0];
                    var port = int.Parse(ipAndPort[1]);
                    string[] certPaths = addrAndCert[1].Split(",");
                    var rootPath = $"{AppDomain.CurrentDomain.BaseDirectory}{_config.ContentRoot}";
                    var rootCert = File.ReadAllText($"{rootPath}{certPaths[0]}");
                    var certChain = File.ReadAllText($"{rootPath}{certPaths[1]}");
                    var privateKey = File.ReadAllText($"{rootPath}{certPaths[2]}");
                    var serverCredentials = new SslServerCredentials(new[] { new KeyCertificatePair(certChain, privateKey) }, rootCert, false);
                    _server.Ports.Add(new ServerPort(ip, port, serverCredentials));
                    if (!string.IsNullOrEmpty(listen)) listen += ",";
                    listen += $"https://{ip}:{port}";
                }
                RegisterFuncFromAssemblies();
                _server.Start();
                _logger?.LogInformationAsync($"RPC服务已开启 => 监听地址:{listen}");
            });
        }

        /// <summary>
        /// 获取gRPC服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceImpl"></param>
        /// <returns></returns>
        private ServerServiceDefinition GetServerServiceDefinition(object serviceImpl)
        {
            var type = serviceImpl.GetType();
            var baseType = type.BaseType;
            if (baseType.ReflectedType == null) throw new Exception("RPC注册异常 => 非gRPC实现");
            var method = baseType.ReflectedType.GetMethod("BindService", new Type[] { baseType });
            if (method == null) throw new Exception("RPC注册异常 => 非gRPC实现");
            return (ServerServiceDefinition)method.Invoke(null, new[] { serviceImpl });
        }

        /// <summary>
        /// 从程序集中注册Rpc函数
        /// </summary>
        private void RegisterFuncFromAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var item in types)
                {
                    var attr = item.GetCustomAttribute<RPCAttribute>();
                    if (attr == null) continue;
                    var service = ServiceHost.Provider.GetService(item);
                    if (service == null) continue;
                    _server.Services.Add(GetServerServiceDefinition(service));
                }
            }
        }
    }
}