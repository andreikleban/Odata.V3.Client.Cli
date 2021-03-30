﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Odata.V3.Cli.Abstractions;
using Odata.V3.Cli.Generator;

namespace Odata.V3.Cli
{
    public class Program
    {
        private static CommandLineApplication _commandLine;
        private static CommandOption _metadatUriOption;
        private static CommandOption _outputDirOption;
        private static CommandOption _namespaceOption;
        private static CommandOption _verboseOption;

        private static ILoggerFactory _loggerFactory;
        private static CommandOption _proxyOption;
        private static GeneratorParams _generatorParams;
        private static CommandOption _filenameOption;
        private static CommandOption _pluginsOption;

        public static void Main(string[] args)
        {
            _commandLine = new CommandLineApplication(false);
            _commandLine.Description = "Client Proxy generator for OData V3";
            _commandLine.Name = typeof(Program).Assembly.GetName().Name;

            _commandLine.HelpOption("-?|-h|--help");
            var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            _commandLine.VersionOption("--version", () => version, () => $"{_commandLine.Description}\n{version}");
            _metadatUriOption = _commandLine.Option("-m|--metadata|--metadatauri", "Metadata address or local path", CommandOptionType.SingleValue);
            _outputDirOption = _commandLine.Option("-o|--outputdir", "Full path to output directory", CommandOptionType.SingleValue);
            _filenameOption = _commandLine.Option("-f|--filename", "Output file name", CommandOptionType.SingleValue);
            _namespaceOption = _commandLine.Option("-ns|--namespace", "Namespace prefix", CommandOptionType.SingleValue);
            _verboseOption = _commandLine.Option("-v|--verbose", "Verbose", CommandOptionType.NoValue);
            _proxyOption = _commandLine.Option("-p|--proxy", "Proxy settings. Format: domain\\user:password@SERVER:PORT", CommandOptionType.SingleValue);
            _pluginsOption = _commandLine.Option("-pl|--plugins", "List of plugins", CommandOptionType.MultipleValue);

            _generatorParams = new GeneratorParams();

            _commandLine.OnExecute(Parse);

            try
            {
                var commandLineParseResult = (ExitCode) _commandLine.Execute(args);

                // ExitCode.Default возвращается при наличии параметров -h или --version
                if (commandLineParseResult <= ExitCode.Default)
                    return;

                _loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder
                        .AddFilter(_commandLine.Name, _verboseOption.HasValue() ? LogLevel.Debug : LogLevel.None)
                        .AddConsole(options =>
                        {
                            options.Format = ConsoleLoggerFormat.Default;
                            options.TimestampFormat = "hh:mm:ss ";
                            options.IncludeScopes = false;
                        });
                });

                _generatorParams.Configuration = new ConfigurationBuilder().AddCommandLine(_commandLine.RemainingArguments.ToArray()).Build();

                var generator = new Odata3ClientGenerator(_loggerFactory.CreateLogger(_commandLine.Name));
                generator.GenerateClient(_generatorParams);

            }
            catch (CommandParsingException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            args = _commandLine.RemainingArguments.ToArray();
        }

        private static int Parse()
        {
            ExitCode result = ExitCode.NormalLaunch;

            if (!_commandLine.GetOptions().Any(o => o.HasValue()))
            {
                _commandLine.ShowHint();
                return (int) ExitCode.EmptyArgs;
            }

            //опция -m
            if (_metadatUriOption.HasValue())
            {
                _generatorParams.MetadataUri = _metadatUriOption.Value();
                result |= ExitCode.HasMetadata;
            }

            //опция -o
            if (_outputDirOption.HasValue())
            {
                if (!Directory.Exists(_outputDirOption.Value()))
                    Directory.CreateDirectory(_outputDirOption.Value());

                _generatorParams.OutputPath = _outputDirOption.Value();
                result |= ExitCode.HasOutputDir;
            }

            //опция -f
            if (_filenameOption.HasValue())
            {
                _generatorParams.Filename = _filenameOption.Value();
                result |= ExitCode.HasFilename;
            }

            //опция -ns
            if (_namespaceOption.HasValue())
            {
                _generatorParams.NamespacePrefix = _namespaceOption.Value();
                result |= ExitCode.HasNamespace;
            }

            if (_verboseOption.HasValue())
            {
                _generatorParams.Verbose = _verboseOption.HasValue();
                result |= ExitCode.Verbose;
            }

            if (_proxyOption.HasValue())
            {
                var proxy = _proxyOption.Value();
                var proxyParts = proxy.Split('@');

                var server = string.Empty;
                var person = string.Empty;

                if (proxyParts.Length == 1)
                    server = proxyParts[0];
                else if (proxyParts.Length == 2)
                {
                    person = proxyParts[0];
                    server = proxyParts[1];
                }

                if (!string.IsNullOrWhiteSpace(server))
                {
                    _generatorParams.WebProxyHost = server;
                    if (!string.IsNullOrWhiteSpace(person))
                    {
                        var personParts = person.Split(':');
                        _generatorParams.WebProxyNetworkCredentialsPassword = personParts[1];

                        var userParts = personParts[0].Split('\\', '/');
                        _generatorParams.WebProxyNetworkCredentialsDomain = userParts[0];
                        _generatorParams.WebProxyNetworkCredentialsUsername = userParts[1];
                        _generatorParams.IncludeWebProxyNetworkCredentials = true;
                    }
                    _generatorParams.IncludeWebProxy = true;
                }

                result |= ExitCode.HasProxy;
            }

            if (_pluginsOption.HasValue())
            {
                _generatorParams.Plugins = _pluginsOption.Values;
                result |= ExitCode.HasPlugins;
            }

            return (int)result;
        }
    }
}
