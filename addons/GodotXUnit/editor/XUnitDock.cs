using System.Globalization;
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
        private MessageWatcher watcher;

        public override void _Ready()
        {
            summaryTotal = (SummaryIntLabel) FindNode("TotalRanLabel");
            summaryTotal.TextValue = 0;
            passedTotal = (SummaryIntLabel) FindNode("PassedLabel");
            passedTotal.TextValue = 0;
            failedTotal = (SummaryIntLabel) FindNode("FailedLabel");
            failedTotal.TextValue = 0;
            timeTotal = (SummaryFloatLabel) FindNode("TimeLabel");
            timeTotal.TextValue = 0;
            var runAllButton = (Button) FindNode("RunAllTestsButton");
            runAllButton.Connect("pressed", this, nameof(RunAllTests));
            resultsTree = (Tree) FindNode("ResultsTree");
            resultsTree.HideRoot = true;
            SetProcess(false);
        }

        public void RunAllTests()
        {
            summaryTotal.TextValue = 0;
            passedTotal.TextValue = 0;
            failedTotal.TextValue = 0;
            timeTotal.TextValue = 0;
            resultsTree.Clear();
            watcher = new MessageWatcher();
            watcher.Start();
            SetProcess(true);
            GD.Print("RunAllTests [0]");
            Plugin.Instance.GetEditorInterface().OpenSceneFromPath(Consts.RUNNER_SCENE_PATH);
            var editorNode = Plugin.Instance.GetEditorInterface().GetParent();
            // we have to make the EditorNode::_run method get called, and the only
            // way that i see to do this is to call the _menu_option with 
            // EditorNode::MenuOptions::RUN_PLAY_SCENE... but we also don't have access
            // to that enum value in the script environments.
            editorNode.Call("_menu_option", Consts.GODOT_RUN_PLAY_SCENE);
        }

        public override void _Process(float delta)
        {
            while (watcher != null)
            {
                var message = watcher.Poll();
                if (message == null) break;

                switch (message)
                {
                    case GodotXUnitTestStart testStart:
                        HandleTestStart(testStart);
                        break;
                    case GodotXUnitTestResult testResult:
                        HandleTestResult(testResult);
                        break;
                    case GodotXUnitSummary testSummary:
                        HandleTestSummary(testSummary);
                        break;
                    default:
                        GD.PrintErr($"unable to handle message type: {message.GetType()}");
                        break;
                }
            }
            if (!IsProcessing())
            {
                watcher.Stop();
                watcher = null;
            }
        }

        private void HandleTestStart(GodotXUnitTestStart testStart)
        {
            var testItem = EnsureTreeClassAndMethod(testStart.testCaseClass, testStart.testCaseName);
            testItem.SetText(0, $"{testStart.testCaseName}: running");
        }

        private void HandleTestResult(GodotXUnitTestResult testResult)
        {
            var testItem = EnsureTreeClassAndMethod(testResult.testCaseClass, testResult.testCaseName);
            testItem.SetText(0, $"{testResult.testCaseName}: {testResult.result} " +
                                $"({testResult.time.ToString(CultureInfo.CurrentCulture)})");
        }

        private void HandleTestSummary(GodotXUnitSummary testSummary)
        {
            foreach (var failed in testSummary.failed)
                HandleTestResult(failed);
            foreach (var passed in testSummary.passed)
                HandleTestResult(passed);
            foreach (var skipped in testSummary.skipped)
                HandleTestResult(skipped);
            SetProcess(false);
        }

        private TreeItem EnsureTreeClassAndMethod(string testClass, string testCaseName)
        {
            var testClassItem = EnsureTreeClass(testClass);
            return FindTreeChildOrCreate(testClassItem, testCaseName);
        }

        private TreeItem EnsureTreeClass(string testClass)
        {
            var root = resultsTree.GetRoot() ?? resultsTree.CreateItem();
            return FindTreeChildOrCreate(root, testClass);
        }

        private TreeItem FindTreeChildOrCreate(TreeItem parent, string name)
        {
            var child = parent.GetChildren();
            while (child != null)
            {
                if (child.GetText(0).StartsWith(name)) return child;
                child = child.GetNext();
            }
            var newClassItem = resultsTree.CreateItem(parent);
            newClassItem.SetText(0, name);
            return newClassItem;
        }
    }
}