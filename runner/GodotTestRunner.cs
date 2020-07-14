using System;
using System.Reflection;
using Godot;
using Xunit.Runners;
using Xunit.Sdk;

namespace GodotCSUnitTest
{
	public class GodotTestRunner : GodotEvents
	{
		private AssemblyRunner runner;
		
		public override void _Ready()
		{
			Instance = this;
			
			runner = AssemblyRunner.WithoutAppDomain(Assembly.GetCallingAssembly().Location);
			runner.OnDiagnosticMessage = message =>
			{
				GD.PrintErr($"OnDiagnosticMessage: {message.Message}");
			};
			runner.OnDiscoveryComplete = message =>
			{
				GD.Print($"OnDiscoveryComplete: discovered => {message.TestCasesDiscovered}, run => {message.TestCasesToRun}");
			};
			runner.OnErrorMessage = message =>
			{
				GD.PrintErr($"OnErrorMessage ({message.MesssageType}): {message.ExceptionType}: {message.ExceptionMessage}");
			};
			runner.OnExecutionComplete = message =>
			{
				var run = message.TotalTests - message.TestsSkipped;
				GD.Print($"OnExecutionComplete ({message.ExecutionTime}): total({run}), failed({message.TestsFailed})");
				GetTree().Quit();
			};
			runner.OnTestStarting = message =>
			{
				GD.Print($"OnTestStarting: {message.TestDisplayName}");
			};
			runner.OnTestFailed = message =>
			{
				GD.Print($"OnTestFailed: {message.TestDisplayName}");
			};
			runner.OnTestPassed = message =>
			{
				GD.Print($"OnTestPassed: {message.TestDisplayName}");
			};
			runner.OnTestSkipped = message =>
			{
				GD.Print($"OnTestSkipped: {message.TestDisplayName}");
			};
			runner.OnTestFinished = message =>
			{
				GD.Print($"OnTestFinished: {message.TestDisplayName}");
			};
			runner.OnTestOutput = message =>
			{
				GD.Print($"OnTestOutput: {message.TestDisplayName} => {message.Output}");
			};
			runner.Start(null, null, null, null, null, false, null, null);
		}

		public override void _Process(float delta)
		{
			EmitSignal(nameof(OnProcess));
		}

		public override void _PhysicsProcess(float delta)
		{
			EmitSignal(nameof(OnPhysicsProcess));
		}

		public override void _ExitTree()
		{
			Instance = null;
			runner?.Dispose();
			runner = null;
		}
	}

}
