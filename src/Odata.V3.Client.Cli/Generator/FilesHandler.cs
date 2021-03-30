using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Odata.V3.Cli.Generator
{
    public class FilesHandler// : ConnectedServiceHandlerHelper
    {
        public FilesHandler() : base()
        {
            AddedFiles = new List<(string CreatedFile, string SourceFile)>();
            TokenReplacementValues = new Dictionary<string, string>();
        }

        public IList<(string CreatedFile, string SourceFile)> AddedFiles { get; private set; }
        // used to access the temp file that the generated code was written to
        public string AddedFileInputFileName { get; private set; }
        // used to find out which file the final output would be written to
        public string AddedFileTargetFilePath { get; private set; }
        public string ServicesRootFolder { get; set; }
        public Task<string> AddFileAsync(string fileName, string targetPath)
        {
            AddedFileInputFileName = fileName;
            AddedFileTargetFilePath = targetPath;
            AddedFiles.Add((targetPath, fileName));

            var content = File.ReadAllText(fileName);
            foreach (var token in TokenReplacementValues)
                content = content.Replace(token.Key, token.Value);

            File.WriteAllText(targetPath, content);
            return Task.FromResult(string.Empty);
        }
        public IDictionary<string, string> TokenReplacementValues { get; }
        public void AddAssemblyReference(string assemblyPath) =>
            throw new System.NotImplementedException();
        public string GetServiceArtifactsRootFolder() => ServicesRootFolder;
        public string PerformTokenReplacement(string input, IDictionary<string, string> additionalReplacementValues = null) =>
            throw new System.NotImplementedException();
    }
}
