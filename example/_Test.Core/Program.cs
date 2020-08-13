﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using DwFramework.Core;
using DwFramework.Core.Plugins;
using DwFramework.Core.Extensions;

namespace _Test.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var cache = new MemoryCache();
                for (var i = 0; i < 100; i++)
                {
                    cache.Set($"t{i}", i);
                }
                var s = cache.KeysWhere(".*2$");

                //var host = new ServiceHost();

                //host.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }
    }
}