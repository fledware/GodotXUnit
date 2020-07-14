using System;
using Godot;

namespace GodotCSUnitTest
{
    public class GodotEvents : Node
    {
        private static GodotEvents _instance;
        public static GodotEvents Instance
        {
            get => _instance ?? throw new Exception("GodotEvents not set");
            set => _instance = value;
        }
        
        [Signal]
        public delegate void OnProcess();

        public SignalAwaiter OnProcessSignal => ToSignal(this, nameof(OnProcess));
        
        [Signal]
        public delegate void OnPhysicsProcess();

        public SignalAwaiter OnPhysicsProcessSignal => ToSignal(this, nameof(OnPhysicsProcess));
    }
}