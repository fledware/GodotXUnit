using System;
using System.Collections.Concurrent;
using System.Reflection;
using Godot;
using Newtonsoft.Json;
using Xunit.Runners;

namespace GodotXUnitApi.Internal
{
    public abstract class GodotXUnitRunner : Node2D
    {
        protected abstract Assembly GetAssemblyToTest();
        
        private ConcurrentQueue<Action<Node2D>> drawRequests = new ConcurrentQueue<Action<Node2D>>();

        [Signal]
        public delegate void OnProcess();

        [Signal]
        public delegate void OnPhysicsProcess();

        [Signal]
        public delegate void OnDrawRequestDone();

        public void RequestDraw(Action<Node2D> request)
        {
            drawRequests.Enqueue(request);
        }

        private AssemblyRunner runner;

        private GodotXUnitSummary summary;

        private MessageSender messages;

        public override void _Ready()
        {
            GDU.Instance = this;
            GD.Print($"running tests in tree at: {GetPath()}");

            WorkFiles.CleanWorkDir();
            messages = new MessageSender();
            summary = new GodotXUnitSummary();
            runner = AssemblyRunner.WithoutAppDomain(GetAssemblyToTest().Location);
            runner.OnDiagnosticMessage = message =>
            {
                GD.PrintErr($"OnDiagnosticMessage: {message.Message}");
                summary.diagnostics.Add(message.Message);
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
                GD.PrintErr($"OnErrorMessage ({message.MesssageType}) {message.ExceptionType}: " +
                            $"{message.ExceptionMessage}\n{message.ExceptionStackTrace}");
                summary.errors.Add(new GodotXUnitOtherError
                {
                    exceptionType = message.ExceptionType,
                    exceptionMessage = message.ExceptionMessage,
                    exceptionStackTrace = message.ExceptionStackTrace
                });
            };
            runner.OnTestStarting = message =>
            {
                messages.SendMessage(new GodotXUnitTestStart()
                {
                    testCaseClass = message.TypeName,
                    testCaseName = message.MethodName
                });
            };
            runner.OnTestFailed = message =>
            {
                messages.SendMessage(summary.AddFailed(message));
                GD.Print($"  > OnTestFailed: {message.TestDisplayName} in {message.ExecutionTime}");
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
                GetTree().Quit();
            };
            
            var runArgs = RunArgsHelper.Read();
            if (!string.IsNullOrEmpty(runArgs.methodToRun))
            {
                runner.TestCaseFilter = check =>
                {
                    Console.WriteLine($"{runArgs.methodToRun} == {check.TestMethod.Method.Name}");
                    return runArgs.methodToRun.Equals(check.TestMethod.Method.Name);
                };
            }
            runner.Start(runArgs.classToRun, null, null, null, null, false, null, null);
        }

        public override void _ExitTree()
        {
            GDU.Instance = null;
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
            Update();
        }

        public override void _PhysicsProcess(float delta)
        {
            EmitSignal(nameof(OnPhysicsProcess));
        }

        public override void _Draw()
        {
            while (drawRequests.TryDequeue(out var request))
                request(this);
            EmitSignal(nameof(OnDrawRequestDone));
        }
    }
}