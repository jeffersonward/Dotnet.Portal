using System.Collections.Generic;
using System.IO;

namespace Dotnet.Portal.SolutionReader.Models
{
    public class Solution
    {
        public string Name { get; set; }
        public FileInfo File { get; set; }
        public List<Project> Projects { get; set; }
    }
}