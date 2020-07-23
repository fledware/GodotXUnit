using Godot;
using Godot.Collections;

namespace GodotXUnitApi
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
        
        public const int GODOT_RUN_PLAY_SCENE = 39;
        public const string RUNNER_SCENE_PATH = "res://addons/GodotXUnit/runner/GodotTestRunnerScene.tscn";
        public const string EMPTY_SCENE_PATH = "res://addons/GodotXUnit/runner/EmptyScene.tscn";
        public const string DOCK_SCENE_PATH = "res://addons/GodotXUnit/editor/XUnitDock.tscn";
    }
}