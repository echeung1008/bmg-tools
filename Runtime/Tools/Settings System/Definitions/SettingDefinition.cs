using System;
using UnityEngine;

namespace BlueMuffinGames.Tools.SettingsSystem
{
    public abstract class SettingDefinition<T> : BaseSettingDefinition
    {
        [SerializeField] private T _defaultValue;

        public override Type ValueType => typeof(T);
        public override object DefaultValueObject => _defaultValue;
    }
}
