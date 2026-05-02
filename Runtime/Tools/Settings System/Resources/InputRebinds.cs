using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlueMuffinGames.Tools.SettingsSystem.Rebinding
{
    [Serializable]
    public class InputRebinds
    {
        public Dictionary<int, string> rebinds = new();

        public static InputRebinds Deserialize(string json)
        {
            var result = JsonConvert.DeserializeObject<InputRebinds>(json);

            return result != null ? result : new InputRebinds();
        }

    }
}
