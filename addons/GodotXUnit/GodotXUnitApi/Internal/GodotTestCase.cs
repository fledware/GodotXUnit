using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GodotXUnitApi.Internal
{
    public partial class GodotTestCase : XunitTestCase
    {
        private IAttributeInfo attribute;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public GodotTestCase() { }

        public GodotTestCase(IAttributeInfo attribute,
                             IMessageSink diagnosticMessageSink,
                             ITestFrameworkDiscoveryOptions discoveryOptions,
                             ITestMethod testMethod,
                             object[] testMethodArguments = null)
            : base(diagnosticMessageSink,
                   discoveryOptions.MethodDisplayOrDefault(),
                   discoveryOptions.MethodDisplayOptionsOrDefault(),
                   testMethod,
                   testMethodArguments)
        {
            this.attribute = attribute;
        }

        public override async Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink,
                                                        IMessageBus messageBus,
                                                        object[] constructorArguments,
                                                        ExceptionAggregator aggregator,
                                                        CancellationTokenSource cancellationTokenSource)
        {
            return await new GodotTestCaseRunner(attribute,
                                                 this,
                                                 DisplayName,
                                                 SkipReason,
                                                 constructorArguments,
                                                 TestMethodArguments,
                                                 messageBus,
                                                 aggregator,
                                                 cancellationTokenSource)
                .RunAsync();
        }
    }

    public partial class GodotTestCaseRunner : XunitTestCaseRunner
    {
        private IAttributeInfo attribute;

        public GodotTestCaseRunner(IAttributeInfo attribute,
                                   IXunitTestCase testCase,
                                   string displayName,
                                   string skipReason,
                                   object[] constructorArguments,
                                   object[] testMethodArguments,
                                   IMessageBus messageBus,
                                   ExceptionAggregator aggregator,
                                   CancellationTokenSource cancellationTokenSource)
            : base(testCase,
                   displayName,
                   skipReason,
                   constructorArguments,
                   testMethodArguments,
                   messageBus,
                   aggregator,
                   cancellationTokenSource)
        {
            this.attribute = attribute;
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
            return new GodotTestRunner(attribute,
                                       test,
                                       messageBus,
                                       testClass,
                                       constructorArguments,
                                       testMethod,
                                       testMethodArguments,
                                       skipReason,
                                       beforeAfterAttributes,
                                       new ExceptionAggregator(aggregator),
                                       cancellationTokenSource);
        }
    }

    public partial class GodotTestRunner : XunitTestRunner
    {
        private IAttributeInfo attribute;

        public GodotTestRunner(IAttributeInfo attribute,
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
            : base(test,
                   messageBus,
                   testClass,
                   constructorArguments,
                   testMethod,
                   testMethodArguments,
                   skipReason,
                   beforeAfterAttributes,
                   aggregator,
                   cancellationTokenSource)
        {
            this.attribute = attribute;
        }

        protected override async Task<Tuple<Decimal, string>> InvokeTestAsync(ExceptionAggregator aggregator)
        {

            // override the ITestOutputHelper from XunitTestClassRunner
            TestOutputHelper helper = null;
            for (int i = 0; i < ConstructorArguments.Length; i++)
            {
                if (ConstructorArguments[i] is ITestOutputHelper)
                {
                    helper = (TestOutputHelper)ConstructorArguments[i];
                    break;
                }
            }
            var output = new GodotTestOutputHelper(helper);
            output.Initialize(MessageBus, Test);
            var runTime = await InvokeTestMethodAsync(aggregator);
            return Tuple.Create(runTime, output.UnInitAndGetOutput());
        }

        protected override Task<Decimal> InvokeTestMethodAsync(ExceptionAggregator aggregator)
        {
            return new GodotTestInvoker(attribute,
                                        this.Test,
                                        this.MessageBus,
                                        this.TestClass,
                                        this.ConstructorArguments,
                                        this.TestMethod,
                                        this.TestMethodArguments,
                                        this.BeforeAfterAttributes,
                                        aggregator,
                                        this.CancellationTokenSource)
                .RunAsync();
        }
    }

    public partial class GodotTestInvoker : XunitTestInvoker
    {
        private IAttributeInfo attribute;

        private Node addingToTree;

        private bool loadEmptyScene;

        public GodotTestInvoker(IAttributeInfo attribute,
                                ITest test,
                                IMessageBus messageBus,
                                Type testClass,
                                object[] constructorArguments,
                                MethodInfo testMethod,
                                object[] testMethodArguments,
                                IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
                                ExceptionAggregator aggregator,
                                CancellationTokenSource cancellationTokenSource)
            : base(test,
                   messageBus,
                   testClass,
                   constructorArguments,
                   testMethod,
                   testMethodArguments,
                   beforeAfterAttributes,
                   aggregator,
                   cancellationTokenSource)
        {
            this.attribute = attribute;
        }

        protected override object CreateTestClass()
        {
            var check = base.CreateTestClass();
            if (check is Node node)
                addingToTree = node;

            return check;
        }

        /// <summary>
        /// Runs the given function in Godot's main thread.
        /// </summary>
        /// <param name="fn"></param>
        private void CallInGodotMain(Func<Task> fn)
        {
            Exception caught = null;
            var semaphore = new Godot.Semaphore();
            // Create a callable and use CallDeferred to add it to Godot's
            // execution queue, which will run the function in the main thread.
            // Callables do not (as of Godot 4.1) accept Task functions.
            // Wrapping it in an action makes it fire-and-forget, which is
            // fine; we're using a semaphore to signal completion anyway.
            Callable.From(new Action(async () =>
            {
                try
                {
                    await fn();
                }
                catch (AggregateException aggregate)
                {
                    caught = aggregate.InnerException;
                }
                catch (Exception e)
                {
                    caught = e;
                }
                finally
                {
                    semaphore.Post();
                }
            })).CallDeferred();
            // Note: We're blocking the thread here. Is that a bad thing?
            // It's probably a XUnit worker thread, so maybe its fine, but
            // if any deadlocks are discovered we might want to spawn a new
            // thread for this whole operation. It might be nicer if this whole
            // method was async anyway.
            semaphore.Wait();
            if (caught is not null)
            {
                throw caught;
            }
        }

        protected override async Task BeforeTestMethodInvokedAsync()
        {
            var sceneCheck = attribute.GetNamedArgument<string>(nameof(GodotFactAttribute.Scene));
            try
            {
                CallInGodotMain(async () =>
                {
                    if (!string.IsNullOrEmpty(sceneCheck))
                    {
                        // you must be in the process frame to 
                        await GDU.OnProcessAwaiter;

                        if (GDU.Instance.GetTree().ChangeSceneToFile(sceneCheck) != Error.Ok)
                        {
                            Aggregator.Add(new Exception($"could not load scene: {sceneCheck}"));
                            return;
                        }
                        loadEmptyScene = true;

                        // the scene should be loaded within two frames
                        await GDU.OnProcessFrameAwaiter;
                        await GDU.OnProcessFrameAwaiter;
                        await GDU.OnProcessAwaiter;
                    }

                    if (addingToTree != null)
                    {
                        await GDU.OnProcessAwaiter;
                        GDU.Instance.AddChild(addingToTree);
                        await GDU.OnProcessAwaiter;
                    }
                });

            }
            catch (Exception e)
            {
                Aggregator.Add(e);
            }
            await base.BeforeTestMethodInvokedAsync();
        }

        protected override async Task<decimal> InvokeTestMethodAsync(object testClassInstance)
        {
            decimal result = default;
            CallInGodotMain(async () =>
            {
                var sceneCheck = attribute.GetNamedArgument<GodotFactFrame>(nameof(GodotFactAttribute.Frame));
                switch (sceneCheck)
                {
                    case GodotFactFrame.Default:
                        break;
                    case GodotFactFrame.Process:
                        await GDU.OnProcessAwaiter;
                        break;
                    case GodotFactFrame.PhysicsProcess:
                        await GDU.OnPhysicsProcessAwaiter;
                        break;
                    default:
                        Aggregator.Add(new Exception($"unknown GodotFactFrame: {sceneCheck.ToString()}"));
                        throw new ArgumentOutOfRangeException();
                }
                result = await base.InvokeTestMethodAsync(testClassInstance);
            });
            return result;
        }

        protected override async Task AfterTestMethodInvokedAsync()
        {
            await base.AfterTestMethodInvokedAsync();
            CallInGodotMain(async () =>
            {
                if (addingToTree != null)
                {
                    await GDU.OnProcessAwaiter;
                    GDU.Instance.RemoveChild(addingToTree);
                    await GDU.OnProcessAwaiter;
                }

                if (loadEmptyScene)
                {
                    // change scenes again and wait for godot to catch up
                    GDU.Instance.GetTree().ChangeSceneToFile(Consts.EMPTY_SCENE_PATH);
                    await GDU.OnProcessFrameAwaiter;
                    await GDU.OnProcessFrameAwaiter;
                    await GDU.OnProcessAwaiter;
                }
            });
        }
    }
}