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
        
		private AssemblyRunner runner;
		
		private GodotXUnitSummary summary;
		
		public override void _Ready()
		{
			Instance = this;
			GD.Print($"running tests in tree at: {GetPath()}");
			
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
				summary.AddFailed(message);
				GD.Print($"  > OnTestFailed: {message.TestDisplayName} in {message.ExecutionTime}");
				GD.PrintErr(message.ExceptionType);
				GD.PrintErr(message.ExceptionMessage);
				GD.PrintErr(message.ExceptionStackTrace);
			};
			runner.OnTestPassed = message =>
			{
				summary.AddPassed(message);
				GD.Print($"  > OnTestPassed: {message.TestDisplayName} in {message.ExecutionTime}");
			};
			runner.OnTestSkipped = message =>
			{
				summary.AddSkipped(message);
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

		private void WriteResultsFile()
		{
			var summaryJson = JsonConvert.SerializeObject(summary, Formatting.Indented);
			var file = new Godot.File();
			if (file.Open("res://TestResults.json", File.ModeFlags.Write) == Error.Ok)
			{
				file.StoreLine(summaryJson);
			}
			else
			{
				GD.PrintErr("count not open file to write results");
				GD.Print(summaryJson);
			}
			file.Close();
		}
    }
}