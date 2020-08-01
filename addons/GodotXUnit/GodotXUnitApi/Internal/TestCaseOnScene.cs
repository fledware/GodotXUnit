using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GodotXUnitApi.Internal
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
            // make sure we are in the process event to switch scenes
            await GDU.OnProcessAwaiter;
            if (GDU.Instance.GetTree().ChangeScene(sceneName) != Error.Ok)
            {
                var message = $"could not load scene: {sceneName}";
                var test = new XunitTest(this, DisplayName);
                messageBus.QueueMessage(new Xunit.Sdk.TestFailed(test, 0, message, new Exception(message)));
                return new RunSummary
                {
                    Total = 1,
                    Failed = 1
                };
            }
            
            // the scene should be loaded within two frames
            await GDU.OnIdleFrameAwaiter;
            await GDU.OnIdleFrameAwaiter;
            await GDU.OnProcessAwaiter;
            var result = await base.RunAsync(
                diagnosticMessageSink,
                messageBus,
                constructorArguments,
                aggregator,
                cancellationTokenSource);
            
            // change scenes again and wait for godot to catch up
            GDU.Instance.GetTree().ChangeScene(Consts.EMPTY_SCENE_PATH);
            await GDU.OnIdleFrameAwaiter;
            await GDU.OnIdleFrameAwaiter;
            await GDU.OnProcessAwaiter;
            return result;
        }
    }
}