// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: network_connection.proto
// </auto-generated>

#region Designer generated code
#pragma warning disable CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
namespace SteamKit2.GC.Dota.Internal
{

    [global::ProtoBuf.ProtoContract()]
    public enum ENetworkDisconnectionReason
    {
        NETWORK_DISCONNECT_INVALID = 0,
        NETWORK_DISCONNECT_SHUTDOWN = 1,
        NETWORK_DISCONNECT_DISCONNECT_BY_USER = 2,
        NETWORK_DISCONNECT_DISCONNECT_BY_SERVER = 3,
        NETWORK_DISCONNECT_LOST = 4,
        NETWORK_DISCONNECT_OVERFLOW = 5,
        NETWORK_DISCONNECT_STEAM_BANNED = 6,
        NETWORK_DISCONNECT_STEAM_INUSE = 7,
        NETWORK_DISCONNECT_STEAM_TICKET = 8,
        NETWORK_DISCONNECT_STEAM_LOGON = 9,
        NETWORK_DISCONNECT_STEAM_AUTHCANCELLED = 10,
        NETWORK_DISCONNECT_STEAM_AUTHALREADYUSED = 11,
        NETWORK_DISCONNECT_STEAM_AUTHINVALID = 12,
        NETWORK_DISCONNECT_STEAM_VACBANSTATE = 13,
        NETWORK_DISCONNECT_STEAM_LOGGED_IN_ELSEWHERE = 14,
        NETWORK_DISCONNECT_STEAM_VAC_CHECK_TIMEDOUT = 15,
        NETWORK_DISCONNECT_STEAM_DROPPED = 16,
        NETWORK_DISCONNECT_STEAM_OWNERSHIP = 17,
        NETWORK_DISCONNECT_SERVERINFO_OVERFLOW = 18,
        NETWORK_DISCONNECT_TICKMSG_OVERFLOW = 19,
        NETWORK_DISCONNECT_STRINGTABLEMSG_OVERFLOW = 20,
        NETWORK_DISCONNECT_DELTAENTMSG_OVERFLOW = 21,
        NETWORK_DISCONNECT_TEMPENTMSG_OVERFLOW = 22,
        NETWORK_DISCONNECT_SOUNDSMSG_OVERFLOW = 23,
        NETWORK_DISCONNECT_SNAPSHOTOVERFLOW = 24,
        NETWORK_DISCONNECT_SNAPSHOTERROR = 25,
        NETWORK_DISCONNECT_RELIABLEOVERFLOW = 26,
        NETWORK_DISCONNECT_BADDELTATICK = 27,
        NETWORK_DISCONNECT_NOMORESPLITS = 28,
        NETWORK_DISCONNECT_TIMEDOUT = 29,
        NETWORK_DISCONNECT_DISCONNECTED = 30,
        NETWORK_DISCONNECT_LEAVINGSPLIT = 31,
        NETWORK_DISCONNECT_DIFFERENTCLASSTABLES = 32,
        NETWORK_DISCONNECT_BADRELAYPASSWORD = 33,
        NETWORK_DISCONNECT_BADSPECTATORPASSWORD = 34,
        NETWORK_DISCONNECT_HLTVRESTRICTED = 35,
        NETWORK_DISCONNECT_NOSPECTATORS = 36,
        NETWORK_DISCONNECT_HLTVUNAVAILABLE = 37,
        NETWORK_DISCONNECT_HLTVSTOP = 38,
        NETWORK_DISCONNECT_KICKED = 39,
        NETWORK_DISCONNECT_BANADDED = 40,
        NETWORK_DISCONNECT_KICKBANADDED = 41,
        NETWORK_DISCONNECT_HLTVDIRECT = 42,
        NETWORK_DISCONNECT_PURESERVER_CLIENTEXTRA = 43,
        NETWORK_DISCONNECT_PURESERVER_MISMATCH = 44,
        NETWORK_DISCONNECT_USERCMD = 45,
        NETWORK_DISCONNECT_REJECTED_BY_GAME = 46,
        NETWORK_DISCONNECT_MESSAGE_PARSE_ERROR = 47,
        NETWORK_DISCONNECT_INVALID_MESSAGE_ERROR = 48,
        NETWORK_DISCONNECT_BAD_SERVER_PASSWORD = 49,
        NETWORK_DISCONNECT_DIRECT_CONNECT_RESERVATION = 50,
        NETWORK_DISCONNECT_CONNECTION_FAILURE = 51,
        NETWORK_DISCONNECT_NO_PEER_GROUP_HANDLERS = 52,
        NETWORK_DISCONNECT_RECONNECTION = 53,
        NETWORK_DISCONNECT_LOOPSHUTDOWN = 54,
        NETWORK_DISCONNECT_LOOPDEACTIVATE = 55,
        NETWORK_DISCONNECT_HOST_ENDGAME = 56,
        NETWORK_DISCONNECT_LOOP_LEVELLOAD_ACTIVATE = 57,
        NETWORK_DISCONNECT_CREATE_SERVER_FAILED = 58,
        NETWORK_DISCONNECT_EXITING = 59,
        NETWORK_DISCONNECT_REQUEST_HOSTSTATE_IDLE = 60,
        NETWORK_DISCONNECT_REQUEST_HOSTSTATE_HLTVRELAY = 61,
        NETWORK_DISCONNECT_CLIENT_CONSISTENCY_FAIL = 62,
        NETWORK_DISCONNECT_CLIENT_UNABLE_TO_CRC_MAP = 63,
        NETWORK_DISCONNECT_CLIENT_NO_MAP = 64,
        NETWORK_DISCONNECT_CLIENT_DIFFERENT_MAP = 65,
        NETWORK_DISCONNECT_SERVER_REQUIRES_STEAM = 66,
        NETWORK_DISCONNECT_STEAM_DENY_MISC = 67,
        NETWORK_DISCONNECT_STEAM_DENY_BAD_ANTI_CHEAT = 68,
        NETWORK_DISCONNECT_SERVER_SHUTDOWN = 69,
        NETWORK_DISCONNECT_REPLAY_INCOMPATIBLE = 71,
        NETWORK_DISCONNECT_CONNECT_REQUEST_TIMEDOUT = 72,
        NETWORK_DISCONNECT_SERVER_INCOMPATIBLE = 73,
        NETWORK_DISCONNECT_LOCALPROBLEM_MANYRELAYS = 74,
        NETWORK_DISCONNECT_LOCALPROBLEM_HOSTEDSERVERPRIMARYRELAY = 75,
        NETWORK_DISCONNECT_LOCALPROBLEM_NETWORKCONFIG = 76,
        NETWORK_DISCONNECT_LOCALPROBLEM_OTHER = 77,
        NETWORK_DISCONNECT_REMOTE_TIMEOUT = 79,
        NETWORK_DISCONNECT_REMOTE_TIMEOUT_CONNECTING = 80,
        NETWORK_DISCONNECT_REMOTE_OTHER = 81,
        NETWORK_DISCONNECT_REMOTE_BADCRYPT = 82,
        NETWORK_DISCONNECT_REMOTE_CERTNOTTRUSTED = 83,
        NETWORK_DISCONNECT_UNUSUAL = 84,
        NETWORK_DISCONNECT_INTERNAL_ERROR = 85,
        NETWORK_DISCONNECT_REJECT_BADCHALLENGE = 128,
        NETWORK_DISCONNECT_REJECT_NOLOBBY = 129,
        NETWORK_DISCONNECT_REJECT_BACKGROUND_MAP = 130,
        NETWORK_DISCONNECT_REJECT_SINGLE_PLAYER = 131,
        NETWORK_DISCONNECT_REJECT_HIDDEN_GAME = 132,
        NETWORK_DISCONNECT_REJECT_LANRESTRICT = 133,
        NETWORK_DISCONNECT_REJECT_BADPASSWORD = 134,
        NETWORK_DISCONNECT_REJECT_SERVERFULL = 135,
        NETWORK_DISCONNECT_REJECT_INVALIDRESERVATION = 136,
        NETWORK_DISCONNECT_REJECT_FAILEDCHANNEL = 137,
        NETWORK_DISCONNECT_REJECT_CONNECT_FROM_LOBBY = 138,
        NETWORK_DISCONNECT_REJECT_RESERVED_FOR_LOBBY = 139,
        NETWORK_DISCONNECT_REJECT_INVALIDKEYLENGTH = 140,
        NETWORK_DISCONNECT_REJECT_OLDPROTOCOL = 141,
        NETWORK_DISCONNECT_REJECT_NEWPROTOCOL = 142,
        NETWORK_DISCONNECT_REJECT_INVALIDCONNECTION = 143,
        NETWORK_DISCONNECT_REJECT_INVALIDCERTLEN = 144,
        NETWORK_DISCONNECT_REJECT_INVALIDSTEAMCERTLEN = 145,
        NETWORK_DISCONNECT_REJECT_STEAM = 146,
        NETWORK_DISCONNECT_REJECT_SERVERAUTHDISABLED = 147,
        NETWORK_DISCONNECT_REJECT_SERVERCDKEYAUTHINVALID = 148,
        NETWORK_DISCONNECT_REJECT_BANNED = 149,
        NETWORK_DISCONNECT_KICKED_TEAMKILLING = 150,
        NETWORK_DISCONNECT_KICKED_TK_START = 151,
        NETWORK_DISCONNECT_KICKED_UNTRUSTEDACCOUNT = 152,
        NETWORK_DISCONNECT_KICKED_CONVICTEDACCOUNT = 153,
        NETWORK_DISCONNECT_KICKED_COMPETITIVECOOLDOWN = 154,
        NETWORK_DISCONNECT_KICKED_TEAMHURTING = 155,
        NETWORK_DISCONNECT_KICKED_HOSTAGEKILLING = 156,
        NETWORK_DISCONNECT_KICKED_VOTEDOFF = 157,
        NETWORK_DISCONNECT_KICKED_IDLE = 158,
        NETWORK_DISCONNECT_KICKED_SUICIDE = 159,
        NETWORK_DISCONNECT_KICKED_NOSTEAMLOGIN = 160,
        NETWORK_DISCONNECT_KICKED_NOSTEAMTICKET = 161,
        NETWORK_DISCONNECT_KICKED_INPUTAUTOMATION = 162,
    }

}

#pragma warning restore CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
#endregion