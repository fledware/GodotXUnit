using System.Collections.Concurrent;
using System.IO;
using Godot;

namespace GodotXUnitApi.Internal
{
    public class MessageSender
    {
        public int idAt { get; private set; }

        public int NextId()
        {
            return ++idAt;
        }

        public void SendMessage(object message)
        {
            WorkFiles.WriteFile(NextId().ToString(), message);
        }
    }

    public class MessageWatcher
    {
        private FileSystemWatcher _watcher = null;
        
        private ConcurrentQueue<object> queue = new ConcurrentQueue<object>();
        
        public object Poll()
        {
            if (queue.TryDequeue(out var result)) return result;
            return null;
        }

        public void Start()
        {
            if (_watcher == null)
            {
                _watcher = new FileSystemWatcher(WorkFiles.PassDir);
                _watcher.Filter = "*.json";
                _watcher.Created += OnCreated;
                _watcher.Error += OnError;
            }
            _watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            if (_watcher == null) return;
            _watcher.EnableRaisingEvents = false;
            while (Poll() != null) { }
        }

        private void OnCreated(object sender, FileSystemEventArgs eventArgs)
        {
            queue.Enqueue(WorkFiles.ReadFile(eventArgs.Name, true));
        }

        private void OnError(object sender, ErrorEventArgs eventArgs)
        {
            GD.PrintErr(eventArgs.GetException());
        }
    }
}