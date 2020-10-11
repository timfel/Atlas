﻿using Atlasd.Battlenet.Exceptions;
using Atlasd.Daemon;
using System;
using System.IO;

namespace Atlasd.Battlenet.Protocols.Game.Messages
{
    class SID_CHATEVENT : Message
    {
        public SID_CHATEVENT()
        {
            Id = (byte)MessageIds.SID_CHATEVENT;
            Buffer = new byte[26];
        }

        public SID_CHATEVENT(byte[] buffer)
        {
            Id = (byte)MessageIds.SID_CHATEVENT;
            Buffer = buffer;
        }

        public override bool Invoke(MessageContext context)
        {
            if (context.Direction == MessageDirection.ClientToServer)
                throw new ProtocolViolationException(ProtocolType.Game, "Client is not allowed to send SID_CHATEVENT");

            Buffer = ((ChatEvent)context.Arguments["chatEvent"]).ToByteArray();

            Logging.WriteLine(Logging.LogLevel.Debug, Logging.LogType.Client_Game, context.Client.RemoteEndPoint, "[" + Common.DirectionToString(context.Direction) + "] SID_CHATEVENT (" + (4 + Buffer.Length) + " bytes)");

            return true;
        }
    }
}
