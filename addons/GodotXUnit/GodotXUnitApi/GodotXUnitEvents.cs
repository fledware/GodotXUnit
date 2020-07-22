using System;
using System.Reflection;
using Godot;
using Newtonsoft.Json;
using Xunit.Runners;

namespace GodotXUnitApi
{
    public abstract class GodotXUnitEvents : Node
    {
        private static GodotXUnitEvents _instance;

        public static GodotXUnitEvents Instance
        {
            get => _instance ?? throw new Exception("GodotEvents not set");
            set => _instance = value;
        }

        [Signal]
        public delegate void OnProcess();

        public static SignalAwaiter OnProcessAwaiter =>
            Instance.ToSignal(Instance, nameof(OnProcess));

        [Signal]
        public delegate void OnPhysicsProcess();

        public static SignalAwaiter OnPhysicsProcessAwaiter =>
            Instance.ToSignal(Instance, nameof(OnPhysicsProcess));

        public static SignalAwaiter OnIdleFrameAwaiter =>
            Instance.ToSignal(Instance.GetTree(), "idle_frame");

        protected abstract Assembly GetAssemblyToTest();

        private int testCount = 0;

        private AssemblyRunner runner;

        private GodotXUnitSummary summary;

        public override void _Ready()
        {
            Instance = this;
            GD.Print($"running tests in tree at: {GetPath()}");

            EnsureWorkDirectory();
            summary = new GodotXUnitSummary();
            runner = AssemblyRunner.WithoutAppDomain(GetAssemblyToTest().Location);
            runner.OnDiagnosticMessage = message =>
            {
                GD.PrintErr($"OnDiagnosticMessage: {message.Message}");
            };
            runner.OnDiscoveryComplete = message =>
            {
                summary.testsDiscovered = message.TestCasesDiscovered;
                summary.testsExpectedToRun = message.TestCasesToRun;
                GD.Print($"discovery finished: found {message.TestCasesDiscovered}," +
                         $" running {message.TestCasesToRun}");
            };
            runner.OnErrorMessage = message =>
            {
                GD.PrintErr($"OnErrorMessage ({message.MesssageType}): {message.ExceptionType}");
                GD.PrintErr(message.ExceptionMessage);
                GD.PrintErr("this is a bug in GodotXUnit");
            };
            runner.OnTestStarting = message =>
            {
                GD.Print($"OnTestStarting: {message.TestDisplayName}");
            };
            runner.OnTestFailed = message =>
            {
                SaveAndIncResult(summary.AddFailed(message));
                GD.Print($"  > OnTestFailed: {message.TestDisplayName} in {message.ExecutionTime}");
                GD.PrintErr(message.ExceptionType);
                GD.PrintErr(message.ExceptionMessage);
                GD.PrintErr(message.ExceptionStackTrace);
            };
            runner.OnTestPassed = message =>
            {
                SaveAndIncResult(summary.AddPassed(message));
                GD.Print($"  > OnTestPassed: {message.TestDisplayName} in {message.ExecutionTime}");
            };
            runner.OnTestSkipped = message =>
            {
                SaveAndIncResult(summary.AddSkipped(message));
                GD.Print($"  > OnTestSkipped: {message.TestDisplayName}");
            };
            runner.OnExecutionComplete = message =>
            {
                GD.Print($"tests completed ({message.ExecutionTime}): {summary.completed}");
                GD.Print($"   skipped: {summary.skipped.Count}");
                GD.Print($"   passed: {summary.passed.Count}");
                GD.Print($"   failed: {summary.failed.Count}");
                WriteResultsFile();
                GetTree().Quit();
            };
            runner.Start(null, null, null, null, null, false, null, null);
        }

        public override void _ExitTree()
        {
            Instance = null;
            runner?.Dispose();
            runner = null;
        }

        private void EnsureWorkDirectory()
        {
            var directory = new Godot.Directory();
            directory.MakeDirRecursive(Consts.RUN_WORK_DIR);
            if (directory.Open(Consts.RUN_WORK_DIR) != Error.Ok)
                return;
            directory.ListDirBegin();
            while (true)
            {
                var next = directory.GetNext();
                if (next.EndsWith(".json"))
                {
                    directory.Remove(next);
                }
                if (string.IsNullOrEmpty(next))
                    break;
            }
        }

        private void SaveAndIncResult(GodotXUnitTestResult result)
        {
            testCount++;
            var resultJson = JsonConvert.SerializeObject(result, Formatting.Indented);
            SaveFile($"{Consts.RUN_WORK_DIR}/{testCount}.json", resultJson);
        }

        private void WriteResultsFile()
        {
            var summaryLocation = ProjectSettings.GetSetting(Consts.SETTING_RESULTS_SUMMARY)?.ToString();
            if (string.IsNullOrEmpty(summaryLocation))
            {
                summaryLocation = Consts.SETTING_RESULTS_SUMMARY_DEF;
                GD.PrintErr($"unable to find setting for GodotXUnit/results_summary, defaulting to {summaryLocation}");
            }
            var summaryJson = JsonConvert.SerializeObject(summary, Formatting.Indented);
            SaveFile(summaryLocation, summaryJson);
            SaveFile($"{Consts.RUN_WORK_DIR}/finished.json", summaryJson);
        }

        private void SaveFile(string filename, string contents)
        {
            var file = new Godot.File();
            var openResult = file.Open(filename, File.ModeFlags.Write);
            if (openResult == Error.Ok)
            {
                file.StoreLine(contents);
            }
            else
            {
                GD.PrintErr($"unable to open file: {openResult}");
            }
            file.Close();
        }
    }
}