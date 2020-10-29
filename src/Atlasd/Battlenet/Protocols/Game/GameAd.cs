﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Atlasd.Battlenet.Protocols.Game
{
    class GameAd
    {
        public enum StateFlags : UInt32
        {
            None = 0x00,
            Private = 0x01,
            Full = 0x02,
            HasPlayers = 0x04,
            InProgress = 0x08,
            DisconnectIsLoss = 0x10,
            Replay = 0x80,
        };

        public enum GameTypes : UInt16
        {
            Melee = 0x02,
            FreeForAll = 0x03,
            FFA = FreeForAll,
            OneVsOne = 0x04,
            CaptureTheFlag = 0x05,
            CTF = 0x05,
            Greed = 0x06,
            Slaughter = 0x07,
            SuddenDeath = 0x08,
            Ladder = 0x09,
            IronManLadder = 0x10,
            UseMapSettings = 0x0A,
            UMS = UseMapSettings,
            TeamMelee = 0x0B,
            TeamFreeForAll = 0x0C,
            TeamFFA = TeamFreeForAll,
            TeamCaptureTheFlag = 0x0D,
            TeamCTF = TeamCaptureTheFlag,
            TopVsBottom = 0x0F,
            PGL = 0x20,
        };

        public enum LadderTypes : UInt32
        {
            None = 0x00,
            Ladder = 0x01,
            IronManLadder = 0x03,
        };

        public StateFlags ActiveStateFlags { get; private set; }
        public UInt32 ElapsedTime { get; private set; }
        public UInt32 GamePort { get; private set; }
        public GameTypes GameType { get; private set; }
        public UInt32 GameVersion { get; private set; }
        public string Name { get; private set; }
        public string Password { get; private set; }
        public string Statstring { get; private set; }

        public GameAd(string name, string password, string statstring, UInt32 gamePort, GameTypes gameType, UInt32 gameVersion)
        {
            ActiveStateFlags = StateFlags.None;
            ElapsedTime = 0;
            GamePort = gamePort;
            GameType = gameType;
            GameVersion = gameVersion;
            Name = name;
            Password = password;
            Statstring = statstring;
        }

        public void SetActiveStateFlags(StateFlags newFlags)
        {
            ActiveStateFlags = newFlags;
        }

        public void SetElapsedTime(UInt32 newElapsedTime)
        {
            ElapsedTime = newElapsedTime;
        }

        public void SetGameType(GameTypes newGameType)
        {
            GameType = newGameType;
        }

        public void SetGameVersion(UInt32 newGameVersion)
        {
            GameVersion = newGameVersion;
        }

        public void SetName(string newName)
        {
            Name = newName;
        }

        public void SetPassword(string newPassword)
        {
            Password = newPassword;
        }

        public void SetStatstring(string newStatstring)
        {
            Statstring = newStatstring;
        }
    };
}
