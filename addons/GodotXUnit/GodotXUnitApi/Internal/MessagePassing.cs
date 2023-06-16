using Godot;

namespace GodotXUnitApi.Internal
{
    public partial class MessageSender
    {
        public int idAt { get; private set; }

        public int NextId()
        {
            return ++idAt;
        }

        public void SendMessage(object message, string type)
        {
            WorkFiles.WriteFile($"{NextId().ToString()}-{type}", message);
        }
    }

    public partial class MessageWatcher
    {
        private DirAccess directory = new DirAccess();
        
        public object Poll()
        {
            directory.ChangeDir(WorkFiles.WorkDir).ThrowIfNotOk();
            directory.ListDirBegin(true, true).ThrowIfNotOk();
            try
            {
                while (true)
                {
                    var next = directory.GetNext();
                    if (string.IsNullOrEmpty(next)) break;
                    if (directory.FileExists(next))
                    {
                        var result = WorkFiles.ReadFile(next);
                        directory.Remove(next);
                        return result;
                    }
                }
            }
            finally
            {
                directory.ListDirEnd();
            }
            return null;
        }
    }
}