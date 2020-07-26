using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GodotXUnitApi.Internal
{
    public class TestCaseAfterSignal : XunitTestCase
    {
#pragma warning disable 618
        public TestCaseAfterSignal() {}
#pragma warning restore 618

        private Func<SignalAwaiter> CreateSignalAwaiter;
        
        public TestCaseAfterSignal(
            Func<SignalAwaiter> CreateSignalAwaiter,
            IMessageSink diagnosticMessageSink,
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            object[] testMethodArguments = null)
            : base(diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(),
                discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, testMethodArguments)
        {
            this.CreateSignalAwaiter = CreateSignalAwaiter;
        }

        public override async Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            await CreateSignalAwaiter();
            return await base.RunAsync(
                diagnosticMessageSink,
                messageBus,
                constructorArguments,
                aggregator,
                cancellationTokenSource);
        }
    }
}