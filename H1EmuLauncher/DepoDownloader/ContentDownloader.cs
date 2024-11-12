﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using H1EmuLauncher.SteamFramePages;
using SteamKit2;
using SteamKit2.CDN;

namespace H1EmuLauncher
{
    public class ContentDownloaderException : Exception
    {
        public ContentDownloaderException( String value ) : base( value ) {}
    }

    static class ContentDownloader
    {
        public const uint INVALID_APP_ID = uint.MaxValue;
        public const uint INVALID_DEPOT_ID = uint.MaxValue;
        public const ulong INVALID_MANIFEST_ID = ulong.MaxValue;
        public const string DEFAULT_BRANCH = "Public";

        public static DownloadConfig Config = new();

        public static Steam3Session steam3;
        public static Steam3Session.Credentials steam3Credentials;
        public static CDNClientPool cdnPool;

        public static string DEFAULT_DOWNLOAD_DIR = "depots";
        public static string CONFIG_DIR = "DepotDownloader";
        public static string STAGING_DIR = Path.Combine(CONFIG_DIR, "staging");
        
        private sealed class DepotDownloadInfo
        {
            public uint id { get; private set; }
            public uint appId { get; private set; }
            public ulong manifestId { get; private set; }
            public string branch { get; private set; }
            public string installDir { get; private set; }
            public string contentName { get; private set; }


            public byte[] depotKey;

            public DepotDownloadInfo(
                uint depotid, uint appId, ulong manifestId, string branch,
                string installDir, string contentName,
                byte[] depotKey)
            {
                this.id = depotid;
                this.appId = appId;
                this.manifestId = manifestId;
                this.branch = branch;
                this.installDir = installDir;
                this.contentName = contentName;
                this.depotKey = depotKey;
            }
        }

        static bool CreateDirectories( uint depotId, uint depotVersion, out string installDir)
        {
            installDir = null;

            try
            {
                Directory.CreateDirectory(DEFAULT_DOWNLOAD_DIR);

                installDir = DEFAULT_DOWNLOAD_DIR;

                Directory.CreateDirectory(Path.Combine(installDir, CONFIG_DIR));
                Directory.CreateDirectory(Path.Combine(installDir, STAGING_DIR));

            }
            catch
            {
                return false;
            }

            return true;
        }

        static bool TestIsFileIncluded( string filename )
        {
            if ( !Config.UsingFileList )
                return true;

            filename = filename.Replace('\\', '/');

            if (Config.FilesToDownload.Contains(filename))
            {
                return true;
            }

            foreach (var rgx in Config.FilesToDownloadRegex)
            {
                var m = rgx.Match(filename);

                if ( m.Success )
                    return true;
            }

            return false;
        }

        public static bool AccountHasAccess( uint depotId )
        {
            if ( steam3 == null || steam3.steamUser.SteamID == null || ( steam3.Licenses == null && steam3.steamUser.SteamID.AccountType != EAccountType.AnonUser ) )
                return false;

            IEnumerable<uint> licenseQuery;
            if ( steam3.steamUser.SteamID.AccountType == EAccountType.AnonUser )
            {
                licenseQuery = new List<uint>() { 17906 };
            }
            else
            {
                licenseQuery = steam3.Licenses.Select( x => x.PackageID ).Distinct();
            }

            steam3.RequestPackageInfo( licenseQuery );

            foreach ( var license in licenseQuery )
            {
                SteamApps.PICSProductInfoCallback.PICSProductInfo package;
                if ( steam3.PackageInfo.TryGetValue( license, out package ) && package != null )
                {
                    if ( package.KeyValues[ "appids" ].Children.Any( child => child.AsUnsignedInteger() == depotId ) )
                        return true;

                    if ( package.KeyValues[ "depotids" ].Children.Any( child => child.AsUnsignedInteger() == depotId ) )
                        return true;
                }
            }

            return false;
        }

        internal static KeyValue GetSteam3AppSection( uint appId, EAppInfoSection section )
        {
            if ( steam3 == null || steam3.AppInfo == null )
            {
                return null;
            }

            SteamApps.PICSProductInfoCallback.PICSProductInfo app;
            if ( !steam3.AppInfo.TryGetValue( appId, out app ) || app == null )
            {
                return null;
            }

            var appinfo = app.KeyValues;
            string section_key;

            switch ( section )
            {
                case EAppInfoSection.Common:
                    section_key = "common";
                    break;
                case EAppInfoSection.Extended:
                    section_key = "extended";
                    break;
                case EAppInfoSection.Config:
                    section_key = "config";
                    break;
                case EAppInfoSection.Depots:
                    section_key = "depots";
                    break;
                default:
                    throw new NotImplementedException();
            }

            var section_kv = appinfo.Children.Where( c => c.Name == section_key ).FirstOrDefault();
            return section_kv;
        }

        static uint GetSteam3AppBuildNumber( uint appId, string branch )
        {
            if ( appId == INVALID_APP_ID )
                return 0;

            var depots = ContentDownloader.GetSteam3AppSection( appId, EAppInfoSection.Depots );
            var branches = depots[ "branches" ];
            var node = branches[ branch ];

            if ( node == KeyValue.Invalid )
                return 0;

            var buildid = node[ "buildid" ];

            if ( buildid == KeyValue.Invalid )
                return 0;

            return uint.Parse( buildid.Value );
        }

        static uint GetSteam3DepotProxyAppId(uint depotId, uint appId)
        {
            var depots = GetSteam3AppSection(appId, EAppInfoSection.Depots);
            var depotChild = depots[depotId.ToString()];

            if (depotChild == KeyValue.Invalid)
                return INVALID_APP_ID;

            if (depotChild["depotfromapp"] == KeyValue.Invalid)
                return INVALID_APP_ID;

            return depotChild["depotfromapp"].AsUnsignedInteger();
        }

        static ulong GetSteam3DepotManifest( uint depotId, uint appId, string branch )
        {
            var depots = GetSteam3AppSection( appId, EAppInfoSection.Depots );
            var depotChild = depots[ depotId.ToString() ];

            if ( depotChild == KeyValue.Invalid )
                return INVALID_MANIFEST_ID;

            // Shared depots can either provide manifests, or leave you relying on their parent app.
            // It seems that with the latter, "sharedinstall" will exist (and equals 2 in the one existance I know of).
            // Rather than relay on the unknown sharedinstall key, just look for manifests. Test cases: 111710, 346680.
            if ( depotChild[ "manifests" ] == KeyValue.Invalid && depotChild[ "depotfromapp" ] != KeyValue.Invalid )
            {
                var otherAppId = depotChild["depotfromapp"].AsUnsignedInteger();
                if ( otherAppId == appId )
                {
                    // This shouldn't ever happen, but ya never know with Valve. Don't infinite loop.
                    Debug.WriteLine( "App {0}, Depot {1} has depotfromapp of {2}!",
                        appId, depotId, otherAppId );
                    return INVALID_MANIFEST_ID;
                }

                steam3.RequestAppInfo( otherAppId );

                return GetSteam3DepotManifest( depotId, otherAppId, branch );
            }

            var manifests = depotChild[ "manifests" ];
            var manifests_encrypted = depotChild[ "encryptedmanifests" ];

            if ( manifests.Children.Count == 0 && manifests_encrypted.Children.Count == 0 )
                return INVALID_MANIFEST_ID;

            var node = manifests[ branch ];

            if ( branch != "Public" && node == KeyValue.Invalid )
            {
                var node_encrypted = manifests_encrypted[ branch ];
                if ( node_encrypted != KeyValue.Invalid )
                {
                    var password = Config.BetaPassword;
                    if ( password == null )
                    {
                        Console.Write( "Please enter the password for branch {0}: ", branch );
                        Config.BetaPassword = password = Console.ReadLine();
                    }

                    var encrypted_v1 = node_encrypted[ "encrypted_gid" ];
                    var encrypted_v2 = node_encrypted[ "encrypted_gid_2" ];

                    if ( encrypted_v1 != KeyValue.Invalid )
                    {
                        var input = Util.DecodeHexString( encrypted_v1.Value );
                        var manifest_bytes = CryptoHelper.VerifyAndDecryptPassword( input, password );

                        if ( manifest_bytes == null )
                        {
                            Debug.WriteLine( "Password was invalid for branch {0}", branch );
                            return INVALID_MANIFEST_ID;
                        }

                        return BitConverter.ToUInt64( manifest_bytes, 0 );
                    }

                    if ( encrypted_v2 != KeyValue.Invalid )
                    {
                        // Submit the password to Steam now to get encryption keys
                        steam3.CheckAppBetaPassword( appId, Config.BetaPassword );

                        if ( !steam3.AppBetaPasswords.ContainsKey( branch ) )
                        {
                            Debug.WriteLine( "Password was invalid for branch {0}", branch );
                            return INVALID_MANIFEST_ID;
                        }

                        var input = Util.DecodeHexString( encrypted_v2.Value );
                        byte[] manifest_bytes;
                        try
                        {
                            manifest_bytes = CryptoHelper.SymmetricDecryptECB( input, steam3.AppBetaPasswords[ branch ] );
                        }
                        catch ( Exception e )
                        {
                            Debug.WriteLine( "Failed to decrypt branch {0}: {1}", branch, e.Message );
                            return INVALID_MANIFEST_ID;
                        }

                        return BitConverter.ToUInt64( manifest_bytes, 0 );
                    }

                    Debug.WriteLine("Unhandled depot encryption for depotId {0}", depotId);
                    return INVALID_MANIFEST_ID;
                }

                return INVALID_MANIFEST_ID;
            }

            if ( node.Value == null )
                return INVALID_MANIFEST_ID;

            return UInt64.Parse( node.Value );
        }

        static string GetAppOrDepotName( uint depotId, uint appId )
        {
            if ( depotId == INVALID_DEPOT_ID )
            {
                KeyValue info = GetSteam3AppSection( appId, EAppInfoSection.Common );

                if ( info == null )
                    return String.Empty;

                return info[ "name" ].AsString();
            }

            var depots = GetSteam3AppSection(appId, EAppInfoSection.Depots);

            if (depots == null)
                return String.Empty;

            var depotChild = depots[depotId.ToString()];

            if (depotChild == null)
                return String.Empty;

            return depotChild["name"].AsString();
        }

        public static bool InitializeSteam3( string username, string password )
        {
            string loginKey = null;

            if ( username != null && Config.RememberPassword )
            {
                _ = AccountSettingsStore.Instance.LoginKeys.TryGetValue( username, out loginKey );
            }

            steam3 = new Steam3Session(
                new SteamUser.LogOnDetails()
                {
                    Username = username,
                    Password = loginKey == null ? password : null,
                    ShouldRememberPassword = Config.RememberPassword,
                    AccessToken = loginKey,
                    LoginID = Config.LoginID ?? 0x534B32, // "SK2"
                }
            );

            steam3Credentials = steam3.WaitForCredentials();

            if ( !steam3Credentials.IsValid )
            {
                Debug.WriteLine("Unable to get steam3 credentials.");
                return false;
            }

            return true;
        }

        public static void ShutdownSteam3()
        {
            if (cdnPool != null)
            {
                cdnPool.Shutdown();
                cdnPool = null;
            }

            if ( steam3 == null )
                return;

            steam3.TryWaitForLoginKey();
            steam3.Disconnect();
        }

        public static async Task DownloadAppAsync( uint appId, List<(uint depotId, ulong manifestId)> depotManifestIds, string branch, string os, string arch, string language, bool lv, bool isUgc )
        {
            cdnPool = new CDNClientPool(steam3, appId);

            // Load our configuration data containing the depots currently installed
            var configPath = Config.InstallDirectory;
            if (string.IsNullOrWhiteSpace(configPath))
            {
                configPath = DEFAULT_DOWNLOAD_DIR;
            }

            Directory.CreateDirectory(Path.Combine(configPath, CONFIG_DIR));
            DepotConfigStore.LoadFromFile(Path.Combine(configPath, CONFIG_DIR, "depot.config"));

            if ( steam3 != null )
                steam3.RequestAppInfo( appId );

            if ( !AccountHasAccess( appId ) )
            {
                if ( steam3.RequestFreeAppLicense( appId ) )
                {
                    Debug.WriteLine( "Obtained FreeOnDemand license for app {0}", appId );

                    // Fetch app info again in case we didn't get it fully without a license.
                    steam3.RequestAppInfo( appId, true );
                }
                else
                {
                    var contentName = GetAppOrDepotName( INVALID_DEPOT_ID, appId );
                    throw new ContentDownloaderException($"App {appId} ({contentName}) is not available from this account.");
                }
            }

            var hasSpecificDepots = depotManifestIds.Count > 0;
            var depotIdsFound = new List<uint>();
            var depotIdsExpected = depotManifestIds.Select( x => x.Item1 ).ToList();
            var depots = GetSteam3AppSection( appId, EAppInfoSection.Depots );

            if ( isUgc )
            {
                var workshopDepot = depots["workshopdepot"].AsUnsignedInteger();
                if ( workshopDepot != 0 && !depotIdsExpected.Contains( workshopDepot ) )
                {
                    depotIdsExpected.Add( workshopDepot );
                    depotManifestIds = depotManifestIds.Select( pair => ( workshopDepot, pair.manifestId ) ).ToList();
                }

                depotIdsFound.AddRange( depotIdsExpected );
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    DownloadStatus.downloadStatusInstance.downloadProgressText.Text = LauncherWindow.launcherInstance.FindResource("item55").ToString().Replace("{0}", $"'{branch}'");
                }));

                Debug.WriteLine($"Using app branch: '{branch}'.");

                if ( depots != null )
                {
                    foreach ( var depotSection in depots.Children )
                    {
                        var id = INVALID_DEPOT_ID;
                        if ( depotSection.Children.Count == 0 )
                            continue;

                        if ( !uint.TryParse( depotSection.Name, out id ) )
                            continue;

                        if ( hasSpecificDepots && !depotIdsExpected.Contains( id ) )
                            continue;

                        if ( !hasSpecificDepots )
                        {
                            var depotConfig = depotSection[ "config" ];
                            if ( depotConfig != KeyValue.Invalid )
                            {
                                if ( !Config.DownloadAllPlatforms &&
                                    depotConfig["oslist"] != KeyValue.Invalid &&
                                    !string.IsNullOrWhiteSpace( depotConfig["oslist"].Value ) )
                                {
                                    var oslist = depotConfig["oslist"].Value.Split( ',' );
                                    if ( Array.IndexOf( oslist, os ?? Util.GetSteamOS() ) == -1 )
                                        continue;
                                }

                                if ( depotConfig["osarch"] != KeyValue.Invalid &&
                                    !string.IsNullOrWhiteSpace( depotConfig["osarch"].Value ) )
                                {
                                    var depotArch = depotConfig["osarch"].Value;
                                    if ( depotArch != ( arch ?? Util.GetSteamArch() ) )
                                        continue;
                                }

                                if ( !Config.DownloadAllLanguages &&
                                    depotConfig["language"] != KeyValue.Invalid &&
                                    !string.IsNullOrWhiteSpace( depotConfig["language"].Value ) )
                                {
                                    var depotLang = depotConfig["language"].Value;
                                    if ( depotLang != ( language ?? "english" ) )
                                        continue;
                                }

                                if ( !lv &&
                                    depotConfig["lowviolence"] != KeyValue.Invalid &&
                                    depotConfig["lowviolence"].AsBoolean() )
                                    continue;
                            }
                        }

                        depotIdsFound.Add( id );

                        if ( !hasSpecificDepots )
                            depotManifestIds.Add( ( id, ContentDownloader.INVALID_MANIFEST_ID ) );
                    }
                }
                if ( depotManifestIds.Count == 0 && !hasSpecificDepots )
{
                    throw new ContentDownloaderException(LauncherWindow.launcherInstance.FindResource("item84").ToString().Replace("{0}", $"{appId}"));
                }
                else if ( depotIdsFound.Count < depotIdsExpected.Count )
                {
                    var remainingDepotIds = depotIdsExpected.Except( depotIdsFound );
                    throw new ContentDownloaderException(LauncherWindow.launcherInstance.FindResource("item85").ToString().Replace("{0}", $"'{string.Join(", ", remainingDepotIds).Replace("{1}", $"{appId}")}'"));
                }
            }

            var infos = new List<DepotDownloadInfo>();

            foreach ( var depotManifest in depotManifestIds )
            {
                var info = GetDepotInfo( depotManifest.Item1, appId, depotManifest.Item2, branch );
                if ( info != null )
                {
                    infos.Add( info );
                }
            }

            try
            {
                await DownloadSteam3Async( appId, infos ).ConfigureAwait( false );
            }
            catch ( OperationCanceledException )
            {
                Debug.WriteLine( "App {0} was not completely downloaded.", appId );
                throw;
            }
        }

        static DepotDownloadInfo GetDepotInfo( uint depotId, uint appId, ulong manifestId, string branch )
        {
            if ( steam3 != null && appId != INVALID_APP_ID )
                steam3.RequestAppInfo( ( uint )appId );

            var contentName = GetAppOrDepotName( depotId, appId );

            if ( !AccountHasAccess( depotId ) )
            {
                Debug.WriteLine($"App {depotId} ({contentName}) is not available from this account.");
                return null;
            }

            if (manifestId == INVALID_MANIFEST_ID)
            {
                manifestId = GetSteam3DepotManifest(depotId, appId, branch);
                if (manifestId == INVALID_MANIFEST_ID && branch != "public")
                {
                    Debug.WriteLine("Warning: Depot {0} does not have branch named \"{1}\". Trying public branch.", depotId, branch);
                    branch = "public";
                    manifestId = GetSteam3DepotManifest(depotId, appId, branch);
                }

                if (manifestId == INVALID_MANIFEST_ID)
                {
                    Debug.WriteLine("Depot {0} ({1}) missing public subsection or manifest section.", depotId, contentName);
                    return null;
                }
            }

            // For depots that are proxied through depotfromapp, we still need to resolve the proxy app id
            var containingAppId = appId;
            var proxyAppId = GetSteam3DepotProxyAppId(depotId, appId);
            if (proxyAppId != INVALID_APP_ID) containingAppId = proxyAppId;

            steam3.RequestDepotKey(depotId, appId);
            if (!steam3.DepotKeys.ContainsKey(depotId))
            {
                Debug.WriteLine("No valid depot key for {0}, unable to download.", depotId);
                return null;
            }

            var uVersion = GetSteam3AppBuildNumber(appId, branch);

            string installDir;
            if (!CreateDirectories(depotId, uVersion, out installDir))
            {
                Debug.WriteLine("Error: Unable to create install directories!");
                return null;
            }

            var depotKey = steam3.DepotKeys[ depotId ];

            return new DepotDownloadInfo(depotId, containingAppId, manifestId, branch, installDir, contentName, depotKey);
        }

        private class ChunkMatch
        {
            public ChunkMatch( ProtoManifest.ChunkData oldChunk, ProtoManifest.ChunkData newChunk )
            {
                OldChunk = oldChunk;
                NewChunk = newChunk;
            }
            public ProtoManifest.ChunkData OldChunk { get; private set; }
            public ProtoManifest.ChunkData NewChunk { get; private set; }
        }

        private class DepotFilesData
        {
            public DepotDownloadInfo depotDownloadInfo;
            public DepotDownloadCounter depotCounter;
            public string stagingDir;
            public ProtoManifest manifest;
            public ProtoManifest previousManifest;
            public List<ProtoManifest.FileData> filteredFiles;
            public HashSet<string> allFileNames;
        }

        private class FileStreamData
        {
            public FileStream fileStream;
            public SemaphoreSlim fileLock;
            public int chunksToDownload;
        }

        private class GlobalDownloadCounter
        {
            public ulong TotalBytesCompressed;
            public ulong TotalBytesUncompressed;
        }

        private class DepotDownloadCounter
        {
            public ulong CompleteDownloadSize;
            public ulong SizeDownloaded;
            public ulong DepotBytesCompressed;
            public ulong DepotBytesUncompressed;

        }

        public static CancellationTokenSource tokenSource = new();
        private static async Task DownloadSteam3Async(uint appId, List<DepotDownloadInfo> depots)
        {
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();

            cdnPool.ExhaustedToken = tokenSource;

            GlobalDownloadCounter downloadCounter = new();
            var depotsToDownload = new List<DepotFilesData>(depots.Count);
            var allFileNamesAllDepots = new HashSet<String>();

            // First, fetch all the manifests for each depot (including previous manifests) and perform the initial setup
            foreach (var depot in depots)
            {
                var depotFileData = await ProcessDepotManifestAndFiles(tokenSource, appId, depot, cdnPool);

                if (depotFileData != null)
                {
                    depotsToDownload.Add(depotFileData);
                    allFileNamesAllDepots.UnionWith(depotFileData.allFileNames);
                }

                tokenSource.Token.ThrowIfCancellationRequested();
            }

            // If we're about to write all the files to the same directory, we will need to first de-duplicate any files by path
            // This is in last-depot-wins order, from Steam or the list of depots supplied by the user
            if (!string.IsNullOrWhiteSpace(Config.InstallDirectory) && depotsToDownload.Count > 0)
            {
                var claimedFileNames = new HashSet<String>();

                for (var i = depotsToDownload.Count - 1; i >= 0; i--)
                {
                    // For each depot, remove all files from the list that have been claimed by a later depot
                    depotsToDownload[i].filteredFiles.RemoveAll(file => claimedFileNames.Contains(file.FileName));

                    claimedFileNames.UnionWith(depotsToDownload[i].allFileNames);
                }
            }

            foreach (var depotFileData in depotsToDownload)
            {
                await DownloadSteam3AsyncDepotFiles(tokenSource, appId, downloadCounter, depotFileData, allFileNamesAllDepots);
            }

            Debug.WriteLine("Total downloaded: {0} bytes ({1} bytes uncompressed) from {2} depots",
                downloadCounter.TotalBytesCompressed, downloadCounter.TotalBytesUncompressed, depots.Count);
        }

        private static async Task<DepotFilesData> ProcessDepotManifestAndFiles(CancellationTokenSource cts, 
            uint appId, DepotDownloadInfo depot, CDNClientPool cdnPool)
        {
            var depotCounter = new DepotDownloadCounter();
            var contentName = GetAppOrDepotName(INVALID_DEPOT_ID, appId);

            System.Windows.Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                DownloadStatus.downloadStatusInstance.downloadProgressText.Text = LauncherWindow.launcherInstance.FindResource("item56").ToString().Replace("{0}", $"{depot.id}").Replace("{1}", $"{contentName}");
            }));

            Debug.WriteLine($"Processing depot {depot.id} - {contentName}");

            ProtoManifest oldProtoManifest = null;
            ProtoManifest newProtoManifest = null;
            var configDir = Path.Combine(depot.installDir, CONFIG_DIR);

            var lastManifestId = INVALID_MANIFEST_ID;
            DepotConfigStore.Instance.InstalledManifestIDs.TryGetValue(depot.id, out lastManifestId);

            // In case we have an early exit, this will force equiv of verifyall next run.
            DepotConfigStore.Instance.InstalledManifestIDs[depot.id] = INVALID_MANIFEST_ID;
            DepotConfigStore.Save();

            if (lastManifestId != INVALID_MANIFEST_ID)
            {
                var oldManifestFileName = Path.Combine(configDir, string.Format("{0}_{1}.bin", depot.id, lastManifestId));

                if (File.Exists(oldManifestFileName))
                {
                    byte[] expectedChecksum, currentChecksum;

                    try
                    {
                        expectedChecksum = File.ReadAllBytes(oldManifestFileName + ".sha");
                    }
                    catch (IOException)
                    {
                        expectedChecksum = null;
                    }

                    oldProtoManifest = ProtoManifest.LoadFromFile(oldManifestFileName, out currentChecksum);

                    if (expectedChecksum == null || !expectedChecksum.SequenceEqual(currentChecksum))
                    {
                        // We only have to show this warning if the old manifest ID was different
                        if (lastManifestId != depot.manifestId)
                            Debug.WriteLine("Manifest {0} on disk did not match the expected checksum.", lastManifestId);
                        oldProtoManifest = null;
                    }
                }
            }

            if (lastManifestId == depot.manifestId && oldProtoManifest != null)
            {
                newProtoManifest = oldProtoManifest;
                Debug.WriteLine("Already have manifest {0} for depot {1}.", depot.manifestId, depot.id);
            }
            else
            {
                var newManifestFileName = Path.Combine(configDir, string.Format("{0}_{1}.bin", depot.id, depot.manifestId));
                if (newManifestFileName != null)
                {
                    byte[] expectedChecksum, currentChecksum;

                    try
                    {
                        expectedChecksum = File.ReadAllBytes(newManifestFileName + ".sha");
                    }
                    catch (IOException)
                    {
                        expectedChecksum = null;
                    }

                    newProtoManifest = ProtoManifest.LoadFromFile(newManifestFileName, out currentChecksum);

                    if (newProtoManifest != null && (expectedChecksum == null || !expectedChecksum.SequenceEqual(currentChecksum)))
                    {
                        Debug.WriteLine("Manifest {0} on disk did not match the expected checksum.", depot.manifestId);
                        newProtoManifest = null;
                    }
                }

                if (newProtoManifest != null)
                {
                    Debug.WriteLine("Already have manifest {0} for depot {1}.", depot.manifestId, depot.id);
                }
                else
                {
                    Debug.Write("Downloading depot manifest...");

                    DepotManifest depotManifest = null;
                    ulong manifestRequestCode = 0;
                    var manifestRequestCodeExpiration = DateTime.MinValue;

                    do
                    {
                        cts.Token.ThrowIfCancellationRequested();

                        Server connection = null;

                        try
                        {
                            connection = cdnPool.GetConnection(cts.Token);

                            var now = DateTime.Now;

                            // In order to download this manifest, we need the current manifest request code
                            // The manifest request code is only valid for a specific period in time
                            if (manifestRequestCode == 0 || now >= manifestRequestCodeExpiration)
                            {
                                manifestRequestCode = await steam3.GetDepotManifestRequestCodeAsync(
                                    depot.id,
                                    depot.appId,
                                    depot.manifestId,
                                    depot.branch);
                                // This code will hopefully be valid for one period following the issuing period
                                manifestRequestCodeExpiration = now.Add(TimeSpan.FromMinutes(5));

                                // If we could not get the manifest code, this is a fatal error
                                if (manifestRequestCode == 0)
                                {
                                    Console.WriteLine("No manifest request code was returned for {0} {1}", depot.id, depot.manifestId);
                                    cts.Cancel();
                                }
                            }

                            DebugLog.WriteLine("ContentDownloader",
                                "Downloading manifest {0} from {1} with {2}",
                                depot.manifestId,
                                connection,
                                cdnPool.ProxyServer != null ? cdnPool.ProxyServer : "no proxy");
                            depotManifest = await cdnPool.CDNClient.DownloadManifestAsync(
                                depot.id,
                                depot.manifestId,
                                manifestRequestCode,
                                connection,
                                depot.depotKey,
                                cdnPool.ProxyServer).ConfigureAwait(false);

                            cdnPool.ReturnConnection(connection);
                        }
                        catch (TaskCanceledException)
                        {
                            Console.WriteLine("Connection timeout downloading depot manifest {0} {1}. Retrying.", depot.id, depot.manifestId);
                        }
                        catch (SteamKitWebRequestException e)
                        {
                            cdnPool.ReturnBrokenConnection(connection);

                            if (e.StatusCode == HttpStatusCode.Unauthorized || e.StatusCode == HttpStatusCode.Forbidden)
                            {
                                Console.WriteLine("Encountered 401 for depot manifest {0} {1}. Aborting.", depot.id, depot.manifestId);
                                break;
                            }

                            if (e.StatusCode == HttpStatusCode.NotFound)
                            {
                                Console.WriteLine("Encountered 404 for depot manifest {0} {1}. Aborting.", depot.id, depot.manifestId);
                                break;
                            }

                            Console.WriteLine("Encountered error downloading depot manifest {0} {1}: {2}", depot.id, depot.manifestId, e.StatusCode);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception e)
                        {
                            cdnPool.ReturnBrokenConnection(connection);
                            Console.WriteLine("Encountered error downloading manifest for depot {0} {1}: {2}", depot.id, depot.manifestId, e.Message);
                        }
                    }

                    while (depotManifest == null);

                    if (depotManifest == null)
                    {
                        Debug.WriteLine("\nUnable to download manifest {0} for depot {1}", depot.manifestId, depot.id);
                        cts.Cancel();
                    }

                    // Throw the cancellation exception if requested so that this task is marked failed
                    cts.Token.ThrowIfCancellationRequested();

                    byte[] checksum;

                    newProtoManifest = new ProtoManifest(depotManifest, depot.manifestId);
                    newProtoManifest.SaveToFile(newManifestFileName, out checksum);
                    File.WriteAllBytes(newManifestFileName + ".sha", checksum);

                    Debug.WriteLine(" Done!");
                }
            }

            newProtoManifest.Files.Sort((x, y) => string.Compare(x.FileName, y.FileName, StringComparison.Ordinal));

            System.Windows.Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                DownloadStatus.downloadStatusInstance.downloadProgressText.Text = LauncherWindow.launcherInstance.FindResource("item56").ToString().Replace("{0}", depot.manifestId.ToString());
            }));

            Debug.WriteLine($"Manifest {depot.manifestId} ({newProtoManifest.CreationTime})");

            if (Config.DownloadManifestOnly)
            {
                DumpManifestToTextFile(depot, newProtoManifest);
                return null;
            }

            var stagingDir = Path.Combine(depot.installDir, STAGING_DIR);

            var filesAfterExclusions = newProtoManifest.Files.AsParallel().Where(f => TestIsFileIncluded(f.FileName)).ToList();
            var allFileNames = new HashSet<string>(filesAfterExclusions.Count);

            // Pre-process
            filesAfterExclusions.ForEach(file =>
            {
                allFileNames.Add(file.FileName);

                var fileFinalPath = Path.Combine(depot.installDir, file.FileName);
                var fileStagingPath = Path.Combine(stagingDir, file.FileName);

                if (file.Flags.HasFlag(EDepotFileFlag.Directory))
                {
                    Directory.CreateDirectory(fileFinalPath);
                    Directory.CreateDirectory(fileStagingPath);
                }
                else
                {
                    // Some manifests don't explicitly include all necessary directories
                    Directory.CreateDirectory(Path.GetDirectoryName(fileFinalPath));
                    Directory.CreateDirectory(Path.GetDirectoryName(fileStagingPath));

                    depotCounter.CompleteDownloadSize += file.TotalSize;
                }
            });

            return new DepotFilesData
            {
                depotDownloadInfo = depot,
                depotCounter = depotCounter,
                stagingDir = stagingDir,
                manifest = newProtoManifest,
                previousManifest = oldProtoManifest,
                filteredFiles = filesAfterExclusions,
                allFileNames = allFileNames
            };
        }

        private static async Task DownloadSteam3AsyncDepotFiles(CancellationTokenSource cts, uint appId,
            GlobalDownloadCounter downloadCounter, DepotFilesData depotFilesData, HashSet<String> allFileNamesAllDepots)
        {
            var depot = depotFilesData.depotDownloadInfo;
            var depotCounter = depotFilesData.depotCounter;

            Debug.WriteLine("Downloading depot {0} - {1}", depot.id, depot.contentName);

            var files = depotFilesData.filteredFiles.Where(f => !f.Flags.HasFlag(EDepotFileFlag.Directory)).ToArray();
            var networkChunkQueue = new ConcurrentQueue<(FileStreamData fileStreamData, ProtoManifest.FileData fileData, ProtoManifest.ChunkData chunk)>();

            await Util.InvokeAsync(
                files.Select(file => new Func<Task>(async () =>
                    await Task.Run(() => DownloadSteam3AsyncDepotFile(cts, depotFilesData, file, networkChunkQueue)))),
                maxDegreeOfParallelism: Config.MaxDownloads
            );

            await Util.InvokeAsync(
                networkChunkQueue.Select(q => new Func<Task>(async () =>
                    await Task.Run(() => DownloadSteam3AsyncDepotFileChunk(cts, appId, downloadCounter, depotFilesData,
                        q.fileData, q.fileStreamData, q.chunk)))),
                maxDegreeOfParallelism: Config.MaxDownloads
            );

            // Check for deleted files if updating the depot.
            if (depotFilesData.previousManifest != null)
            {
                var previousFilteredFiles = new HashSet<string>(depotFilesData.previousManifest.Files.AsParallel().Where(f => TestIsFileIncluded(f.FileName)).Select(f => f.FileName).ToHashSet());

                // Check if we are writing to a single output directory. If not, each depot folder is managed independently
                if (string.IsNullOrWhiteSpace(ContentDownloader.Config.InstallDirectory))
                {
                    // Of the list of files in the previous manifest, remove any file names that exist in the current set of all file names
                    previousFilteredFiles.ExceptWith(depotFilesData.allFileNames);
                }
                else
                {
                    // Of the list of files in the previous manifest, remove any file names that exist in the current set of all file names across all depots being downloaded
                    previousFilteredFiles.ExceptWith(allFileNamesAllDepots);
                }

                foreach(var existingFileName in previousFilteredFiles)
                {
                    var fileFinalPath = Path.Combine(depot.installDir, existingFileName);

                    if (!File.Exists(fileFinalPath))
                        continue;

                    File.Delete(fileFinalPath);
                    Debug.WriteLine("Deleted {0}", fileFinalPath);
                }
            }

            DepotConfigStore.Instance.InstalledManifestIDs[depot.id] = depot.manifestId;
            DepotConfigStore.Save();

            Debug.WriteLine("Depot {0} - Downloaded {1} bytes ({2} bytes uncompressed)", depot.id, depotCounter.DepotBytesCompressed, depotCounter.DepotBytesUncompressed);
        }

        private static void DownloadSteam3AsyncDepotFile(
            CancellationTokenSource cts,
            DepotFilesData depotFilesData,
            ProtoManifest.FileData file,
            ConcurrentQueue<(FileStreamData, ProtoManifest.FileData, ProtoManifest.ChunkData)> networkChunkQueue)
        {
            cts.Token.ThrowIfCancellationRequested();

            var depot = depotFilesData.depotDownloadInfo;
            var stagingDir = depotFilesData.stagingDir;
            var depotDownloadCounter = depotFilesData.depotCounter;
            var oldProtoManifest = depotFilesData.previousManifest;
            string singleFileName = string.Empty;

            try
            {
                singleFileName = file.FileName.Substring(file.FileName.LastIndexOf("\\") + 1);
            }
            catch { }

            ProtoManifest.FileData oldManifestFile = null;
            if (oldProtoManifest != null)
            {
                oldManifestFile = oldProtoManifest.Files.SingleOrDefault(f => f.FileName == file.FileName);
            }

            var fileFinalPath = Path.Combine(depot.installDir, file.FileName);
            var fileStagingPath = Path.Combine(stagingDir, file.FileName);

            // This may still exist if the previous run exited before cleanup
            if (File.Exists(fileStagingPath))
            {
                File.Delete(fileStagingPath);
            }

            List<ProtoManifest.ChunkData> neededChunks;
            var fi = new FileInfo(fileFinalPath);
            var fileDidExist = fi.Exists;
            if (!fileDidExist)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    DownloadStatus.downloadStatusInstance.downloadProgressText.Text = $"{LauncherWindow.launcherInstance.FindResource("item57")} {singleFileName}";
                }));

                Debug.WriteLine($"Pre-allocating {fileFinalPath}");

                // create new file. need all chunks
                using var fs = File.Create(fileFinalPath);
                try
                {
                    fs.SetLength((long)file.TotalSize);
                }
                catch (IOException e)
                {
                    throw new ContentDownloaderException(String.Format("Failed to allocate file {0}: {1}", fileFinalPath, e.Message));
                }

                neededChunks = new List<ProtoManifest.ChunkData>(file.Chunks);
            }
            else
            {
                // open existing
                if (oldManifestFile != null)
                {
                    neededChunks = new List<ProtoManifest.ChunkData>();

                    var hashMatches = oldManifestFile.FileHash.SequenceEqual(file.FileHash);
                    if (Config.VerifyAll || !hashMatches)
                    {
                        // we have a version of this file, but it doesn't fully match what we want
                        if (Config.VerifyAll)
                        {
                            Console.WriteLine("Validating {0}", fileFinalPath);
                        }

                        var matchingChunks = new List<ChunkMatch>();

                        foreach (var chunk in file.Chunks)
                        {
                            var oldChunk = oldManifestFile.Chunks.FirstOrDefault(c => c.ChunkID.SequenceEqual(chunk.ChunkID));
                            if (oldChunk != null)
                            {
                                matchingChunks.Add(new ChunkMatch(oldChunk, chunk));
                            }
                            else
                            {
                                neededChunks.Add(chunk);
                            }
                        }

                        var orderedChunks = matchingChunks.OrderBy(x => x.OldChunk.Offset);

                        var copyChunks = new List<ChunkMatch>();

                        using (var fsOld = File.Open(fileFinalPath, FileMode.Open))
                        {
                            foreach (var match in orderedChunks)
                            {
                                fsOld.Seek((long)match.OldChunk.Offset, SeekOrigin.Begin);

                                var tmp = new byte[match.OldChunk.UncompressedLength];
                                fsOld.Read(tmp, 0, tmp.Length);

                                var adler = Util.AdlerHash(tmp);
                                if (!adler.SequenceEqual(match.OldChunk.Checksum))
                                {
                                    neededChunks.Add(match.NewChunk);
                                }
                                else
                                {
                                    copyChunks.Add(match);
                                }
                            }
                        }

                        if (!hashMatches || neededChunks.Count > 0)
                        {
                            File.Move(fileFinalPath, fileStagingPath);

                            using (var fsOld = File.Open(fileStagingPath, FileMode.Open))
                            {
                                using var fs = File.Open(fileFinalPath, FileMode.Create);
                                try
                                {
                                    fs.SetLength((long)file.TotalSize);
                                }
                                catch (IOException e)
                                {
                                    throw new ContentDownloaderException(String.Format("Failed to resize file to expected size {0}: {1}", fileFinalPath, e.Message));
                                }

                                foreach (var match in copyChunks)
                                {
                                    fsOld.Seek((long)match.OldChunk.Offset, SeekOrigin.Begin);

                                    var tmp = new byte[match.OldChunk.UncompressedLength];
                                    fsOld.Read(tmp, 0, tmp.Length);

                                    fs.Seek((long)match.NewChunk.Offset, SeekOrigin.Begin);
                                    fs.Write(tmp, 0, tmp.Length);
                                }
                            }

                            File.Delete(fileStagingPath);
                        }
                    }
                }
                else
                {
                    //No old manifest or file not in old manifest. We must validate.
                    using var fs = File.Open(fileFinalPath, FileMode.Open);
                    if ((ulong)fi.Length != file.TotalSize)
                    {
                        try
                        {
                            fs.SetLength((long)file.TotalSize);
                        }
                        catch (IOException ex)
                        {
                            throw new ContentDownloaderException(String.Format("Failed to allocate file {0}: {1}", fileFinalPath, ex.Message));
                        }
                    }

                    Console.WriteLine("Validating {0}", fileFinalPath);
                    neededChunks = Util.ValidateSteam3FileChecksums(fs, file.Chunks.OrderBy(x => x.Offset).ToArray());
                }

                if (neededChunks.Count() == 0)
                {
                    lock (depotDownloadCounter)
                    {
                        depotDownloadCounter.SizeDownloaded += file.TotalSize;
                        Console.WriteLine("{0,6:#00.00}% {1}", (depotDownloadCounter.SizeDownloaded / (float)depotDownloadCounter.CompleteDownloadSize) * 100.0f, fileFinalPath);
                    }

                    return;
                }

                var sizeOnDisk = (file.TotalSize - (ulong)neededChunks.Select(x => (long)x.UncompressedLength).Sum());
                lock (depotDownloadCounter)
                {
                    depotDownloadCounter.SizeDownloaded += sizeOnDisk;
                }
            }

            var fileIsExecutable = file.Flags.HasFlag(EDepotFileFlag.Executable);
            if (fileIsExecutable && (!fileDidExist || oldManifestFile == null || !oldManifestFile.Flags.HasFlag(EDepotFileFlag.Executable)))
            {
                PlatformUtilities.SetExecutable(fileFinalPath, true);
            }
            else if (!fileIsExecutable && oldManifestFile != null && oldManifestFile.Flags.HasFlag(EDepotFileFlag.Executable))
            {
                PlatformUtilities.SetExecutable(fileFinalPath, false);
            }

            var fileStreamData = new FileStreamData
            {
                fileStream = null,
                fileLock = new SemaphoreSlim(1),
                chunksToDownload = neededChunks.Count
            };

            foreach (var chunk in neededChunks)
            {
                networkChunkQueue.Enqueue((fileStreamData, file, chunk));
            }
        }

        public static System.Timers.Timer downloadSpeedTimer = new System.Timers.Timer(1000);
        public static ulong sizeDownloadedPublic;
        public static float downloadSpeed;
        private static async Task DownloadSteam3AsyncDepotFileChunk(
            CancellationTokenSource cts, uint appId,
            GlobalDownloadCounter downloadCounter,
            DepotFilesData depotFilesData,
            ProtoManifest.FileData file, 
            FileStreamData fileStreamData, 
            ProtoManifest.ChunkData chunk)
        {
            //cts.Token.ThrowIfCancellationRequested();
            if (cts.Token.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }

            var depot = depotFilesData.depotDownloadInfo;
            var depotDownloadCounter = depotFilesData.depotCounter;

            var chunkID = Util.EncodeHexString(chunk.ChunkID);

            var data = new DepotManifest.ChunkData();
            data.ChunkID = chunk.ChunkID;
            data.Checksum = chunk.Checksum;
            data.Offset = chunk.Offset;
            data.CompressedLength = chunk.CompressedLength;
            data.UncompressedLength = chunk.UncompressedLength;

            DepotChunk chunkData = null;

            do
            {
                cts.Token.ThrowIfCancellationRequested();

                Server connection = null;

                try
                {
                    connection = cdnPool.GetConnection(cts.Token);

                    DebugLog.WriteLine("ContentDownloader", "Downloading chunk {0} from {1} with {2}", chunkID, connection, cdnPool.ProxyServer != null ? cdnPool.ProxyServer : "no proxy");
                    chunkData = await cdnPool.CDNClient.DownloadDepotChunkAsync(depot.id, data,
                        connection, depot.depotKey, cdnPool.ProxyServer).ConfigureAwait(false);

                    cdnPool.ReturnConnection(connection);
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine("Connection timeout downloading chunk {0}", chunkID);
                }
                catch (SteamKitWebRequestException e)
                {
                    cdnPool.ReturnBrokenConnection(connection);

                    if (e.StatusCode == HttpStatusCode.Unauthorized || e.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Debug.WriteLine("Encountered 401 for chunk {0}. Aborting.", chunkID);
                        break;
                    }

                    Debug.WriteLine("Encountered error downloading chunk {0}: {1}", chunkID, e.StatusCode);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    cdnPool.ReturnBrokenConnection(connection);
                    Debug.WriteLine("Encountered unexpected error downloading chunk {0}: {1}", chunkID, e.Message);
                }
            }

            while (chunkData == null);

            if (chunkData == null)
            {
                Debug.WriteLine("Failed to find any server with chunk {0} for depot {1}. Aborting.", chunkID, depot.id);
                cts.Cancel();
            }

            // Throw the cancellation exception if requested so that this task is marked failed
            cts.Token.ThrowIfCancellationRequested();

            try
            {
                await fileStreamData.fileLock.WaitAsync().ConfigureAwait(false);

                if (fileStreamData.fileStream == null)
                {
                    var fileFinalPath = Path.Combine(depot.installDir, file.FileName);
                    fileStreamData.fileStream = File.Open(fileFinalPath, FileMode.Open);
                }

                fileStreamData.fileStream.Seek((long)chunkData.ChunkInfo.Offset, SeekOrigin.Begin);
                await fileStreamData.fileStream.WriteAsync(chunkData.Data, 0, chunkData.Data.Length);
            }
            finally
            {
                fileStreamData.fileLock.Release();
            }

            var remainingChunks = Interlocked.Decrement(ref fileStreamData.chunksToDownload);
            if (remainingChunks == 0)
            {
                fileStreamData.fileStream.Dispose();
                fileStreamData.fileLock.Dispose();
            }

            ulong sizeDownloaded = 0;
            lock (depotDownloadCounter)
            {
                sizeDownloaded = depotDownloadCounter.SizeDownloaded + (ulong)chunkData.Data.Length;
                sizeDownloadedPublic = sizeDownloaded;
                depotDownloadCounter.SizeDownloaded = sizeDownloaded;
                depotDownloadCounter.DepotBytesCompressed += chunk.CompressedLength;
                depotDownloadCounter.DepotBytesUncompressed += chunk.UncompressedLength;
            }

            lock (downloadCounter)
            {
                downloadCounter.TotalBytesCompressed += chunk.CompressedLength;
                downloadCounter.TotalBytesUncompressed += chunk.UncompressedLength;
            }

            if (remainingChunks == 0)
            {
                var fileFinalPath = Path.Combine(depot.installDir, file.FileName);
                Debug.WriteLine("{0,6:#00.00}% {1}", sizeDownloaded / depotDownloadCounter.CompleteDownloadSize * 100.0f, fileFinalPath);

                System.Windows.Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    DownloadStatus.downloadStatusInstance.downloadProgress.Value = sizeDownloaded / (float)depotDownloadCounter.CompleteDownloadSize * 100.0f;
                    DownloadStatus.downloadStatusInstance.downloadProgressText.Text = $"{LauncherWindow.launcherInstance.FindResource("item54")} {sizeDownloaded / (float)depotDownloadCounter.CompleteDownloadSize * 100.0f:0.00}% - {downloadSpeed:0.00} MB/s";
                    LauncherWindow.launcherInstance.taskbarIcon.ProgressValue = sizeDownloaded / (float)depotDownloadCounter.CompleteDownloadSize;
                }));
            }
        }

        static void DumpManifestToTextFile(DepotDownloadInfo depot, ProtoManifest manifest)
        {
            var txtManifest = Path.Combine(depot.installDir, $"manifest_{depot.id}_{depot.manifestId}.txt");

            using var sw = new StreamWriter(txtManifest);
            sw.WriteLine($"Content Manifest for Depot {depot.id}");
            sw.WriteLine();
            sw.WriteLine($"Manifest ID / date     : {depot.manifestId} / {manifest.CreationTime}");

            int numFiles = 0, numChunks = 0;
            ulong uncompressedSize = 0, compressedSize = 0;

            foreach (var file in manifest.Files)
            {
                if (file.Flags.HasFlag(EDepotFileFlag.Directory))
                    continue;

                numFiles++;
                numChunks += file.Chunks.Count;

                foreach (var chunk in file.Chunks)
                {
                    uncompressedSize += chunk.UncompressedLength;
                    compressedSize += chunk.CompressedLength;
                }
            }

            sw.WriteLine($"Total number of files  : {numFiles}");
            sw.WriteLine($"Total number of chunks : {numChunks}");
            sw.WriteLine($"Total bytes on disk    : {uncompressedSize}");
            sw.WriteLine($"Total bytes compressed : {compressedSize}");
            sw.WriteLine();
            sw.WriteLine("          Size Chunks File SHA                                 Flags Name");

            foreach (var file in manifest.Files)
            {
                var sha1Hash = BitConverter.ToString(file.FileHash).Replace("-", "");
                sw.WriteLine($"{file.TotalSize,14} {file.Chunks.Count,6} {sha1Hash} {file.Flags,5:D} {file.FileName}");
            }
        }
    }
}