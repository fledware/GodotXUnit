using System;

namespace GodotXUnitApi
{
    [Serializable]
    public class RunArgs
    {
        public string classToRun;
    }

    public static class RunArgsHelper
    {
        public static string filename = "RunArgs";

        public static RunArgs Read()
        {
            return (RunArgs) WorkFiles.ReadFile(filename) ?? new RunArgs();
        }
        
        public static void ClearRunArgs()
        {
            WorkFiles.DeleteFile(filename);
        }

        public static void RunClass(string className)
        {
            WorkFiles.WriteFile(filename, new RunArgs
            {
                classToRun = className
            });
        }
    }
}