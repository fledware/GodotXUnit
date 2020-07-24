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
            System.IO.Path.DirectorySeparatorChar.ToString();

        public static string PassFile(string passDir, int id)
        {
            return $"{passDir}{sep}{id}.json";
        }

        public static string FigurePassDir()
        {
            var current = System.IO.Directory.GetCurrentDirectory();
            while (!string.IsNullOrEmpty(current))
            {
                if (System.IO.File.Exists($"{current}{sep}project.godot"))
                    return $"{current}{sep}addons{sep}GodotXUnit{sep}_msg";
                current = System.IO.Directory.GetParent(current).FullName;
            }
            GD.PrintErr("unable to find root of godot project");
            throw new Exception("unable to find root dir");
        }
    }

    public class MessageSender
    {
        public int idAt { get; private set; }
        public readonly string passDir = MessagePassing.FigurePassDir();

        public int NextId()
        {
            return ++idAt;
        }

        public void SendMessage(object message)
        {
            var id = NextId();
            var sending = JsonConvert.SerializeObject(message, Formatting.Indented,MessagePassing.jsonSettings);
            System.IO.File.WriteAllText(MessagePassing.PassFile(passDir, id), sending);
        }

        public void EnsureMessageDirectory()
        {
            var directory = new Godot.Directory();
            directory.MakeDirRecursive(passDir).ThrowIfNotOk();
            directory.Open(passDir).ThrowIfNotOk();
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
        
        public readonly string passDir = MessagePassing.FigurePassDir();
        
        public object Poll()
        {
            if (queue.TryDequeue(out var result)) return result;
            return null;
        }

        public void Start()
        {
            if (_watcher == null)
            {
                _watcher = new FileSystemWatcher(passDir);
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