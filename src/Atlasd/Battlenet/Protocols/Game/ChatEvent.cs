﻿using System;
using System.IO;
using System.Text;

namespace Atlasd.Battlenet.Protocols.Game
{
    class ChatEvent
    {
        public enum EventIds : UInt32
        {
            EID_USERSHOW = 0x01,
            EID_USERJOIN = 0x02,
            EID_USERLEAVE = 0x03,
            EID_WHISPERFROM = 0x04,
            EID_TALK = 0x05,
            EID_BROADCAST = 0x06,
            EID_CHANNELJOIN = 0x07,
            EID_USERUPDATE = 0x09,
            EID_WHISPERTO = 0x0A,
            EID_CHANNELFULL = 0x0D,
            EID_CHANNELNOTFOUND = 0x0E,
            EID_CHANNELRESTRICTED = 0x0F,
            EID_INFO = 0x12,
            EID_ERROR = 0x13,
            EID_EMOTE = 0x17,
        };

        public EventIds EventId { get; protected set; }
        public UInt32 Flags { get; protected set; }
        public Int32 Ping { get; protected set; }
        public string Username { get; protected set; }
        public string Text { get; protected set; }

        public ChatEvent(EventIds eventId, UInt32 flags, Int32 ping, string username, string text)
        {
            Initialize(eventId, flags, ping, username, text);
        }

        public ChatEvent(EventIds eventId, Account.Flags flags, Int32 ping, string username, string text)
        {
            Initialize(eventId, (UInt32)flags, ping, username, text);
        }

        public ChatEvent(EventIds eventId, Channel.Flags flags, Int32 ping, string username, string text)
        {
            Initialize(eventId, (UInt32)flags, ping, username, text);
        }

        public static string EventIdToString(EventIds eventId)
        {
            return eventId switch {
                EventIds.EID_USERSHOW => "EID_USERSHOW",
                EventIds.EID_USERJOIN => "EID_USERJOIN",
                EventIds.EID_USERLEAVE => "EID_USERLEAVE",
                EventIds.EID_WHISPERFROM => "EID_WHISPERFROM",
                EventIds.EID_TALK => "EID_TALK",
                EventIds.EID_BROADCAST => "EID_BROADCAST",
                EventIds.EID_CHANNELJOIN => "EID_CHANNELJOIN",
                EventIds.EID_USERUPDATE => "EID_USERUPDATE",
                EventIds.EID_WHISPERTO => "EID_WHISPERTO",
                EventIds.EID_CHANNELFULL => "EID_CHANNELFULL",
                EventIds.EID_CHANNELNOTFOUND => "EID_CHANNELNOTFOUND",
                EventIds.EID_CHANNELRESTRICTED => "EID_CHANNELRESTRICTED",
                EventIds.EID_INFO => "EID_INFO",
                EventIds.EID_ERROR => "EID_ERROR",
                EventIds.EID_EMOTE => "EID_EMOTE",
                _ => throw new ArgumentOutOfRangeException(string.Format("Unknown Event Id [0x{0:X8}]", eventId)),
            };
        }

        protected void Initialize(EventIds eventId, UInt32 flags, Int32 ping, string username, string text)
        {
            EventId = eventId;
            Flags = flags;
            Ping = ping;
            Username = username;
            Text = text;
        }

        public byte[] ToByteArray()
        {
            var buf = new byte[26 + Encoding.ASCII.GetByteCount(Username) + Encoding.ASCII.GetByteCount(Text)];
            var m = new MemoryStream(buf);
            var w = new BinaryWriter(m);

            w.Write((UInt32)EventId);
            w.Write((UInt32)Flags);
            w.Write((Int32)Ping);
            w.Write((UInt32)0); // IP address (Defunct)
            w.Write((UInt32)0xBAADF00D); // Account number (Defunct)
            w.Write((UInt32)0xDEADBEEF); // Registration authority (Defunct)
            w.Write((string)Username);
            w.Write((string)Text);

            w.Close();
            m.Close();

            return buf;
        }
    }
}
