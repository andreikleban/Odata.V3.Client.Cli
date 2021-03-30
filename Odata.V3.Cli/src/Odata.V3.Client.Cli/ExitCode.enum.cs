using System;

namespace Odata.V3.Cli
{
    /// <summary>
    /// Microservice command line interface
    /// </summary>
    [Flags]
    public enum ExitCode
    {
        #region Errors
        /// <summary>
        /// Entry assembly isn't found
        /// </summary>
        EntryAssemblyNotFound = -4,

        /// <summary>
        /// File with configuration parameters isn't found
        /// </summary>
        EmptyArgs = -2,

        #endregion Errors

        /// <summary>
        /// Default exit code
        /// </summary>
        Default = 0,

        /// <summary>
        /// Usual launch without special command line arguments
        /// </summary>
        NormalLaunch = 1,

        /// <summary>
        /// Microservice has command lines argument with path to configuration settings 
        /// </summary>
        HasMetadata = 2,

        /// <summary>
        /// Microservice has command lines argument with path to entry assembly
        /// </summary>
        HasOutputDir = 4,

        /// <summary>
        /// Microservice in preparation mode
        /// </summary>
        HasContext = 8,

        /// <summary>
        /// Output directory for preparation mode
        /// </summary>
        HasNamespace = 16,

        /// <summary>
        /// Silent mode of preparation
        /// </summary>
        Verbose = 32,

        /// <summary>
        /// Microservice has command lines argument with path to entry assembly
        /// </summary>
        HasFilename = 64,

        /// <summary>
        /// Microservice has command lines argument with path to entry assembly
        /// </summary>
        HasProxy = 128,

        /// <summary>
        /// Microservice has command lines argument with path to entry assembly
        /// </summary>
        HasPlugins = 256
    }
}
