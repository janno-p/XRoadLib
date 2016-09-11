using System.IO;

namespace XRoadLib.Tools
{
    public class GeneratorOptions
    {
        public string WsdlUri { get; set; }
        public bool IsVerbose { get; set; }
        public bool IsCodeOutput { get; set; }
        public DirectoryInfo OutputPath { get; set; }
        public FileInfo MappingPath { get; set; }
    }
}