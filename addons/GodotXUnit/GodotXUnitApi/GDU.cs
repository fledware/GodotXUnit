using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace GodotXUnitApi
{
    /// <summary>
    /// a global helper for interacting with the executing tree
    /// during a test.
    /// </summary>
    // here are a few usage examples:
    //
    // - get the current scene:
    // var scene = GDU.Tree.CurrentScene;
    //
    // - wait for 60 process frames
    // for (int i = 0; i < 60; i++)
    //     await GDU.OnProcessAwaiter;
    //
    // - move to the physics frame
    // await GDU.OnPhysicsProcessAwaiter;
    public class GDU : Node
    {
        private static GDU _instance;

        public static GDU Instance
        {
            get => _instance ?? throw new Exception("GDU not set");
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

        public static SceneTree Tree => Instance.GetTree();
        
        public static Node CurrentScene => Instance.GetTree().CurrentScene;
        
        public static async Task<object[]> ToSignalWithTimeout(
            Godot.Object source,
            string signal,
            int timeoutMillis,
            bool throwOnTimeout = true)
        {
            var awaiter = source.ToSignal(source, signal);
            var task = Task.Run(async () => await awaiter);
            using (var token = new CancellationTokenSource()) {

                var completedTask = await Task.WhenAny(task, Task.Delay(timeoutMillis, token.Token));
                if (completedTask == task) {
                    token.Cancel();
                    return await task;
                }
                if (throwOnTimeout)
                    throw new TimeoutException($"signal {signal} on {source.GetType()} timed out after {timeoutMillis}ms.");
                return null;
            }
        }
    }
}