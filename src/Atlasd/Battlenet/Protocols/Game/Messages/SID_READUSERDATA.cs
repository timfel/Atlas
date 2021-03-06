﻿using Atlasd.Battlenet.Exceptions;
using Atlasd.Daemon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Atlasd.Battlenet.Protocols.Game.Messages
{
    class SID_READUSERDATA : Message
    {

        public SID_READUSERDATA()
        {
            Id = (byte)MessageIds.SID_READUSERDATA;
            Buffer = new byte[0];
        }

        public SID_READUSERDATA(byte[] buffer)
        {
            Id = (byte)MessageIds.SID_READUSERDATA;
            Buffer = buffer;
        }

        private void CollectValues(GameState requester, List<string> accounts, List<string> keys, out List<string> values)
        {
            values = new List<string>();

            for (var i = 0; i < accounts.Count; i++)
            {
                var accountName = accounts[i];
                Account account = null;
                lock (Battlenet.Common.AccountsDb) Battlenet.Common.AccountsDb.TryGetValue(accountName, out account);

                if (account == null)
                {
                    for (var j = 0; j < keys.Count; j++)
                    {
                        values.Add("");
                    }

                    continue;
                }

                foreach (var reqKey in keys)
                {
                    account.Get(reqKey, out var kv);

                    if (kv == null)
                    {
                        values.Add("");
                        continue;
                    }

                    if (kv.Readable != AccountKeyValue.ReadLevel.Any)
                    {
                        if (!(kv.Readable == AccountKeyValue.ReadLevel.Owner && account == requester.ActiveAccount))
                        {
                            // No permission
                            values.Add("");
                            continue;
                        }
                    }

                    try
                    {
                        if (kv.Value is string)
                        {
                            values.Add(kv.Value);
                        }
                        else if (kv.Value is long @long)
                        {
                            values.Add(@long.ToString());
                        }
                        else if (kv.Value is ulong @ulong)
                        {
                            values.Add(@ulong.ToString());
                        }
                        else if (kv.Value is int @int)
                        {
                            values.Add(@int.ToString());
                        }
                        else if (kv.Value is uint @uint)
                        {
                            values.Add(@uint.ToString());
                        }
                        else if (kv.Value is short @short)
                        {
                            values.Add(@short.ToString());
                        }
                        else if (kv.Value is ushort @ushort)
                        {
                            values.Add(@ushort.ToString());
                        }
                        else if (kv.Value is byte @byte)
                        {
                            values.Add(@byte.ToString());
                        }
                        else if (kv.Value is bool @bool)
                        {
                            values.Add(@bool ? "1" : "0");
                        }
                        else if (kv.Value is DateTime @dateTime)
                        {
                            var _value = dateTime.ToFileTime();
                            var high = (uint)(_value >> 32);
                            var low = (uint)_value;
                            values.Add(high.ToString() + " " + low.ToString());
                        } else
                        {
                            values.Add("");
                        }
                    }
                    catch (Exception)
                    {
                        values.Add("");
                    }
                }
            }
        }

        public override bool Invoke(MessageContext context)
        {
            switch (context.Direction)
            {
                case MessageDirection.ClientToServer:
                    {
                        Logging.WriteLine(Logging.LogLevel.Debug, Logging.LogType.Client_Game, context.Client.RemoteEndPoint, $"[{Common.DirectionToString(context.Direction)}] SID_READUSERDATA ({4 + Buffer.Length} bytes)");

                        /**
                         * (UINT32)   Number of Accounts
                         * (UINT32)   Number of Keys
                         * (UINT32)   Request ID
                         * (STRING)[] Requested Accounts
                         * (STRING)[] Requested Keys
                         */

                        if (Buffer.Length < 12)
                            throw new GameProtocolViolationException(context.Client, "SID_READUSERDATA buffer must be at least 12 bytes");

                        if (context.Client.GameState == null || context.Client.GameState.Version == null || context.Client.GameState.Version.VersionByte == 0)
                            throw new GameProtocolViolationException(context.Client, "SID_READUSERDATA cannot be processed without an active version");

                        using var m = new MemoryStream(Buffer);
                        using var r = new BinaryReader(m);

                        var numAccounts = r.ReadUInt32();
                        var numKeys = r.ReadUInt32();
                        var requestId = r.ReadUInt32();

                        var accounts = new List<string>();
                        var keys = new List<string>();

                        for (var i = 0; i < numAccounts; i++)
                            accounts.Add(r.ReadString());

                        for (var i = 0; i < numKeys; i++)
                            keys.Add(r.ReadString());

                        if (numAccounts > 1)
                        {
                            accounts = new List<string>();
                            keys = new List<string>();
                        }

                        if (numKeys > 31)
                        {
                            throw new GameProtocolViolationException(context.Client, "SID_READUSERDATA must request no more than 31 keys");
                        }

                        return new SID_READUSERDATA().Invoke(new MessageContext(context.Client, MessageDirection.ServerToClient, new Dictionary<string, object> {
                            { "requestId", requestId },
                            { "accounts", accounts },
                            { "keys", keys },
                        }));
                    }
                case MessageDirection.ServerToClient:
                    {
                        /**
                         * (UINT32)    Number of accounts
                         * (UINT32)    Number of keys
                         * (UINT32)    Request ID
                         * (STRING) [] Requested Key Values
                         */

                        var accounts = (List<string>)context.Arguments["accounts"];
                        var keys = (List<string>)context.Arguments["keys"];
                        CollectValues(context.Client.GameState, accounts, keys, out var values);

                        var size = 12;
                        foreach (var value in values)
                        {
                            size += 1 + Encoding.UTF8.GetByteCount(value);
                        }

                        Buffer = new byte[size];

                        using var m = new MemoryStream(Buffer);
                        using var w = new BinaryWriter(m);

                        w.Write((UInt32)accounts.Count);
                        w.Write((UInt32)keys.Count);
                        w.Write((UInt32)context.Arguments["requestId"]);

                        foreach (var value in values)
                        {
                            w.Write((string)value);
                        }

                        Logging.WriteLine(Logging.LogLevel.Debug, Logging.LogType.Client_Game, context.Client.RemoteEndPoint, $"[{Common.DirectionToString(context.Direction)}] SID_READUSERDATA ({4 + Buffer.Length} bytes)");
                        context.Client.Send(ToByteArray(context.Client.ProtocolType));
                        return true;
                    }
            }

            return false;
        }
    }
}
