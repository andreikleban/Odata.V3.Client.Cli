using Microsoft.Extensions.Logging;

namespace Odata.V3.Cli.Abstractions
{
    public abstract class Plugin
    {
        private Plugin()
        {}

        protected Plugin(ILogger logger, GeneratorParams generatorParams)
        {
            Logger = logger;
            GeneratorParams = generatorParams;
        }

        protected ILogger Logger { get; }
        protected GeneratorParams GeneratorParams { get;}

        public abstract void PostProcess();
    }
}
