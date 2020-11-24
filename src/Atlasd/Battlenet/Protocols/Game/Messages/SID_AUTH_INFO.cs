﻿using Atlasd.Daemon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Atlasd.Battlenet.Protocols.Game.Messages
{
    class SID_AUTH_INFO : Message
    {
        public SID_AUTH_INFO()
        {
            Id = (byte)MessageIds.SID_AUTH_INFO;
            Buffer = new byte[0];
        }

        public SID_AUTH_INFO(byte[] buffer)
        {
            Id = (byte)MessageIds.SID_AUTH_INFO;
            Buffer = buffer;
        }

        public override bool Invoke(MessageContext context)
        {
            switch (context.Direction)
            {
                case MessageDirection.ClientToServer:
                    {
                        Logging.WriteLine(Logging.LogLevel.Debug, Logging.LogType.Client_Game, context.Client.RemoteEndPoint, $"[{Common.DirectionToString(context.Direction)}] SID_AUTH_INFO ({4 + Buffer.Length} bytes)");

                        if (Buffer.Length < 38)
                            throw new Exceptions.GameProtocolViolationException(context.Client, "SID_AUTH_INFO must be at least 38 bytes");
                        /**
                         * (UINT32) Protocol ID
                         * (UINT32) Platform code
                         * (UINT32) Product code
                         * (UINT32) Version byte
                         * (UINT32) Language code
                         * (UINT32) Local IP
                         * (UINT32) Time zone bias
                         * (UINT32) MPQ locale ID
                         * (UINT32) User language ID
                         * (STRING) Country abbreviation
                         * (STRING) Country
                         */

                        using var m = new MemoryStream(Buffer);
                        using var r = new BinaryReader(m);

                        context.Client.GameState.ProtocolId = r.ReadUInt32();
                        context.Client.GameState.Platform = (Platform.PlatformCode)r.ReadUInt32();
                        context.Client.GameState.Product = (Product.ProductCode)r.ReadUInt32();
                        context.Client.GameState.Version.VersionByte = r.ReadUInt32();
                        context.Client.GameState.Locale.LanguageCode = r.ReadUInt32();
                        context.Client.GameState.LocalIPAddress = IPAddress.Parse(r.ReadUInt32().ToString());
                        context.Client.GameState.TimezoneBias = r.ReadInt32();
                        context.Client.GameState.Locale.UserLocaleId = r.ReadUInt32();
                        context.Client.GameState.Locale.UserLanguageId = r.ReadUInt32();
                        context.Client.GameState.Locale.CountryNameAbbreviated = r.ReadString();
                        context.Client.GameState.Locale.CountryName = r.ReadString();

                        return new SID_PING().Invoke(new MessageContext(context.Client, MessageDirection.ServerToClient, new Dictionary<string, dynamic>(){{ "token", context.Client.GameState.PingToken }}))
                            && new SID_AUTH_INFO().Invoke(new MessageContext(context.Client, MessageDirection.ServerToClient));
                    }
                case MessageDirection.ServerToClient:
                    {
                        /**
                         *    (UINT32) Logon type
                         *    (UINT32) Server token
                         *    (UINT32) UDP value
                         *  (FILETIME) CheckRevision MPQ filetime
                         *    (STRING) CheckRevision MPQ filename
                         *    (STRING) CheckRevision Formula
                         *
                         *  WAR3/W3XP Only:
                         *      (VOID) 128-byte Server signature
                         */

                        ulong MPQFiletime = 0;
                        string MPQFilename = "ver-IX86-1.mpq";
                        byte[] Formula;

                        if (Product.IsWarcraftII(context.Client.GameState.Product))
                        {
                            MPQFilename = "lockdown-IX86-00.mpq";
                            Formula = new byte[] { 0x07, 0x0C, 0xB5, 0x34, 0x31, 0x8A, 0xC3, 0x61, 0xD0, 0x7D, 0x40, 0x74, 0xB5, 0xD2, 0x75, 0x0B };
                        }
                        else
                        {
                            Formula = Encoding.UTF8.GetBytes("A=3845581634 B=880823580 C=1363937103 4 A=A-S B=B-C C=C-A A=A-B");
                        }

                        var fileinfo = new BNFTP.File(MPQFilename).GetFileInfo();
                        if (fileinfo == null)
                        {
                            Logging.WriteLine(Logging.LogLevel.Error, Logging.LogType.Client_Game, $"Version check file [{MPQFilename}] does not exist!");
                        }
                        else
                        {
                            MPQFilename = fileinfo.Name;
                            MPQFiletime = (ulong)fileinfo.LastWriteTimeUtc.ToFileTimeUtc();
                        }

                        Buffer = new byte[22 + MPQFilename.Length + Formula.Length + (Product.IsWarcraftIII(context.Client.GameState.Product) ? 128 : 0)];

                        using var m = new MemoryStream(Buffer);
                        using var w = new BinaryWriter(m);

                        w.Write((UInt32)context.Client.GameState.LogonType);
                        w.Write((UInt32)context.Client.GameState.ServerToken);
                        w.Write((UInt32)context.Client.GameState.UDPToken);
                        w.Write((UInt64)MPQFiletime);
                        w.Write((string)MPQFilename);
                        w.Write(Formula);
                        w.Write((byte)0);

                        if (Product.IsWarcraftIII(context.Client.GameState.Product))
                            w.Write(new byte[128]);

                        context.Client.GameState.SetLocale();

                        Logging.WriteLine(Logging.LogLevel.Debug, Logging.LogType.Client_Game, context.Client.RemoteEndPoint, $"[{Common.DirectionToString(context.Direction)}] SID_AUTH_INFO ({4 + Buffer.Length} bytes)");
                        context.Client.Send(ToByteArray(context.Client.ProtocolType));
                        return true;
                    }
            }

            return false;
        }
    }
}
