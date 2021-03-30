using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Odata.V3.Cli.Abstractions
{
    public class GeneratorParams
    {
        public GeneratorParams()
        {
            Plugins = new List<string>();
        }

        public string MetadataUri { get; set; }
        public string NamespacePrefix { get; set; } = "";
        public string OutputPath { get; set; } = Directory.GetCurrentDirectory();

        public string Filename { get; set; } = "OdataService";

        public bool Verbose { get; set; }

        public bool IncludeWebProxy { get; set; }
        public string WebProxyHost { get; set; }
        public bool IncludeWebProxyNetworkCredentials { get; set; }
        public string WebProxyNetworkCredentialsUsername { get; set; }
        public string WebProxyNetworkCredentialsPassword { get; set; }
        public string WebProxyNetworkCredentialsDomain { get; set; }

        public IEnumerable<string> Plugins { get; set; }

        public IConfiguration Configuration { get; set; }
    }
}
