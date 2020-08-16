using System.Collections.Generic;
using System.IO;
using Godot;
using Directory = System.IO.Directory;
using GodotDirectory = Godot.Directory;
using GodotPath = Godot.Path;
using Path = System.IO.Path;

namespace GodotXUnitApi.Internal
{
    public static class ProjectListing
    {
        public static List<string> GetProjectList()
        {
            var result = new List<string>();
            foreach (var filename in Directory.GetFiles(WorkFiles.ProjectDir, "*.csproj", SearchOption.AllDirectories))
            {
                if (filename.Contains("GodotXUnitApi"))
                    continue;
                result.Add(Path.GetFileNameWithoutExtension(filename));
            }
            return result;
        }
        
        public static Dictionary<string, string> GetProjectInfo()
        {
            var result = new Dictionary<string, string>();
            foreach (var filename in Directory.GetFiles(WorkFiles.ProjectDir, "*.csproj", SearchOption.AllDirectories))
            {
                if (filename.Contains("GodotXUnitApi"))
                    continue;
                result[Path.GetFileNameWithoutExtension(filename)] = filename;
            }
            return result;
        }

        public static string GetDefaultProject()
        {
            var project = Directory.GetFiles(WorkFiles.ProjectDir, "*.csproj", SearchOption.TopDirectoryOnly);
            if (project.Length == 0)
            {
                GD.PrintErr($"no csproj found on project root at {WorkFiles.ProjectDir}. is this a mono project?");
                return "";
            }
            if (project.Length > 1)
            {
                GD.PrintErr($"multiple csproj found on project root at {WorkFiles.ProjectDir}.");
                return "";
            }
            return Path.GetFileNameWithoutExtension(project[0]);
        }

        public static string GetDllFromCsProjectPath(this string projectPath)
        {
            var projectRoot = Path.GetDirectoryName(projectPath);
            var projectName = Path.GetFileNameWithoutExtension(projectPath);
            return string.Join(WorkFiles.sep, projectRoot, "bin", "Debug", $"{projectName}.dll");
        }
    }
}