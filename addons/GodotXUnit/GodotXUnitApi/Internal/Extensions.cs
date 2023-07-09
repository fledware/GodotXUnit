using System;
using Godot;

namespace GodotXUnitApi.Internal
{
    public static class Extensions
    {
        public static FileAccess ThrowIfNotOk(this FileAccess file)
        {
            if (file is null)
            {
                ThrowIfNotOk(FileAccess.GetOpenError());
            }
            return file;
        }

        public static DirAccess ThrowIfNotOk(this DirAccess dir)
        {
            if (dir is null)
            {
                ThrowIfNotOk(DirAccess.GetOpenError());
            }
            return dir;
        }

        public static void ThrowIfNotOk(this Error check)
        {
            if (check == Error.Ok) return;
            throw new Exception($"godot error returned: {check}");
        }
    }
}