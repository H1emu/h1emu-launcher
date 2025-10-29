// This file is subject to the terms and conditions defined
// in file 'LICENSE', which is part of this source code package.

using SteamKit2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace H1Emu_Launcher
{
    static class Util
    {
        public static string GetSteamOS()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "windows";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "macos";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "linux";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                // Return linux as freebsd steam client doesn't exist yet
                return "linux";
            }

            return "unknown";
        }

        public static string GetSteamArch()
        {
            return Environment.Is64BitOperatingSystem ? "64" : "32";
        }

        public static string ReadPassword()
        {
            ConsoleKeyInfo keyInfo;
            var password = new StringBuilder();

            do
            {
                keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password.Remove(password.Length - 1, 1);
                        Debug.WriteLine("\b \b");
                    }

                    continue;
                }

                /* Printable ASCII characters only */
                var c = keyInfo.KeyChar;
                if (c >= ' ' && c <= '~')
                {
                    password.Append(c);
                    Debug.WriteLine('*');
                }
            } while (keyInfo.Key != ConsoleKey.Enter);

            return password.ToString();
        }

        // Validate a file against Steam3 Chunk data
        public static List<ProtoManifest.ChunkData> ValidateSteam3FileChecksums(FileStream fs, ProtoManifest.ChunkData[] chunkdata)
        {
            var neededChunks = new List<ProtoManifest.ChunkData>();

            foreach (var data in chunkdata)
            {
                fs.Seek((long)data.Offset, SeekOrigin.Begin);

                var adler = AdlerHash(fs, (int)data.UncompressedLength);
                if (!adler.SequenceEqual(data.Checksum))
                {
                    neededChunks.Add(data);
                }
            }

            return neededChunks;
        }

        public static byte[] AdlerHash(Stream stream, int length)
        {
            uint a = 0, b = 0;
            for (var i = 0; i < length; i++)
            {
                var c = (uint)stream.ReadByte();

                a = (a + c) % 65521;
                b = (b + a) % 65521;
            }

            return BitConverter.GetBytes(a | (b << 16));
        }

        public static byte[] DecodeHexString(string hex)
        {
            if (hex == null)
                return null;

            var chars = hex.Length;
            var bytes = new byte[chars / 2];

            for (var i = 0; i < chars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            return bytes;
        }

        public static EOSType GetOSType()
        {
            var osVer = Environment.OSVersion;
            var ver = osVer.Version;

            return osVer.Platform switch
            {
                PlatformID.Win32Windows => ver.Minor switch
                {
                    0 => EOSType.Win95,
                    10 => EOSType.Win98,
                    90 => EOSType.WinME,
                    _ => EOSType.WinUnknown,
                },

                PlatformID.Win32NT => ver.Major switch
                {
                    4 => EOSType.WinNT,
                    5 => ver.Minor switch
                    {
                        0 => EOSType.Win2000,
                        1 => EOSType.WinXP,
                        // Assume nobody runs Windows XP Professional x64 Edition
                        // It's an edition of Windows Server 2003 anyway.
                        2 => EOSType.Win2003,
                        _ => EOSType.WinUnknown,
                    },
                    6 => ver.Minor switch
                    {
                        0 => EOSType.WinVista, // Also Server 2008
                        1 => EOSType.Windows7, // Also Server 2008 R2
                        2 => EOSType.Windows8, // Also Server 2012
                        // Note: The OSVersion property reports the same version number (6.2.0.0) for both Windows 8 and Windows 8.1.- http://msdn.microsoft.com/en-us/library/system.environment.osversion(v=vs.110).aspx
                        // In practice, this will only get hit if the application targets Windows 8.1 in the app manifest.
                        // See http://msdn.microsoft.com/en-us/library/windows/desktop/dn481241(v=vs.85).aspx for more info.
                        3 => EOSType.Windows81, // Also Server 2012 R2
                        _ => EOSType.WinUnknown,
                    },
                    10 when ver.Build >= 22000 => EOSType.Win11,
                    10 => EOSType.Windows10,// Also Server 2016, Server 2019, Server 2022
                    _ => EOSType.WinUnknown,
                },

                // The specific minor versions only exist in Valve's enum for LTS versions
                PlatformID.Unix when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => ver.Major switch
                {
                    2 => ver.Minor switch
                    {
                        2 => EOSType.Linux22,
                        4 => EOSType.Linux24,
                        6 => EOSType.Linux26,
                        _ => EOSType.LinuxUnknown,
                    },
                    3 => ver.Minor switch
                    {
                        2 => EOSType.Linux32,
                        5 => EOSType.Linux35,
                        6 => EOSType.Linux36,
                        10 => EOSType.Linux310,
                        16 => EOSType.Linux316,
                        18 => EOSType.Linux318,
                        _ => EOSType.Linux3x,
                    },
                    4 => ver.Minor switch
                    {
                        1 => EOSType.Linux41,
                        4 => EOSType.Linux44,
                        9 => EOSType.Linux49,
                        14 => EOSType.Linux414,
                        19 => EOSType.Linux419,
                        _ => EOSType.Linux4x,
                    },
                    5 => ver.Minor switch
                    {
                        4 => EOSType.Linux54,
                        10 => EOSType.Linux510,
                        _ => EOSType.Linux5x,
                    },
                    6 => EOSType.Linux6x,
                    7 => EOSType.Linux7x,
                    _ => EOSType.LinuxUnknown,
                },

                PlatformID.Unix when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => ver.Major switch
                {
                    11 => EOSType.MacOS107, // "Lion"
                    12 => EOSType.MacOS108, // "Mountain Lion"
                    13 => EOSType.MacOS109, // "Mavericks"
                    14 => EOSType.MacOS1010, // "Yosemite"
                    15 => EOSType.MacOS1011, // El Capitan
                    16 => EOSType.MacOS1012, // Sierra
                    17 => EOSType.Macos1013, // High Sierra
                    18 => EOSType.Macos1014, // Mojave
                    19 => EOSType.Macos1015, // Catalina
                    20 => EOSType.MacOS11, // Big Sur
                    21 => EOSType.MacOS12, // Monterey
                    22 => EOSType.MacOS13, // Ventura
                    23 => EOSType.MacOS14, // Sonoma
                    24 => EOSType.MacOS15, // Sequoia
                    _ => EOSType.MacOSUnknown,
                },

                _ => EOSType.Unknown,
            };
        }

        /// <summary>
        /// Decrypts using AES/ECB/PKCS7
        /// </summary>
        public static byte[] SymmetricDecryptECB(byte[] input, byte[] key)
        {
            using var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            using var aesTransform = aes.CreateDecryptor(key, null);
            var output = aesTransform.TransformFinalBlock(input, 0, input.Length);

            return output;
        }

        public static async Task InvokeAsync(IEnumerable<Func<Task>> taskFactories, int maxDegreeOfParallelism)
        {
            ArgumentNullException.ThrowIfNull(taskFactories);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxDegreeOfParallelism, 0);

            var queue = taskFactories.ToArray();

            if (queue.Length == 0)
            {
                return;
            }

            var tasksInFlight = new List<Task>(maxDegreeOfParallelism);
            var index = 0;

            do
            {
                while (tasksInFlight.Count < maxDegreeOfParallelism && index < queue.Length)
                {
                    var taskFactory = queue[index++];

                    tasksInFlight.Add(taskFactory());
                }

                var completedTask = await Task.WhenAny(tasksInFlight).ConfigureAwait(false);

                await completedTask.ConfigureAwait(false);

                tasksInFlight.Remove(completedTask);
            } while (index < queue.Length || tasksInFlight.Count != 0);
        }
    }
}