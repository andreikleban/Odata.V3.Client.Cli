using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Odata.V3.Cli.Abstractions;
using Odata.V3.Cli.Properties;

namespace Odata.V3.Cli
{
    internal static class PluginCreator
    {
        internal static Plugin Create(ILogger logger, GeneratorParams generatorParams, string command)
        {
            var args = command.Split(',');
            if (args.Length < 2)
                throw new ArgumentException("Incorrect plugin name. Needed format: Assembly.dll,Namespace.Class");

            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), args[0])))
                throw new FileNotFoundException(Resources.Plugin_assembly_hasn_t_found, args[0]);

            try
            {
                var assembly =  Assembly.LoadFrom(args[0]);
                var pluginType = assembly.GetType(args[1], true, true);
                var plugin = Activator.CreateInstance(pluginType, logger, generatorParams) as Plugin;
                return plugin;
            }
            catch (Exception e)
            {
                logger.LogError(string.Format(Resources.Plugin_creation__0__error, args[0]), e);
                throw;
            }
        }
    }
}
