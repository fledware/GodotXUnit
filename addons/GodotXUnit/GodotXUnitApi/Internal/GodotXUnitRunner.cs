using System.Reflection;
using Godot;
using Newtonsoft.Json;
using Xunit.Runners;

namespace GodotXUnitApi.Internal
{
    public abstract class GodotXUnitRunner : GDU
    {
        protected abstract Assembly GetAssemblyToTest();

        private AssemblyRunner runner;

        private GodotXUnitSummary summary;

        private MessageSender messages;

        public override void _Ready()
        {
            Instance = this;
            GD.Print($"running tests in tree at: {GetPath()}");

            WorkFiles.CleanWorkDir();
            messages = new MessageSender();
            summary = new GodotXUnitSummary();
            runner = AssemblyRunner.WithoutAppDomain(GetAssemblyToTest().Location);
            runner.OnDiagnosticMessage = message => { GD.PrintErr($"OnDiagnosticMessage: {message.Message}"); };
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
                messages.SendMessage(new GodotXUnitTestStart()
                {
                    testCaseClass = message.TypeName,
                    testCaseName = message.MethodName
                });
                // GD.Print($"OnTestStarting: {message.TestDisplayName}");
            };
            runner.OnTestFailed = message =>
            {
                messages.SendMessage(summary.AddFailed(message));
                GD.Print($"  > OnTestFailed: {message.TestDisplayName} in {message.ExecutionTime}");
                // GD.PrintErr(message.ExceptionType);
                // GD.PrintErr(message.ExceptionMessage);
                // GD.PrintErr(message.ExceptionStackTrace);
            };
            runner.OnTestPassed = message =>
            {
                messages.SendMessage(summary.AddPassed(message));
                GD.Print($"  > OnTestPassed: {message.TestDisplayName} in {message.ExecutionTime}");
            };
            runner.OnTestSkipped = message =>
            {
                messages.SendMessage(summary.AddSkipped(message));
                GD.Print($"  > OnTestSkipped: {message.TestDisplayName}");
            };
            runner.OnExecutionComplete = message =>
            {
                messages.SendMessage(summary);
                WriteSummary(summary);
                GD.Print($"tests completed ({message.ExecutionTime}): {summary.completed}");
                // GD.Print($"   skipped: {summary.skipped.Count}");
                // GD.Print($"   passed: {summary.passed.Count}");
                // GD.Print($"   failed: {summary.failed.Count}");
                GetTree().Quit();
            };
            
            var runArgs = RunArgsHelper.Read();
            var classToRun = string.IsNullOrEmpty(runArgs.classToRun) ? null : runArgs.classToRun;
            runner.Start(classToRun, null, null, null, null, false, null, null);
        }

        public override void _ExitTree()
        {
            Instance = null;
            runner?.Dispose();
            runner = null;
        }

        private void WriteSummary(GodotXUnitSummary testSummary)
        {
            var location = ProjectSettings.HasSetting(Consts.SETTING_RESULTS_SUMMARY)
                ? ProjectSettings.GetSetting(Consts.SETTING_RESULTS_SUMMARY).ToString()
                : Consts.SETTING_RESULTS_SUMMARY_DEF;
            var file = new File();
            file.Open(location, File.ModeFlags.Write);
            file.StoreString(JsonConvert.SerializeObject(testSummary, Formatting.Indented, WorkFiles.jsonSettings));
            file.Close();
        }

        public override void _Process(float delta)
        {
            EmitSignal(nameof(OnProcess));
        }

        public override void _PhysicsProcess(float delta)
        {
            EmitSignal(nameof(OnPhysicsProcess));
        }
    }
}