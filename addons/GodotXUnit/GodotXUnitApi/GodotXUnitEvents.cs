using System;
using System.Reflection;
using Godot;
using Xunit.Runners;

namespace GodotXUnitApi
{
    public abstract class GodotXUnitEvents : Node
    {
        private static GodotXUnitEvents _instance;

        public static GodotXUnitEvents Instance
        {
            get => _instance ?? throw new Exception("GodotXUnitEvents not set");
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

        private AssemblyRunner runner;

        private GodotXUnitSummary summary;
        
        private MessageSender messages;

        public override void _Ready()
        {
            Instance = this;
            GD.Print($"running tests in tree at: {GetPath()}");

            messages = new MessageSender();
            messages.EnsureMessageDirectory();
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
                messages.SendMessage(new GodotXUnitTestResult
                {
                    testCaseClass = message.TypeName,
                    testCaseName = message.MethodName,
                    result = "starting"
                });
                // GD.Print($"OnTestStarting: {message.TestDisplayName}");
            };
            runner.OnTestFailed = message =>
            {
                messages.SendMessage(summary.AddFailed(message));
                // GD.Print($"  > OnTestFailed: {message.TestDisplayName} in {message.ExecutionTime}");
                // GD.PrintErr(message.ExceptionType);
                // GD.PrintErr(message.ExceptionMessage);
                // GD.PrintErr(message.ExceptionStackTrace);
            };
            runner.OnTestPassed = message =>
            {
                messages.SendMessage(summary.AddPassed(message));
                // GD.Print($"  > OnTestPassed: {message.TestDisplayName} in {message.ExecutionTime}");
            };
            runner.OnTestSkipped = message =>
            {
                messages.SendMessage(summary.AddSkipped(message));
                // GD.Print($"  > OnTestSkipped: {message.TestDisplayName}");
            };
            runner.OnExecutionComplete = message =>
            {
                messages.SendMessage(summary);
                // GD.Print($"tests completed ({message.ExecutionTime}): {summary.completed}");
                // GD.Print($"   skipped: {summary.skipped.Count}");
                // GD.Print($"   passed: {summary.passed.Count}");
                // GD.Print($"   failed: {summary.failed.Count}");
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
    }
}