using System;
using System.Collections.Concurrent;
using System.IO;
using Godot;
using Newtonsoft.Json;

namespace GodotXUnitApi
{
    public static class MessagePassing
    {
        public static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };
        
        public static readonly string sep =
            System.IO.Path.PathSeparator.ToString();
        
        public static string PassDir =>
            $"{System.IO.Directory.GetCurrentDirectory()}{sep}addons{sep}GodotXUnit{sep}_msg";

        public static string PassFile(int id)
        {
            return $"{PassDir}{sep}{id}.json";
        }
    }

    public class MessageSender
    {
        public int idAt { get; private set; }

        public int NextId()
        {
            return ++idAt;
        }

        public void SendMessage(object message)
        {
            var id = NextId();
            var sending = JsonConvert.SerializeObject(message, MessagePassing.jsonSettings);
            System.IO.File.WriteAllText(MessagePassing.PassFile(id), sending);
        }

        public void EnsureMessageDirectory()
        {
            var directory = new Godot.Directory();
            directory.MakeDirRecursive(MessagePassing.PassDir).ThrowIfNotOk();
            directory.Open(MessagePassing.PassDir).ThrowIfNotOk();
            directory.ListDirBegin().ThrowIfNotOk();
            while (true)
            {
                var next = directory.GetNext();
                if (next.EndsWith(".json"))
                {
                    directory.Remove(next).ThrowIfNotOk();
                }
                if (string.IsNullOrEmpty(next))
                    break;
            }
            directory.ListDirEnd();
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
                _watcher = new FileSystemWatcher(MessagePassing.PassDir);
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
        }

        private void OnCreated(object sender, FileSystemEventArgs eventArgs)
        {
            var rawMessage = System.IO.File.ReadAllText(eventArgs.FullPath);
            System.IO.File.Delete(eventArgs.FullPath);
            var message = JsonConvert.DeserializeObject(rawMessage, MessagePassing.jsonSettings);
            queue.Enqueue(message);
        }

        private void OnError(object sender, ErrorEventArgs eventArgs)
        {
            GD.PrintErr(eventArgs.GetException());
        }
    }
}