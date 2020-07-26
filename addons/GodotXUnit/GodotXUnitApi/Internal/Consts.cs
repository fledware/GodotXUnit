using Godot;
using Godot.Collections;

namespace GodotXUnitApi.Internal
{
    public static class Consts
    {
        public const string SETTING_RESULTS_SUMMARY = "GodotXUnit/results_summary";
        public static readonly string SETTING_RESULTS_SUMMARY_DEF = "res://TestSummary.json";
        public static readonly Dictionary SETTING_RESULT_SUMMARY_PROP = new Dictionary
        {
            ["name"] = SETTING_RESULTS_SUMMARY,
            ["type"] = Variant.Type.String,
            ["hint"] = PropertyHint.Dir,
            ["hint_string"] = "set the name of the test assembly, or empty string for main assembly",
            ["default"] = SETTING_RESULTS_SUMMARY_DEF
        };
        
        public static readonly string SETTING_TARGET_ASSEMBLY = "GodotXUnit/target_assembly";
        
        public const string RUNNER_SCENE_PATH = "res://addons/GodotXUnit/runner/GodotTestRunnerScene.tscn";
        public const string EMPTY_SCENE_PATH = "res://addons/GodotXUnit/runner/EmptyScene.tscn";
        public const string DOCK_SCENE_PATH = "res://addons/GodotXUnit/XUnitDock.tscn";

        public const string ICON_RUNNING = "res://addons/GodotXUnit/assets/running.png";
        public const string ICON_WARN = "res://addons/GodotXUnit/assets/warn.png";
        public const string ICON_CHECK = "res://addons/GodotXUnit/assets/check.png";
        public const string ICON_ERROR = "res://addons/GodotXUnit/assets/error.png";
        
        public static Texture IconRunning => GD.Load<Texture>(ICON_RUNNING);
        public static Texture IconWarn => GD.Load<Texture>(ICON_WARN);
        public static Texture IconCheck => GD.Load<Texture>(ICON_CHECK);
        public static Texture IconError => GD.Load<Texture>(ICON_ERROR);
    }
}