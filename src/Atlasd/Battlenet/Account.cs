﻿using System;
using System.Collections.Generic;

namespace Atlasd.Battlenet
{
    class Account
    {
        public const string AccountCreatedKey = "System\\Account Created";
        public const string FailedLogonsKey = "System\\Total Failed Logons";
        public const string FlagsKey = "System\\Flags";
        public const string FriendsKey = "System\\Friends";
        public const string IPAddressKey = "System\\IP";
        public const string LastLogoffKey = "System\\Last Logoff";
        public const string LastLogonKey = "System\\Last Logon";
        public const string PasswordKey = "System\\Password Digest";
        public const string PortKey = "System\\Port";
        public const string ProfileAgeKey = "profile\\age";
        public const string ProfileDescriptionKey = "profile\\description";
        public const string ProfileLocationKey = "profile\\location";
        public const string ProfileSexKey = "profile\\sex";
        public const string TimeLoggedKey = "System\\Time Logged";
        public const string UsernameKey = "System\\Username";

        public enum Flags : UInt32
        {
            None = 0x00,
            Employee = 0x01,
            ChannelOp = 0x02,
            Speaker = 0x04,
            Admin = 0x08,
            NoUDP = 0x10,
            Squelched = 0x20,
            Guest = 0x40,
            Closed = 0x80,
        };

        public Dictionary<string, dynamic> Userdata { get; protected set; }

        public Account()
        {
            Userdata = new Dictionary<string, dynamic>();
        }

        public bool ContainsKey(string key)
        {
            return Userdata.ContainsKey(key);
        }

        public dynamic Get(string key, dynamic defaultValue = null)
        {
            if (!Userdata.TryGetValue(key, out dynamic value))
                return defaultValue;

            return value ?? defaultValue;
        }

        public void Set(string key, dynamic value)
        {
            Userdata[key] = value;
        }
    }
}
