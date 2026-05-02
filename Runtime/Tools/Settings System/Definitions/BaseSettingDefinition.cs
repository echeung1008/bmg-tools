using System;
using UnityEngine;

namespace BlueMuffinGames.Tools.SettingsSystem
{
    public abstract class BaseSettingDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeReference, SerializeReferenceTypePicker] private BaseSettingBehaviour _behaviour;
        [SerializeReference, SerializeReferenceTypePicker] private BaseOptionProvider _optionProvider;

        public string ID => _id;
        public abstract Type ValueType { get; }
        public abstract object DefaultValueObject { get; }
        public BaseSettingBehaviour Behaviour => _behaviour;
        public BaseOptionProvider OptionProvider => _optionProvider;
    }
}
