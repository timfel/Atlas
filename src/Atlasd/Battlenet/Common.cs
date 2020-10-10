﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Atlasd.Battlenet
{
    class Common
    {

        public static Dictionary<string, Account> AccountsDb;
        public static Dictionary<string, Account> ActiveAccounts;
        public static Dictionary<string, Channel> ActiveChannels;
        public static List<ClientState> ActiveClients;
        public static IPAddress DefaultInterface { get; private set; }
        public static int DefaultPort { get; private set; }
        public static TcpListener Listener;

        public static void Initialize()
        {
            AccountsDb = new Dictionary<string, Account>(StringComparer.OrdinalIgnoreCase);
            ActiveAccounts = new Dictionary<string, Account>(StringComparer.OrdinalIgnoreCase);
            ActiveChannels = new Dictionary<string, Channel>(StringComparer.OrdinalIgnoreCase);
            ActiveClients = new List<ClientState>();

            ActiveChannels.Append(new KeyValuePair<string, Channel>("The Void", new Channel("The Void", Channel.Flags.Public | Channel.Flags.Silent, -1, "This channel does not have chat privileges.")));
            ActiveChannels.Append(new KeyValuePair<string, Channel>("Backstage", new Channel("Backstage", Channel.Flags.Public | Channel.Flags.Restricted, -1, "Abandon hope, all ye who enter here...")));
            ActiveChannels.Append(new KeyValuePair<string, Channel>("Town Square", new Channel("Town Square", Channel.Flags.Public, 200, "Welcome and enjoy your stay!")));

            DefaultInterface = IPAddress.Any;
            DefaultPort = 6112;

            Listener = new TcpListener(DefaultInterface, DefaultPort);

            Listener.ExclusiveAddressUse = false;
            Listener.Server.NoDelay = true;
            Listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true); // SO_KEEPALIVE
            Listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
        }

        public static string ProtocolTypeName(ProtocolType protocolType)
        {
            return protocolType switch
            {
                ProtocolType.None => "None",
                ProtocolType.Game => "Game",
                ProtocolType.BNFTP => "BNFTP",
                ProtocolType.Chat => "Chat",
                ProtocolType.Chat_Alt1 => "Chat_Alt1",
                ProtocolType.Chat_Alt2 => "Chat_Alt2",
                ProtocolType.IPC => "IPC",
                _ => "Unknown (0x" + ((byte)protocolType).ToString("X2") + ")",
            };
        }
    }
}
