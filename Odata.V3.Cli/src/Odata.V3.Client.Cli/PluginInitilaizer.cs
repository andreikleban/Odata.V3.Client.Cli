using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Odata.V3.Cli.Abstractions;

namespace Odata.V3.Cli
{
    internal static class PluginInitilaizer
    {
        // Assembly.dll,Namespace.PluginClass

        internal static Plugin Create(ILogger logger, GeneratorParams generatorParams, string command)
        {
            var args = command.Split(',');

            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), args[0])))
                throw new FileNotFoundException($"Plugin hasn't found", args[0]);

            try
            {
                var assembly =  Assembly.LoadFrom(args[0]);
                var pluginType = assembly.GetType(args[1], true, true);
                var plugin = Activator.CreateInstance(pluginType, logger, generatorParams) as Plugin;
                return plugin;
            }
            catch (Exception e)
            {
                logger.LogError($"Plugin {args[0]} error", e);
                throw;
            }
        }
    }
}
