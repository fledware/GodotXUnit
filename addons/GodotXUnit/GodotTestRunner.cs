using System;
using System.Reflection;
using Godot;
using GodotXUnitApi;
using GodotXUnitApi.Internal;

namespace GodotXUnit
{
    public class GodotTestRunner : GodotXUnitRunner
    {
        protected override Assembly GetTargetAssembly(GodotXUnitSummary summary)
        {
            // check if we even have a project assembly set
            if (!ProjectSettings.HasSetting(Consts.SETTING_TARGET_ASSEMBLY))
                return Assembly.GetExecutingAssembly();
            
            // get the project and if its the default (the godot project), return executing
            var targetProject = ProjectSettings.GetSetting(Consts.SETTING_TARGET_ASSEMBLY).ToString();
            if (string.IsNullOrEmpty(targetProject) || targetProject.Equals(ProjectListing.GetDefaultProject()))
                return Assembly.GetExecutingAssembly();

            // if its a custom project target, attempt to just load the assembly directly
            if (targetProject.Equals(Consts.SETTING_TARGET_ASSEMBLY_CUSTOM_FLAG))
            {
                var customDll = ProjectSettings.HasSetting(Consts.SETTING_TARGET_ASSEMBLY_CUSTOM)
                    ? ProjectSettings.GetSetting(Consts.SETTING_TARGET_ASSEMBLY_CUSTOM).ToString()
                    : "";
                if (string.IsNullOrEmpty(customDll))
                {
                    summary.AddDiagnostic("no custom dll assembly configured.");
                    GD.PrintErr("no custom dll assembly configured.");
                    return Assembly.GetExecutingAssembly();
                }
                summary.AddDiagnostic($"attempting to load custom dll at: {customDll}");
                return AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(customDll));
            }
            
            // find the project in the project list. if its not there, print error and leave
            var projectList = ProjectListing.GetProjectInfo();
            if (!projectList.ContainsKey(targetProject))
            {
                GD.PrintErr($"unable to find project {targetProject}. expected values: {string.Join(", ", projectList.Keys)}");
                return Assembly.GetExecutingAssembly();
            }
            
            // finally, attempt to load project..
            var name = AssemblyName.GetAssemblyName(projectList[targetProject].GetDllFromCsProjectPath());
            return AppDomain.CurrentDomain.Load(name);
        }

        protected override string GetTargetClass(GodotXUnitSummary summary)
        {
            return ProjectSettings.HasSetting(Consts.SETTING_TARGET_CLASS)
                ? ProjectSettings.GetSetting(Consts.SETTING_TARGET_CLASS)?.ToString()
                : null;
        }

        protected override string GetTargetMethod(GodotXUnitSummary summary)
        {
            return ProjectSettings.HasSetting(Consts.SETTING_TARGET_METHOD)
                ? ProjectSettings.GetSetting(Consts.SETTING_TARGET_METHOD)?.ToString()
                : null;
        }
    }
}
