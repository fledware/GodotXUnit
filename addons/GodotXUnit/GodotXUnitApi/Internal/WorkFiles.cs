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

        public const string WorkDir = "res://addons/GodotXUnit/_work";

        public static string PathForResFile(string filename)
        {
            var appending = filename.EndsWith(".json") ? filename : $"{filename}.json";
            return $"{WorkDir}/{appending}";
        }

        public static void CleanWorkDir()
        {
            DirAccess.MakeDirRecursiveAbsolute(WorkDir).ThrowIfNotOk();
            var directory = DirAccess.Open(WorkDir).ThrowIfNotOk();
            directory.IncludeHidden = false;
            directory.IncludeNavigational = false;
            try
            {
                directory.ListDirBegin().ThrowIfNotOk();
                while (true)
                {
                    var next = directory.GetNext();
                    if (string.IsNullOrEmpty(next))
                        break;
                    directory.Remove(next).ThrowIfNotOk();
                }
            }
            finally
            {
                directory.ListDirEnd();
            }
        }

        public static void WriteFile(string filename, object contents)
        {
            var writing = JsonConvert.SerializeObject(contents, Formatting.Indented, jsonSettings);
            var file = FileAccess.Open(PathForResFile(filename), FileAccess.ModeFlags.WriteRead).ThrowIfNotOk();
            try
            {
                file.StoreString(writing);
            }
            finally
            {
                file.Close();
            }
        }

        public static object ReadFile(string filename)
        {
            var file = FileAccess.Open(PathForResFile(filename), FileAccess.ModeFlags.Read).ThrowIfNotOk();
            try
            {
                return JsonConvert.DeserializeObject(file.GetAsText(), jsonSettings);
            }
            finally
            {
                file.Close();
            }
        }
    }
}