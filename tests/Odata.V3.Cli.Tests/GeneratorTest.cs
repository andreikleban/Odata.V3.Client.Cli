using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Odata.V3.Client.Cli.Tests
{
    [TestClass]
    public class GeneratorTest
    {
        private const string OutputDir = "TestResults";

        [TestInitialize]
        public void Init()
        {
            if (Directory.Exists(OutputDir))
                Directory.Delete(OutputDir, true);
        }

        [TestMethod]
        [DataRow(new string[] {""})]
        public void EmptyArgs(string[] args)
        {
            Program.Main(args);
        }

        [TestMethod]
        [DataRow(new string[] { "-o", OutputDir, "-v", "-f", "OdataService" })]
        public void WithoutMetadata(string[] args)
        {
            Assert.ThrowsException<ArgumentNullException>(() => Program.Main(args));
        }

        [TestMethod]
        [DataRow(new string[] { "-m", "Assets\\metadataV3.edmx", "-o", OutputDir, "-v", "-f", "OdataService" })]
        public void WithMetadataV3(string[] args)
        {
            Program.Main(args);
            Assert.IsTrue(File.Exists($"{OutputDir}\\OdataService.cs"), "OdataService.cs isn't exist");
            Assert.IsTrue(File.Exists($"{OutputDir}\\OdataService.edmx"), "OdataService.edmx isn't exist");
        }

        [TestMethod]
        [DataRow(new string[] { "-m", "Assets\\metadataV4.edmx", "-o", OutputDir, "-v", "-f", "OdataService" })]
        public void WithMetadataV4(string[] args)
        {
            Assert.ThrowsException<ArgumentException>(() => Program.Main(args));
        }


        [TestMethod]
        [DataRow(new string[] { "-m", "Assets\\metadataV3.edmx", "-o", OutputDir, "-v", "-f", "OdataService", "-pl=Odata.V3.Client.Cli.Tests.dll,Odata.V3.Client.Cli.Tests.TestPlugin" })]
        public void PluginEmptySetting(string[] args)
        {
            Assert.ThrowsException<ArgumentException>(() => Program.Main(args));
        }

        [TestMethod]
        [DataRow(new string[] { "-m", "Assets\\metadataV3.edmx", "-o", OutputDir, "-v", "-f", "OdataService", "-pl=Odata.V3.Client.Cli.Tests.TestPlugin" })]
        public void WrongPluginName(string[] args)
        {
            Assert.ThrowsException<ArgumentException>(() => Program.Main(args));
        }

        [TestMethod]
        [DataRow(new string[] { "-m", "Assets\\metadataV3.edmx", "-o", OutputDir, "-v", "-f", "OdataService", "-pl=Assembly.dll,Odata.V3.Client.Cli.Tests.TestPlugin" })]
        public void WrongPluginAssemblyName(string[] args)
        {
            Assert.ThrowsException<FileNotFoundException>(() => Program.Main(args));
        }

        [TestMethod]
        [DataRow(new string[] { "-m", "Assets\\metadataV3.edmx", "-o", OutputDir, "-v", "-f", "OdataService", "-pl=Odata.V3.Client.Cli.Tests.dll,Odata.V3.Client.Cli.Tests.WrongTestPlugin" })]
        public void WrongPluginTypeName(string[] args)
        {
            Assert.ThrowsException<TypeLoadException>(() => Program.Main(args));
        }


        [TestMethod]
        [DataRow(new string[] { "-m", "Assets\\metadataV3.edmx", "-o", OutputDir, "-v", "-f", "OdataService", "-pl=Odata.V3.Client.Cli.Tests.dll,Odata.V3.Client.Cli.Tests.TestPlugin", "testSetting=testString" })]
        public void PluginTest(string[] args)
        {
            Program.Main(args);
            var file = File.ReadAllText($"{OutputDir}\\OdataService.cs");
            Assert.IsTrue(file.Contains("testString"));
        }
    }
}
