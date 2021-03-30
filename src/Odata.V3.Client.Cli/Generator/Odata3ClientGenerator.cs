using System;
using System.Data.Services.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Xml;
using Microsoft.Extensions.Logging;
using Odata.V3.Cli.Abstractions;

namespace Odata.V3.Cli.Generator
{
    public class Odata3ClientGenerator
    {
        private readonly ILogger _logger;

        public Odata3ClientGenerator(ILogger logger)
        {
            _logger = logger;
        }

        private static string GetMetadata(GeneratorParams generatorParams, out Version edmxVersion)
        {
            if (string.IsNullOrEmpty(generatorParams.MetadataUri))
                throw new ArgumentNullException("OData Service Endpoint", "Please input the service endpoint");

            if (File.Exists(generatorParams.MetadataUri))
                generatorParams.MetadataUri = new FileInfo(generatorParams.MetadataUri).FullName;

            if (generatorParams.MetadataUri.StartsWith("https:", StringComparison.Ordinal)
                || generatorParams.MetadataUri.StartsWith("http", StringComparison.Ordinal))
            {
                if (!generatorParams.MetadataUri.EndsWith("$metadata", StringComparison.Ordinal))
                {
                    generatorParams.MetadataUri = generatorParams.MetadataUri.TrimEnd('/') + "/$metadata";
                }
            }

            Stream metadataStream;
            var metadataUri = new Uri(generatorParams.MetadataUri);
            if (!metadataUri.IsFile)
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(metadataUri);

                if (generatorParams.IncludeWebProxy)
                {
                    var proxy = new WebProxy(generatorParams.WebProxyHost);
                    if (generatorParams.IncludeWebProxyNetworkCredentials)
                    {
                        proxy.Credentials = new NetworkCredential(
                            generatorParams.WebProxyNetworkCredentialsUsername,
                            generatorParams.WebProxyNetworkCredentialsPassword,
                            generatorParams.WebProxyNetworkCredentialsDomain);
                    }

                    webRequest.Proxy = proxy;
                }

                WebResponse webResponse = webRequest.GetResponse();
                metadataStream = webResponse.GetResponseStream();
            }
            else
            {
                // Set up XML secure resolver
                var xmlUrlResolver = new XmlUrlResolver
                {
                    Credentials = CredentialCache.DefaultNetworkCredentials
                };

                metadataStream = (Stream)xmlUrlResolver.GetEntity(metadataUri, null, typeof(Stream));
            }

            var workFile = Path.GetTempFileName();

            try
            {
                using (XmlReader reader = XmlReader.Create(metadataStream))
                {
                    using (var writer = XmlWriter.Create(workFile))
                    {
                        while (reader.NodeType != XmlNodeType.Element)
                        {
                            reader.Read();
                        }

                        if (reader.EOF)
                        {
                            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The metadata is an empty file"));
                        }

                        Constants.SupportedEdmxNamespaces.TryGetValue(reader.NamespaceURI, out edmxVersion);
                        writer.WriteNode(reader, false);
                    }
                }
                return workFile;
            }
            catch (WebException e)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot access {0}", generatorParams.MetadataUri), e);
            }
        }

        public void GenerateClient(GeneratorParams generatorParams)
        { 
            _logger.LogInformation("Generating Client Proxy ...");

            try
            {
                var edmxTmpFile = GetMetadata(generatorParams, out var version);

                if (version == Constants.EdmxVersion4)
                    throw new ArgumentException($"Wrong EDMX version. Current version={version}");

                var generator = new EntityClassGenerator(LanguageOption.GenerateCSharpCode)
                {
                    UseDataServiceCollection = true,
                    Version = DataServiceCodeVersion.V3
                };

                // Set up XML secure resolver
                var xmlUrlResolver = new XmlUrlResolver
                {
                    Credentials = CredentialCache.DefaultNetworkCredentials
                };

                var permissionSet = new PermissionSet(System.Security.Permissions.PermissionState.Unrestricted);

                var settings = new XmlReaderSettings
                {
                    XmlResolver = new XmlSecureResolver(xmlUrlResolver, permissionSet)
                };

                var fileHandler = new FilesHandler();

                using (var reader = XmlReader.Create(generatorParams.MetadataUri, settings))
                {
                    var tempFile = Path.GetTempFileName();
                    var noErrors = true;

                    using (StreamWriter writer = File.CreateText(tempFile))
                    {
                        var errors = generator.GenerateCode(reader, writer, generatorParams.NamespacePrefix);
                        writer.Flush();

                        if (errors != null && errors.Any())
                        {
                            noErrors = false;

                            foreach (var err in errors)
                                _logger.LogError(err.Message);
                            _logger.LogError("Client Proxy for OData V3 was not generated.");
                        }
                    }

                    if (noErrors)
                    {
                        var csFile = new FileInfo(Path.Combine(generatorParams.OutputPath, generatorParams.Filename + ".cs"));
                        _logger.LogInformation($"Writing file {csFile.FullName}");
                        fileHandler.AddFileAsync(tempFile, csFile.FullName).ConfigureAwait(true);

                        var edmxFile = new FileInfo(Path.Combine(generatorParams.OutputPath, generatorParams.Filename + ".edmx"));
                        _logger.LogInformation($"Writing file {edmxFile.FullName}");
                        fileHandler.AddFileAsync(edmxTmpFile, edmxFile.FullName).ConfigureAwait(true);

                        foreach (var pluginCommand in generatorParams.Plugins)
                        {
                            var plugin = PluginInitilaizer.Create(_logger, generatorParams, pluginCommand);
                            plugin.PostProcess();
                        }
                    }
                }

                _logger.LogInformation("Client Proxy for OData V3 was generated.");
            }
            catch (Exception e)
            {
                _logger.LogCritical("Errors during generation Client Proxy for OData V3", e);
                throw;
            }
        }
    }
}
