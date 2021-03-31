using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Odata.V3.Client.Cli.Abstractions
{
    /// <summary>
    /// Proxy classes generator's parameters
    /// </summary>
    public class GeneratorParams
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GeneratorParams()
        {
            Plugins = new List<string>();
        }

        /// <summary>
        /// Metadata document uri
        /// </summary>
        public string MetadataUri { get; set; }

        /// <summary>
        /// Proxy classes namespace
        /// </summary>
        public string NamespacePrefix { get; set; } = "";

        /// <summary>
        /// Proxy classes output directory
        /// </summary>
        public string OutputDir { get; set; } = Directory.GetCurrentDirectory();

        /// <summary>
        /// Filename 
        /// </summary>
        public string OutputFilename { get; set; } = "OdataService";

        /// <summary>
        /// Enables verbose logging
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Enables proxy server settings
        /// </summary>
        public bool IncludeWebProxy { get; set; }

        /// <summary>
        /// Proxy server 
        /// </summary>
        public string WebProxyHost { get; set; }

        /// <summary>
        /// Enables proxy credentials settings
        /// </summary>
        public bool IncludeWebProxyNetworkCredentials { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string WebProxyNetworkCredentialsUsername { get; set; }
        
        /// <summary>
        /// Password
        /// </summary>
        public string WebProxyNetworkCredentialsPassword { get; set; }

        /// <summary>
        /// Proxy server user's domain
        /// </summary>
        public string WebProxyNetworkCredentialsDomain { get; set; }

        /// <summary>
        /// List of plugins names
        /// </summary>
        public IEnumerable<string> Plugins { get; set; }

        /// <summary>
        /// <see cref="IConfiguration"/> instance
        /// </summary>
        public IConfiguration Configuration { get; set; }
    }
}
