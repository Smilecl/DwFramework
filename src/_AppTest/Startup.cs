﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using DwFramework.Core;
using DwFramework.Core.Extensions;
using DwFramework.Core.Plugins;
using DwFramework.WebAPI.Swagger;
using DwFramework.WebAPI.RequestFilter;
using DwFramework.WebAPI.Jwt;

namespace _AppTest
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;

        public Startup()
        {
            _logger = ServiceHost.Provider.GetLogger<Startup>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            if (ServiceHost.Environment.EnvironmentType == EnvironmentType.Develop)
                services.AddSwagger("IndexServiceDoc", "索引服务", "v1");
            // JWT插件
            services.AddJwtAuthentication(new DefaultJwtTokenValidator("fc3d06d9b75f92b648ab4e372dfd22f2"), context =>
            {
                Console.WriteLine("Success");
                return Task.CompletedTask;
            }, context =>
            {
                Console.WriteLine("Fail");
                return Task.CompletedTask;
            });
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime lifetime)
        {
            // 请求过滤器
            app.UseRequestFilter(new Dictionary<string, Action<HttpContext>>
            {
                // 请求日志
                {"/*",context =>_logger.LogDebug($"接收到请求:{context.Request.Path} ({GetIP(context)})")}
            }, async (context, ex) =>
            {
                _logger.LogDebug(ex.Message);
                context.Response.Headers.Add("Content-type", "application/json");
                await context.Response.WriteAsync(ResultInfo.Fail(ex.Message).ToJson());
                return;
            });
            if (ServiceHost.Environment.EnvironmentType == EnvironmentType.Develop)
                app.UseSwagger("IndexServiceDoc", "索引服务");
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private string GetIP(HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(ip))
                ip = IPAddress.Parse(ip).MapToIPv4().ToString();
            if (string.IsNullOrEmpty(ip))
                ip = context.Connection.RemoteIpAddress.MapToIPv4().ToString();
            return ip;
        }
    }
}
