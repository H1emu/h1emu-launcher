// This file is subject to the terms and conditions defined
// in file 'LICENSE', which is part of this source code package.

using System.IO;
using System.Runtime.InteropServices;

namespace H1Emu_Launcher
{
    static class PlatformUtilities
    {
        public static void SetExecutable(string path, bool value)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            const UnixFileMode ModeExecute = UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;

            var mode = File.GetUnixFileMode(path);
            var hasExecuteMask = (mode & ModeExecute) == ModeExecute;
            if (hasExecuteMask != value)
            {
                File.SetUnixFileMode(path, value
                    ? mode | ModeExecute
                    : mode & ~ModeExecute);
            }
        }
    }
}