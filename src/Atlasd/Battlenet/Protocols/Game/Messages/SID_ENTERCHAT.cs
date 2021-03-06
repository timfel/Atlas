﻿using Atlasd.Battlenet.Exceptions;
using Atlasd.Daemon;
using System;
using System.IO;
using System.Text;

namespace Atlasd.Battlenet.Protocols.Game.Messages
{
    class SID_ENTERCHAT : Message
    {

        public SID_ENTERCHAT()
        {
            Id = (byte)MessageIds.SID_ENTERCHAT;
            Buffer = new byte[0];
        }

        public SID_ENTERCHAT(byte[] buffer)
        {
            Id = (byte)MessageIds.SID_ENTERCHAT;
            Buffer = buffer;
        }

        public override bool Invoke(MessageContext context)
        {
            switch (context.Direction)
            {
                case MessageDirection.ClientToServer:
                    {
                        Logging.WriteLine(Logging.LogLevel.Debug, Logging.LogType.Client_Game, context.Client.RemoteEndPoint, $"[{Common.DirectionToString(context.Direction)}] SID_ENTERCHAT ({4 + Buffer.Length} bytes)");

                        if (Buffer.Length < 2)
                        {
                            throw new GameProtocolViolationException(context.Client, "SID_ENTERCHAT buffer must be at least 2 bytes");
                        }

                        if (context.Client.GameState.ActiveAccount == null || string.IsNullOrEmpty(context.Client.GameState.OnlineName))
                        {
                            throw new GameProtocolViolationException(context.Client, "SID_ENTERCHAT received before logon");
                        }

                        /**
                         * (STRING) Username
                         * (STRING) Statstring
                         */

                        using var m = new MemoryStream(Buffer);
                        using var r = new BinaryReader(m);

                        var username = r.ReadByteString(); // Defunct
                        var statstring = r.ReadByteString();

                        var productId = (UInt32)context.Client.GameState.Product;

                        // Statstring length is either 0 bytes or 4-128 bytes, not including the null-terminator.
                        if (statstring.Length != 0 && (statstring.Length < 4 || statstring.Length > 128))
                            throw new GameProtocolViolationException(context.Client, "Client sent invalid statstring size in SID_ENTERCHAT");

                        // Add the product id if size = 0
                        if (statstring.Length == 0)
                        {
                            statstring = new byte[4];

                            using var _m = new MemoryStream(statstring);
                            using var _w = new BinaryWriter(_m);

                            _w.Write(productId);
                        }

                        // Verify the product matches
                        if (statstring.Length >= 4)
                        {
                            using var _m = new MemoryStream(statstring);
                            using var _r = new BinaryReader(_m);

                            if (_r.ReadUInt32() != productId)
                                throw new GameProtocolViolationException(context.Client, "Client attempted to set different product id in statstring");
                        }

                        // Use their statstring if Diablo
                        if (Product.IsDiablo(context.Client.GameState.Product))
                        {
                            context.Client.GameState.Statstring = statstring;
                        }
                        else
                        {
                            context.Client.GameState.GenerateStatstring();
                        }

                        return new SID_ENTERCHAT().Invoke(new MessageContext(context.Client, MessageDirection.ServerToClient));
                    }
                case MessageDirection.ServerToClient:
                    {
                        var uniqueName = context.Client.GameState.OnlineName;
                        var statstring = context.Client.GameState.Statstring;
                        var accountName = context.Client.GameState.Username;

                        /**
                         * (STRING) Unique name
                         * (STRING) Statstring
                         * (STRING) Account name
                         */

                        Buffer = new byte[3 + Encoding.UTF8.GetByteCount(uniqueName) + statstring.Length + Encoding.UTF8.GetByteCount(accountName)];

                        using var m = new MemoryStream(Buffer);
                        using var w = new BinaryWriter(m);

                        w.Write((string)uniqueName);
                        w.WriteByteString(statstring);
                        w.Write((string)accountName);

                        lock (context.Client.GameState)
                        {
                            context.Client.GameState.ChannelFlags = (Account.Flags)context.Client.GameState.ActiveAccount.Get(Account.FlagsKey);

                            if (Product.IsUDPSupported(context.Client.GameState.Product)
                                && !context.Client.GameState.UDPSupported)
                            {
                                context.Client.GameState.ChannelFlags |= Account.Flags.NoUDP;
                            }
                        }

                        Logging.WriteLine(Logging.LogLevel.Debug, Logging.LogType.Client_Game, context.Client.RemoteEndPoint, $"[{Common.DirectionToString(context.Direction)}] SID_ENTERCHAT ({4 + Buffer.Length} bytes)");
                        context.Client.Send(ToByteArray(context.Client.ProtocolType));
                        return true;
                    }
            }

            return false;
        }
    }
}
