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
        public object Poll()
        {
            var directory = DirAccess.Open(WorkFiles.WorkDir).ThrowIfNotOk();
            directory.IncludeHidden = false;
            directory.IncludeNavigational = false;
            directory.ListDirBegin().ThrowIfNotOk();
            try
            {
                while (true)
                {
                    var next = directory.GetNext();
                    if (string.IsNullOrEmpty(next)) break;
                    if (directory.FileExists(next))
                    {
                        var result = WorkFiles.ReadFile(next);
                        directory.Remove(next).ThrowIfNotOk();
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