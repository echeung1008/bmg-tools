using UnityEngine;

namespace BlueMuffinGames.Tools.SettingsSystem
{
    public abstract class TypedSettingBehaviour<T> : BaseSettingBehaviour
    {
        public sealed override void OnValueChanged(object value)
        {
            if (value is not T casted)
            {
                Debug.LogError($"(TypedSettingBehaviour) Failed to cast value {value} to type {typeof(T)} in SettingBehaviour {GetType()}");
                return;
            }

            OnValueChanged(casted);
        }

        public sealed override void OnValueApplied(object value)
        {
            if (value is not T casted)
            {
                Debug.LogError($"(TypedSettingBehaviour) Failed to cast value {value} to type {typeof(T)} in SettingBehaviour {GetType()}");
                return;
            }

            OnValueApplied(casted);
        }

        protected virtual void OnValueChanged(T value) { }
        protected virtual void OnValueApplied(T value) { }
    }
}
