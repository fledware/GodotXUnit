using System;
using System.Diagnostics;
using System.Text;
using Godot;
using Godot.Collections;
using GodotXUnitApi;
using GodotXUnitApi.Internal;
using StringList = System.Collections.Generic.List<string>;

namespace GodotXUnit
{
    [Tool]
    public class XUnitDock : MarginContainer
    {
        private RichTextLabel resultDetails;
        private Tree resultsTree;
        private MessageWatcher watcher;
        private Dictionary<TreeItem, string> testDetails = new Dictionary<TreeItem, string>();
        private Dictionary<TreeItem, Array<string>> testTargets = new Dictionary<TreeItem, Array<string>>();
        private Button stopButton;
        private Button runAllButton;
        private Button reRunButton;
        private Button runSelectedButton;
        private LineEdit targetClassLabel;
        private LineEdit targetMethodLabel;
        private int runningPid = -1;
        private CheckBox verboseCheck;
        private StringBuilder diagnostics = new StringBuilder();
        private StringBuilder otherErrors = new StringBuilder();
        
        // there are better ways to do this, but to try to limit the amount of user
        // setup required, we'll just do this the hacky way.
        private Label totalRanLabel;
        private int totalRanValue;
        private Label passedLabel;
        private int passedValue;
        private Label failedLabel;
        private int failedValue;
        private Label timeLabel;
        private float timeValue;

        public override void _Ready()
        {
            totalRanLabel = (Label) FindNode("TotalRanLabel");
            passedLabel = (Label) FindNode("PassedLabel");
            failedLabel = (Label) FindNode("FailedLabel");
            timeLabel = (Label) FindNode("TimeLabel");
            ResetLabels();
            
            stopButton = (Button) FindNode("StopButton");
            stopButton.Connect("pressed", this, nameof(StopTests));
            runAllButton = (Button) FindNode("RunAllTestsButton");
            runAllButton.Connect("pressed", this, nameof(RunAllTests));
            reRunButton = (Button) FindNode("ReRunButton");
            reRunButton.Connect("pressed", this, nameof(ReRunTests));
            targetClassLabel = (LineEdit) FindNode("TargetClassLabel");
            targetMethodLabel = (LineEdit) FindNode("TargetMethodLabel");
            runSelectedButton = (Button) FindNode("RunSelectedButton");
            runSelectedButton.Connect("pressed", this, nameof(RunSelected));
            runSelectedButton.Disabled = true;
            resultsTree = (Tree) FindNode("ResultsTree");
            resultsTree.HideRoot = true;
            resultsTree.SelectMode = Tree.SelectModeEnum.Single;
            resultsTree.Connect("cell_selected", this, nameof(OnCellSelected));
            resultDetails = (RichTextLabel) FindNode("ResultDetails");
            verboseCheck = (CheckBox) FindNode("VerboseCheckBox");
            SetProcess(false);
        }

        public void StopTests()
        {
            if (IsRunningTests())
                OS.Kill(runningPid);
            GoToPostState();
        }

        public void RunAllTests()
        {
            RunArgsHelper.ClearRunArgs();
            StartTests();
        }

        public void ReRunTests()
        {
            StartTests();
        }

        public void RunSelected()
        {
            var item = resultsTree.GetSelected();
            // if nothing is selected, just rerun what is there.
            if (item != null)
            {
                if (testTargets.TryGetValue(item, out var value))
                {
                    targetClassLabel.Text = value[0] ?? "";
                    targetMethodLabel.Text = value[1] ?? "";
                }
                else
                {
                    targetClassLabel.Text = item.GetText(0) ?? "";
                    targetMethodLabel.Text = "";
                }
                RunArgsHelper.RunClass(targetClassLabel.Text, targetMethodLabel.Text);
            }
            StartTests();
        }
        
        public void StartTests()
        {
            if (IsRunningTests())
                OS.Kill(runningPid);
            
            runAllButton.Disabled = true;
            reRunButton.Disabled = true;
            runSelectedButton.Disabled = true;
            ResetLabels();
            resultsTree.Clear();
            testTargets.Clear();
            testDetails.Clear();
            resultDetails.Text = "";
            watcher = new MessageWatcher();
            watcher.Start();
            SetProcess(true);

            var runArgs = new StringList();
            runArgs.Add(Consts.RUNNER_SCENE_PATH);
            if (verboseCheck.Pressed)
                runArgs.Add("--verbose");
            runningPid = OS.Execute(OS.GetExecutablePath(), runArgs.ToArray(), false);
        }

        public bool IsRunningTests()
        {
            if (runningPid < 0) return false;
            try
            {
                Process.GetProcessById(runningPid);
                return true;
            }
            catch (Exception)
            {
                GoToPostState();
                return false;
            }
        }

        private void OnCellSelected()
        {
            runSelectedButton.Disabled = resultsTree.GetSelected() == null;
            if (!testDetails.TryGetValue(resultsTree.GetSelected(), out var details))
                details = "Not Found";
            resultDetails.Text = details;
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
                GoToPostState();
            }
        }

        private void GoToPostState()
        {
            watcher?.Stop();
            watcher = null;
            runningPid = -1;
            runAllButton.Disabled = false;
            reRunButton.Disabled = false;
            runSelectedButton.Disabled = resultsTree.GetSelected() != null;
        }

        private void HandleTestStart(GodotXUnitTestStart testStart)
        {
            var testItem = EnsureTreeClassAndMethod(testStart.testCaseClass, testStart.testCaseName);
            testItem.SetText(0, $"{testStart.testCaseName}");
            testItem.SetIcon(0, Consts.IconRunning);
        }

        private void HandleTestResult(GodotXUnitTestResult testResult)
        {
            var testItem = EnsureTreeClassAndMethod(testResult.testCaseClass, testResult.testCaseName);
            switch (testResult.result)
            {
                case "passed":
                    testItem.SetIcon(0, Consts.IconCheck);
                    if (!testDetails.ContainsKey(testItem))
                    {
                        IncPassedLabel();
                        IncTotalLabel();
                        IncTimeLabel(testResult.time);
                    }
                    break;
                case "failed":
                    testItem.SetIcon(0, Consts.IconError);
                    if (!testDetails.ContainsKey(testItem))
                    {
                        IncFailedLabel();
                        IncTotalLabel();
                        IncTimeLabel(testResult.time);
                    }
                    break;
                case "skipped":
                    testItem.SetIcon(0, Consts.IconWarn);
                    break;
                default:
                    testItem.SetText(0, $"{testResult.testCaseName}: unknown result: {testResult.result}");
                    break;
            }
            SetTestResultDetails(testResult, testItem);
        }

        private void SetTestResultDetails(GodotXUnitTestResult testResult, TreeItem item)
        {
            // set the header to include the time it took
            var millis = (int) (testResult.time * 1000f);
            item.SetText(0, $"{testResult.testCaseName} ({millis} ms)");
            
            // create the test result details
            var details = new StringBuilder();
            details.AppendLine(testResult.FullName);
            details.AppendLine(testResult.result);
            details.AppendLine();
            if (!string.IsNullOrEmpty(testResult.exceptionType))
            {
                details.AppendLine(testResult.exceptionMessage);
                details.AppendLine(testResult.exceptionType);
                details.AppendLine(testResult.exceptionStackTrace);
                details.AppendLine();
            }
            details.AppendLine(string.IsNullOrEmpty(testResult.output) ? "No Output" : testResult.output);
            testDetails[item] = details.ToString();
            
            // add the target so the run selected button can query what to run
            var target = new Array<string>();
            target.Add(testResult.testCaseClass);
            target.Add(testResult.testCaseName);
            testTargets[item] = target;
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

        // label work below...
        // dont look at me, i'm hideous
        private void ResetLabels()
        {
            totalRanValue = 0;
            totalRanLabel.Text = $"TotalRan: {totalRanValue}";
            passedValue = 0;
            passedLabel.Text = $"Passed: {passedValue}";
            failedValue = 0;
            failedLabel.Text = $"Failed: {failedValue}";
            timeValue = 0;
            timeLabel.Text = $"Time: {timeValue} ms";
        }

        private void IncPassedLabel()
        {
            // gross
            passedValue++;
            passedLabel.Text = $"Passed: {passedValue}";
        }

        private void IncFailedLabel()
        {
            // naughty
            failedValue++;
            failedLabel.Text = $"Failed: {failedValue}";
        }

        private void IncTotalLabel()
        {
            // terrible
            totalRanValue++;
            totalRanLabel.Text = $"TotalRan: {totalRanValue}";
        }

        private void IncTimeLabel(float time)
        {
            // why?
            timeValue += time;
            timeLabel.Text = $"Time: {(int) (timeValue * 1000)} ms";
        }
    }
}
