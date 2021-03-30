using System;
using System.Data.Services.Design;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Xml;
using Microsoft.Extensions.Logging;
using Odata.V3.Cli.Abstractions;
using Odata.V3.Cli.Properties;

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
                throw new ArgumentNullException("OData Service Endpoint", Resources.Please_input_the_metadata_document_address);

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
                            throw new InvalidOperationException(Resources.The_metadata_is_an_empty_file);
                        }

                        Constants.SupportedEdmxNamespaces.TryGetValue(reader.NamespaceURI, out edmxVersion);
                        writer.WriteNode(reader, false);
                    }
                }
                return workFile;
            }
            catch (WebException e)
            {
                throw new InvalidOperationException(string.Format(Resources.Cannot_access_metadata, generatorParams.MetadataUri), e);
            }
        }

        public void GenerateClientProxyClasses(GeneratorParams generatorParams)
        { 
            _logger.LogInformation(Resources.Generating_Client_Proxy____);

            try
            {
                var edmxTmpFile = GetMetadata(generatorParams, out var version);

                if (version == Constants.EdmxVersion4)
                    throw new ArgumentException(string.Format(Resources.Wrong_edx_version, version));

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
                            _logger.LogError(Resources.Client_Proxy_for_OData_V3_was_not_generated_);
                        }
                    }

                    if (noErrors)
                    {
                        var csFile = new FileInfo(Path.Combine(generatorParams.OutputPath, generatorParams.Filename + ".cs"));
                        _logger.LogInformation(string.Format(Resources.Writing_file__0_, csFile.FullName));
                        fileHandler.AddFileAsync(tempFile, csFile.FullName).ConfigureAwait(true);

                        var edmxFile = new FileInfo(Path.Combine(generatorParams.OutputPath, generatorParams.Filename + ".edmx"));
                        _logger.LogInformation(string.Format(Resources.Writing_file__0_, edmxFile.FullName));
                        fileHandler.AddFileAsync(edmxTmpFile, edmxFile.FullName).ConfigureAwait(true);

                        foreach (var pluginCommand in generatorParams.Plugins)
                        {
                            var plugin = PluginCreator.Create(_logger, generatorParams, pluginCommand);
                            plugin.PostProcess();
                        }
                    }
                }

                _logger.LogInformation(Resources.Client_Proxy_for_OData_V3_was_generated_);
            }
            catch (Exception e)
            {
                _logger.LogCritical(Resources.Errors_during_generation_Client_Proxy_for_OData_V3, e);
                throw;
            }
        }
    }
}
