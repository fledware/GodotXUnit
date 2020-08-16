// using System;
//
// namespace GodotXUnitApi.Internal
// {
//     [Serializable]
//     public class RunArgs
//     {
//         public string classToRun;
//         public string methodToRun;
//
//         public string FilterConfig
//         {
//             get
//             {
//                 if (string.IsNullOrEmpty(classToRun))
//                     return null;
//                 if (string.IsNullOrEmpty(methodToRun))
//                     return classToRun;
//                 return $"{classToRun}.{methodToRun}";
//             }
//         }
//     }
//
//     public static class RunArgsHelper
//     {
//         public static string filename = "RunArgs";
//
//         public static RunArgs Read()
//         {
//             return (RunArgs) WorkFiles.ReadFile(filename) ?? new RunArgs();
//         }
//         
//         public static void ClearRunArgs()
//         {
//             WorkFiles.DeleteFile(filename);
//         }
//
//         public static void RunClass(string className, string methodName)
//         {
//             WorkFiles.WriteFile(filename, new RunArgs
//             {
//                 classToRun = className,
//                 methodToRun = methodName
//             });
//         }
//     }
// }