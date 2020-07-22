using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GodotXUnitApi
{
    public class TestCaseOnScene : XunitTestCase
    {
#pragma warning disable 618
        public TestCaseOnScene() {}
#pragma warning restore 618

        private string sceneName;
        
        public TestCaseOnScene(
            string sceneName,
            IMessageSink diagnosticMessageSink,
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            object[] testMethodArguments = null)
            : base(diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(),
                discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, testMethodArguments)
        {
            this.sceneName = sceneName;
        }

        public override async Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            if (GodotXUnitEvents.Instance.GetTree().ChangeSceneTo(GD.Load<PackedScene>(sceneName)) != Error.Ok)
            {
                GD.PrintErr($"could not load scene: {sceneName}");
                return new RunSummary
                {
                    Total = 1,
                    Failed = 1
                };
            }
            await GodotXUnitEvents.OnIdleFrameAwaiter;
            await GodotXUnitEvents.OnIdleFrameAwaiter;
            await GodotXUnitEvents.OnProcessAwaiter;
            var result = await base.RunAsync(
                diagnosticMessageSink,
                messageBus,
                constructorArguments,
                aggregator,
                cancellationTokenSource);
            GodotXUnitEvents.Instance.GetTree().ChangeScene("res://addons/GodotXUnit/EmptyScene.tscn");
            await GodotXUnitEvents.OnIdleFrameAwaiter;
            await GodotXUnitEvents.OnIdleFrameAwaiter;
            await GodotXUnitEvents.OnProcessAwaiter;
            return result;
        }
    }
}