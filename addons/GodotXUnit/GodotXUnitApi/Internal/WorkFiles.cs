using System;
using Godot;
using Newtonsoft.Json;

namespace GodotXUnitApi.Internal
{
    public static class WorkFiles
    {
        public static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };
        
        public static readonly string sep =
            System.IO.Path.DirectorySeparatorChar.ToString();

        private static string _projectDir;

        public static string ProjectDir
        {
            get
            {
                if (!string.IsNullOrEmpty(_projectDir))
                    return _projectDir;
                var current = System.IO.Directory.GetCurrentDirectory();
                while (!string.IsNullOrEmpty(current))
                {
                    if (System.IO.File.Exists($"{current}{sep}project.godot"))
                    {
                        _projectDir = current;
                        return _projectDir;
                    }
                    current = System.IO.Directory.GetParent(current).FullName;
                }
                GD.PrintErr("unable to find root of godot project");
                throw new Exception("unable to find root dir");
                
                // TODO: if this becomes a problem, we can do OS.Execute('pwd'....), but i don't
                // want to do that if we don't need to.
            }
        }

        private static string _passDir;

        public static string PassDir => _passDir ??= $"{ProjectDir}{sep}addons{sep}GodotXUnit{sep}_work";

        public static string PathForFile(string filename)
        {
            var appending = filename.EndsWith(".json") ? filename : $"{filename}.json";
            return $"{PassDir}{sep}{appending}";
        }

        public static void CleanWorkDir()
        {
            var directory = new Godot.Directory();
            directory.MakeDirRecursive(PassDir).ThrowIfNotOk();
            directory.Open(PassDir).ThrowIfNotOk();
            directory.ListDirBegin().ThrowIfNotOk();
            while (true)
            {
                var next = directory.GetNext();
                if (next.EndsWith(".json"))
                {
                    directory.Remove(next).ThrowIfNotOk();
                }
                if (string.IsNullOrEmpty(next))
                    break;
            }
            directory.ListDirEnd();
        }

        public static void WriteFile(string filename, object contents)
        {
            var fullPath = PathForFile(filename);
            var writing = JsonConvert.SerializeObject(contents, Formatting.Indented, jsonSettings);
            System.IO.File.WriteAllText(fullPath, writing);
        }

        public static object ReadFile(string filename, bool delete = false)
        {
            var fullPath = PathForFile(filename);
            if (System.IO.File.Exists(fullPath))
            {
                var rawText = System.IO.File.ReadAllText(fullPath);
                if (delete)
                    System.IO.File.Delete(fullPath);
                return JsonConvert.DeserializeObject(rawText, jsonSettings);
            }
            return null;
        }

        public static void DeleteFile(string filename)
        {
            var fullPath = PathForFile(filename);
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
    }
}