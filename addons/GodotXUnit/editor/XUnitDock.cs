using Godot;
using GodotXUnitApi;

namespace GodotXUnit.editor
{
    [Tool]
    public class XUnitDock : MarginContainer
    {
        private SummaryIntLabel summaryTotal;
        private SummaryIntLabel passedTotal;
        private SummaryIntLabel failedTotal;
        private SummaryFloatLabel timeTotal;
        private Tree resultsTree;

        public override void _Ready()
        {
            summaryTotal = (SummaryIntLabel) FindNode("TotalRanLabel");
            passedTotal = (SummaryIntLabel) FindNode("PassedLabel");
            failedTotal = (SummaryIntLabel) FindNode("FailedLabel");
            timeTotal = (SummaryFloatLabel) FindNode("TimeLabel");
            var runAllButton = (Button) FindNode("RunAllTestsButton");
            runAllButton.Connect("pressed", this, nameof(RunAllTests));
            resultsTree = (Tree) FindNode("ResultsTree");
        }

        public void RunAllTests()
        {
            Plugin.Instance.GetEditorInterface().OpenSceneFromPath(Consts.RUNNER_SCENE_PATH);
            var editorNode = Plugin.Instance.GetEditorInterface().GetParent();
            // we have to make the EditorNode::_run method get called, and the only
            // way that i see to do this is to call the _menu_option with 
            // EditorNode::MenuOptions::RUN_PLAY_SCENE... but we also don't have access
            // to that enum value in the script environments.
            editorNode.Call("_menu_option", Consts.GODOT_RUN_PLAY_SCENE);
        }
    }
}
