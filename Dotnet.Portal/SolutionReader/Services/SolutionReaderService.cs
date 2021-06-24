using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Dotnet.Portal.SolutionReader.Models;

namespace Dotnet.Portal.SolutionReader.Services
{
    public class SolutionReaderService
    {
        public Solution Read(string path)
        {
            var file = new FileInfo(path);
            var solution = new Solution
            {
                Name = Path.GetFileNameWithoutExtension(file.Name),
                File = file,
                Projects = new List<Project>()
            };

            var allProjects = new List<string>();

            var projectFiles = GetProjectFiles(solution)
                    .Where(x => !allProjects.Contains(x.FullName.ToLowerInvariant()))
                    .Where(IsNetcoreWebApp);

            foreach (var projectFile in projectFiles)
            {
                allProjects.Add(projectFile.FullName.ToLowerInvariant());
                solution.Projects.Add(ToProject(projectFile));
            }

            return solution;
        }

        private static IEnumerable<FileInfo> GetProjectFiles(Solution solution)
        {
            var contents = File.ReadAllLines(solution.File.FullName);
            return contents.Where(x => x.StartsWith("Project(\"{"))
                .Select(x => x.Split(new[] { " = " }, StringSplitOptions.None)[1].Split(',')[1].Replace('"', ' ').Trim())
                .Select(x => Path.Combine(solution.File.DirectoryName ?? "", x))
                .Select(x => new FileInfo(x));
        }

        private static bool IsNetcoreWebApp(FileInfo file)
        {
            try
            {
                var projectXml = XDocument.Load(file.OpenText());
                var webProjectTargetFramework = projectXml.XPathSelectElement("/Project[@Sdk='Microsoft.NET.Sdk.Web']/PropertyGroup/TargetFramework");
                return webProjectTargetFramework != null && webProjectTargetFramework.Value.ToLowerInvariant().StartsWith("netcoreapp");
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static Project ToProject(FileSystemInfo file)
        {
            return new Project { Name = Path.GetFileNameWithoutExtension(file.Name), File = file.FullName };
        }
    }
}