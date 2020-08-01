using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GodotXUnitApi.Internal
{
    public class TestCaseOnTree : XunitTestCase
    {
#pragma warning disable 618
        public TestCaseOnTree() {}
#pragma warning restore 618
        
        public TestCaseOnTree(
            IMessageSink diagnosticMessageSink,
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            object[] testMethodArguments = null)
            : base(diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(),
                discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, testMethodArguments)
        {
        }

        public override async Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            var runner = new OnTreeTestCaseRunner((IXunitTestCase) this, this.DisplayName, this.SkipReason,
                constructorArguments, this.TestMethodArguments, messageBus, aggregator, cancellationTokenSource);
            return await runner.RunAsync();;
        }
    }

    internal class OnTreeTestCaseRunner : XunitTestCaseRunner
    {
        public OnTreeTestCaseRunner(IXunitTestCase testCase, string displayName, string skipReason,
            object[] constructorArguments, object[] testMethodArguments, IMessageBus messageBus,
            ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
            : base(testCase, displayName, skipReason, constructorArguments, testMethodArguments,
                messageBus, aggregator, cancellationTokenSource)
        {
        }
        
        protected override XunitTestRunner CreateTestRunner(
            ITest test,
            IMessageBus messageBus,
            Type testClass,
            object[] constructorArguments,
            MethodInfo testMethod,
            object[] testMethodArguments,
            string skipReason,
            IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            return new OnTreeTestRunner(test, messageBus, testClass, constructorArguments, testMethod,
                testMethodArguments, skipReason, beforeAfterAttributes,
                new ExceptionAggregator(aggregator), cancellationTokenSource);
        }
    }

    internal class OnTreeTestRunner : XunitTestRunner
    {
        public OnTreeTestRunner(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, 
            MethodInfo testMethod, object[] testMethodArguments, string skipReason, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
            ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource) 
            : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments,
                skipReason, beforeAfterAttributes, aggregator, cancellationTokenSource)
        {
        }
        
        protected override Task<Decimal> InvokeTestMethodAsync(ExceptionAggregator aggregator)
        {
            return new OnTreeTestInvoker(this.Test, this.MessageBus, this.TestClass, this.ConstructorArguments,
                this.TestMethod, this.TestMethodArguments, this.BeforeAfterAttributes, aggregator,
                this.CancellationTokenSource).RunAsync();
        }
    }

    internal class OnTreeTestInvoker : XunitTestInvoker
    {
        private Node addingToTree;
        
        public OnTreeTestInvoker(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments,
            MethodInfo testMethod, object[] testMethodArguments, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
            ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource) 
            : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments,
                beforeAfterAttributes, aggregator, cancellationTokenSource)
        {
        }

        protected override object CreateTestClass()
        {
            var check = base.CreateTestClass();
            if (check is Node node)
            {
                addingToTree = node;
            }
            else
            {
                Aggregator.Add(new Exception($"test class must extends Godot.Node: {check.GetType()}"));
            }
            return check;
        }

        protected override async Task BeforeTestMethodInvokedAsync()
        {
            if (addingToTree != null)
            {
                await GDU.OnProcessAwaiter;
                GDU.Instance.AddChild(addingToTree);
                await GDU.OnProcessAwaiter;
            }
            
            await base.BeforeTestMethodInvokedAsync();
        }

        protected override async Task AfterTestMethodInvokedAsync()
        {
            await base.AfterTestMethodInvokedAsync();

            if (addingToTree != null)
            {
                await GDU.OnProcessAwaiter;
                GDU.Instance.RemoveChild(addingToTree);
                await GDU.OnProcessAwaiter;
            }
        }
    }
}