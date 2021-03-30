using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Odata.V3.Cli.Tests
{
    [TestClass]
    public class GeneratorTest
    {
        private const string OutputDir = "TestResults";

        [TestInitialize]
        public void Init()
        {
            Directory.Delete(OutputDir, true);
        }

        [TestMethod]
        [DataRow(new string[] { "-m","Assets\\metadata.edmx", 
                                "-o", OutputDir,
                                "-v",
                                "-f", "OdataService"})]
        public void GenerationTest(string[] args)
        {
            Program.Main(args);
            Assert.IsTrue(File.Exists("TestResults\\OdataService.cs"));
            Assert.IsTrue(File.Exists("TestResults\\OdataService.edmx"));
        }
    }
}
