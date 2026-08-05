using BlueMuffinGames.Utility.Registry;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlueMuffinGames.Tools.SettingsSystem
{
    [CreateAssetMenu(fileName = "SettingsRegistry", menuName = "Scriptable Objects/Settings System/SettingsRegistry")]
    public class SettingsRegistry : BaseRegistry
    {
        [SerializeField] private List<BaseSettingDefinition> _settingDefinitions = new();

        public IReadOnlyList<BaseSettingDefinition> SettingDefinitions => _settingDefinitions;

        protected override void ProcessFolders(string[] folderPaths, out bool changed)
        {
            var newSettingDefinitions = FindScriptableObjects<BaseSettingDefinition>(folderPaths);

            changed = !newSettingDefinitions.SequenceEqual(_settingDefinitions);

            _settingDefinitions = newSettingDefinitions;
        }
    }
}
