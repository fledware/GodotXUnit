using System;
using System.Reflection;
using Godot;
using GodotXUnitApi.Internal;

namespace GodotXUnit
{
    public class GodotTestRunner : GodotXUnitRunner
    {
        protected override Assembly GetAssemblyToTest()
        {
            if (ProjectSettings.HasSetting(Consts.SETTING_TARGET_ASSEMBLY))
            {
                GD.PrintErr("loading a sub project for unit tests dont run really well yet... defaulting to executing assembly");
            }
            // string assemblyLoading = ProjectSettings.GetSetting("GodotXUnit/target_assembly")?.ToString() ?? "";
            // if (string.IsNullOrEmpty(assemblyLoading))
            // {
                return Assembly.GetExecutingAssembly();
            // }
            //
            //
            // if (assemblyLoading.StartsWith("res://"))
            // {
            //     assemblyLoading = assemblyLoading.Replace("res://", System.IO.Directory.GetCurrentDirectory() + "/");
            // }
            // // if (assemblyLoading.EndsWith(".dll"))
            // // {
            // // 	assemblyLoading = assemblyLoading.Replace(".dll", "");
            // // }
            // var loading = Assembly.LoadFrom(assemblyLoading);
            // // foreach (AssemblyName required in loading.GetReferencedAssemblies())
            // // {
            // // 	AppDomain.CurrentDomain.Load(required);
            // // }
            // return loading;
        }
    }
}
