using System;
using System.Diagnostics;
using System.Text;
using Godot;
using Godot.Collections;
using GodotXUnitApi;
using GodotXUnitApi.Internal;
using StringList = System.Collections.Generic.List<string>;
using GodotArray = Godot.Collections.Array;

namespace GodotXUnit
{
    [Tool]
    public partial class XUnitDock : MarginContainer
    {
        private RichTextLabel resultDetails;
        private RichTextLabel resultDiagnostics;
        private Tree resultsTree;
        private MessageWatcher watcher;
        private Dictionary<TreeItem, string> testDetails = new Dictionary<TreeItem, string>();
        private Dictionary<TreeItem, Array<string>> testTargets = new Dictionary<TreeItem, Array<string>>();
        private Button stopButton;
        private Button runAllButton;
        private Button reRunButton;
        private Button runSelectedButton;
        private LineEdit targetAssemblyLabel;
        private OptionButton targetAssemblyOption;
        private LineEdit targetClassLabel;
        private LineEdit targetMethodLabel;
        private int runningPid = -1;
        private CheckBox verboseCheck;
        private TabContainer runTabContainer;

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
            totalRanLabel = (Label)FindChild("TotalRanLabel");
            passedLabel = (Label)FindChild("PassedLabel");
            failedLabel = (Label)FindChild("FailedLabel");
            timeLabel = (Label)FindChild("TimeLabel");
            ResetLabels();

            stopButton = (Button)FindChild("StopButton");
            stopButton.Connect("pressed", new Callable(this, nameof(StopTests)));
            runAllButton = (Button)FindChild("RunAllTestsButton");
            runAllButton.Connect("pressed", new Callable(this, nameof(RunAllTests)));
            reRunButton = (Button)FindChild("ReRunButton");
            reRunButton.Connect("pressed", new Callable(this, nameof(ReRunTests)));
            targetClassLabel = (LineEdit)FindChild("TargetClassLabel");
            targetMethodLabel = (LineEdit)FindChild("TargetMethodLabel");
            runSelectedButton = (Button)FindChild("RunSelectedButton");
            runSelectedButton.Connect("pressed", new Callable(this, nameof(RunSelected)));
            runSelectedButton.Disabled = true;
            targetAssemblyOption = (OptionButton)FindChild("TargetAssemblyOption");
            targetAssemblyOption.Connect("pressed", new Callable(this, nameof(TargetAssemblyOptionPressed)));
            targetAssemblyOption.Connect("item_selected", new Callable(this, nameof(TargetAssemblyOptionSelected)));
            targetAssemblyLabel = (LineEdit)FindChild("TargetAssemblyLabel");
            targetAssemblyLabel.Text = ProjectSettings.HasSetting(Consts.SETTING_TARGET_ASSEMBLY_CUSTOM)
                ? ProjectSettings.GetSetting(Consts.SETTING_TARGET_ASSEMBLY_CUSTOM).ToString()
                : "";
            targetAssemblyLabel.Connect("text_changed", new Callable(this, nameof(TargetAssemblyLabelChanged)));
            TargetAssemblyOptionPressed();
            resultsTree = (Tree)FindChild("ResultsTree");
            resultsTree.HideRoot = true;
            resultsTree.SelectMode = Tree.SelectModeEnum.Single;
            resultsTree.Connect("cell_selected", new Callable(this, nameof(OnCellSelected)));
            resultDetails = (RichTextLabel)FindChild("ResultDetails");
            resultDiagnostics = (RichTextLabel)FindChild("Diagnostics");
            verboseCheck = (CheckBox)FindChild("VerboseCheckBox");
            runTabContainer = (TabContainer)FindChild("RunTabContainer");
            runTabContainer.CurrentTab = 0;
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
            ProjectSettings.SetSetting(Consts.SETTING_TARGET_CLASS, "");
            ProjectSettings.SetSetting(Consts.SETTING_TARGET_METHOD, "");
            ProjectSettings.Save();
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
                ProjectSettings.SetSetting(Consts.SETTING_TARGET_CLASS, targetClassLabel.Text);
                ProjectSettings.SetSetting(Consts.SETTING_TARGET_METHOD, targetMethodLabel.Text);
                ProjectSettings.Save();
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
            resultDiagnostics.Text = "";
            resultDetails.Text = "";
            watcher = new MessageWatcher();

            // if things dont clean up correctly, the old messages can still
            // be on the file system. this will cause the XUnitDock process to
            // see stale messages and potentially stop picking up new messages.
            WorkFiles.CleanWorkDir();

            var runArgs = new StringList();
            runArgs.Add(Consts.RUNNER_SCENE_PATH);
            if (verboseCheck.ButtonPressed)
                runArgs.Add("--verbose");
            runningPid = OS.Execute(OS.GetExecutablePath(), runArgs.ToArray(), null, false);

            SetProcess(true);
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

        private void TargetAssemblyOptionPressed()
        {
            targetAssemblyOption.Clear();
            var projectList = ProjectListing.GetProjectList();
            var projectSelected = ProjectSettings.HasSetting(Consts.SETTING_TARGET_ASSEMBLY)
                ? ProjectSettings.GetSetting(Consts.SETTING_TARGET_ASSEMBLY).ToString()
                : "";
            var projectSelectedIndex = 0;
            for (int i = 0; i < projectList.Count; i++)
            {
                var projectName = projectList[i];
                targetAssemblyOption.AddItem(projectName, i);
                if (projectName.Equals(projectSelected))
                    projectSelectedIndex = i;
            }
            targetAssemblyOption.AddItem("Custom Location ", 1000);
            if (projectSelected.Equals(Consts.SETTING_TARGET_ASSEMBLY_CUSTOM_FLAG))
                projectSelectedIndex = projectList.Count;
            targetAssemblyOption.Selected = projectSelectedIndex;
        }

        private void TargetAssemblyOptionSelected(int index)
        {
            var projectId = targetAssemblyOption.GetItemId(index);
            switch (projectId)
            {
                case 1000:
                    ProjectSettings.SetSetting(Consts.SETTING_TARGET_ASSEMBLY,
                                               Consts.SETTING_TARGET_ASSEMBLY_CUSTOM_FLAG);
                    break;
                default:
                    var projectName = targetAssemblyOption.GetItemText(index);
                    ProjectSettings.SetSetting(Consts.SETTING_TARGET_ASSEMBLY, projectName);
                    break;
            }
            ProjectSettings.Save();
        }

        private void TargetAssemblyLabelChanged(string new_text)
        {
            ProjectSettings.SetSetting(Consts.SETTING_TARGET_ASSEMBLY_CUSTOM, new_text);
            ProjectSettings.Save();
        }

        private void OnCellSelected()
        {
            runSelectedButton.Disabled = resultsTree.GetSelected() == null;
            if (!testDetails.TryGetValue(resultsTree.GetSelected(), out var details))
                details = "Not Found";
            resultDetails.Text = details;
            runTabContainer.CurrentTab = 0;
        }

        public override void _Process(double delta)
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
            if (watcher != null)
            {
                while (true)
                {
                    var missed = watcher.Poll();
                    if (missed == null) break;
                    GD.PrintErr($"missed message: {missed.GetType()}");
                }
            }
            watcher = null;
            runningPid = -1;
            runAllButton.Disabled = false;
            reRunButton.Disabled = false;
            runSelectedButton.Disabled = resultsTree.GetSelected() != null;
        }

        private void HandleTestStart(GodotXUnitTestStart testStart)
        {
            var testItem = EnsureTreeClassAndMethod(testStart.testCaseClass, testStart.testCaseName);
            if (testItem.GetIcon(0) == null)
            {
                testItem.SetIcon(0, Consts.IconRunning);
            }
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
            var millis = (int)(testResult.time * 1000f);
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

            if (testSummary.diagnostics.Count > 0)
            {
                var diagnostics = new StringBuilder();
                foreach (var diagnostic in testSummary.diagnostics)
                {
                    if (diagnostic.exceptionType != null)
                    {
                        diagnostics.Append(diagnostic.exceptionType).Append(": ");
                    }
                    diagnostics.AppendLine(diagnostic.message);
                    if (diagnostic.exceptionStackTrace != null)
                    {
                        diagnostics.AppendLine(diagnostic.exceptionStackTrace);
                    }
                    diagnostics.AppendLine();
                    diagnostics.AppendLine();
                }
                resultDiagnostics.Text = diagnostics.ToString();
            }
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
            foreach (var child in parent.GetChildren())
            {
                var text = child.GetMeta("for");
                if (text.AsString().Equals(name)) return child;
            }
            var newClassItem = resultsTree.CreateItem(parent);
            newClassItem.SetMeta("for", name);
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
            timeLabel.Text = $"Time: {(int)(timeValue * 1000)} ms";
        }
    }
}
