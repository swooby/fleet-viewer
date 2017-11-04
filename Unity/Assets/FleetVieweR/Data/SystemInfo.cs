using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FleetVieweR
{
    public class SystemInfo
    {
        public const string FIELD_NAME = "Name";
        public const string FIELD_CONFIG_PATH = "Config Path";
        public const string FIELD_SHEET_ID = "Sheet ID";

        public string Name { get; private set; }
        public string ConfigPath { get; private set; }
        public string SheetId { get; private set; }

        public SystemInfo(Dictionary<string, string> dictionary)
        {
            Name = dictionary[FIELD_NAME];
            ConfigPath = dictionary[FIELD_CONFIG_PATH];
            SheetId = dictionary[FIELD_SHEET_ID];
        }
    }
}