using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using GodotXUnitApi.Internal;

namespace GodotXUnitApi
{
    /// <summary>
    /// a global helper for interacting with the executing tree
    /// during a test.
    /// </summary>
    // here are a few usage examples:
    //
    // - get the current scene:
    // var scene = GDU.CurrentScene;
    //
    // - wait for 60 process frames
    // await GDU.WaitForFrames(60);
    //
    // - move to the physics frame
    // await GDU.OnPhysicsProcessAwaiter;
    public class GDU
    {
        private static Node2D _instance;

        public static Node2D Instance
        {
            get => _instance ?? throw new Exception("GDU not set");
            set => _instance = value;
        }

        public static SignalAwaiter OnProcessAwaiter =>
            Instance.ToSignal(Instance, "OnProcess");

        public static SignalAwaiter OnPhysicsProcessAwaiter =>
            Instance.ToSignal(Instance, "OnPhysicsProcess");

        public static SignalAwaiter OnIdleFrameAwaiter =>
            Instance.ToSignal(Instance.GetTree(), "idle_frame");

        public static SceneTree Tree => Instance.GetTree();

        public static Viewport Viewport => Instance.GetViewport();
        
        public static Node CurrentScene => Instance.GetTree().CurrentScene;

        /// <summary>
        /// this can be used within tests instead of grabbing ITestOutputHelper
        /// </summary>
        /// <param name="message"></param>
        public static void Print(string message)
        {
            // when [GodotFact] is used, the console output stream is
            // automatically overridden for each test. but this will
            // avoid the annoying warnings.
            Console.WriteLine(message);
        }

        public static async Task WaitForFrames(int count)
        {
            for (int i = 0; i < count; i++)
                await OnProcessAwaiter;
        }
        
        public static async Task<object[]> ToSignalWithTimeout(
            Godot.Object source,
            string signal,
            int timeoutMillis,
            bool throwOnTimeout = true)
        {
            return await AwaitWithTimeout(source.ToSignal(source, signal), timeoutMillis, throwOnTimeout);
        }
        
        public static async Task<object[]> AwaitWithTimeout(
            SignalAwaiter awaiter,
            int timeoutMillis,
            bool throwOnTimeout = true)
        {
            var task = Task.Run(async () => await awaiter);
            using var token = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeoutMillis, token.Token));
            if (completedTask == task) {
                token.Cancel();
                return await task;
            }
            if (throwOnTimeout)
                throw new TimeoutException($"signal {awaiter} timed out after {timeoutMillis}ms.");
            return null;
        }

        public static async Task RequestDrawing(int frames, Action<Node2D> drawer)
        {
            for (int i = 0; i < frames; i++)
            {
                ((GodotXUnitRunner) Instance).RequestDraw(drawer);
                await Instance.ToSignal(Instance, "OnDrawRequestDone");
            }
        }
    }
}