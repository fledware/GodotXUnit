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
        protected abstract Assembly GetTargetAssembly(GodotXUnitSummary summary);

        protected abstract string GetTargetClass(GodotXUnitSummary summary);

        protected abstract string GetTargetMethod(GodotXUnitSummary summary);

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
            summary = new GodotXUnitSummary();
            messages = new MessageSender();
            CreateRunner();
            if (runner == null)
            {
                messages.SendMessage(summary);
                WriteSummary(summary);
                GetTree().Quit(1);
                return;
            }
            runner.OnDiagnosticMessage = message =>
            {
                GD.PrintErr($"OnDiagnosticMessage: {message.Message}");
                summary.AddDiagnostic(message.Message);
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
                summary.diagnostics.Add(new GodotXUnitOtherDiagnostic
                {
                    message = message.ExceptionMessage,
                    exceptionType = message.ExceptionType,
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

            var targetMethod = GetTargetMethod(summary);
            if (!string.IsNullOrEmpty(targetMethod))
            {
                GD.Print($"targeting method for discovery: {targetMethod}");
                runner.TestCaseFilter = test => targetMethod.Equals(test.TestMethod.Method.Name);
            }
            
            // if its an empty string, then we need to set it to null because the runner only checks for null
            var targetClass = GetTargetClass(summary);
            if (string.IsNullOrEmpty(targetClass))
                targetClass = null;
            else
            {
                GD.Print($"targeting class for discovery: {targetClass}");
            }
            runner.Start(targetClass, null, null, null, null, false, null, null);
        }

        private void CreateRunner()
        {
            try
            {
                var check = GetTargetAssembly(summary);
                if (check == null)
                {
                    GD.PrintErr("no assembly returned for tests");
                    summary.AddDiagnostic(new Exception("no assembly returned for tests"));
                    return;
                }
                runner = AssemblyRunner.WithoutAppDomain(check.Location);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"error while attempting to get test assembly: {ex}");
                summary.AddDiagnostic("error while attempting to get test assembly");
                summary.AddDiagnostic(ex);
            }
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