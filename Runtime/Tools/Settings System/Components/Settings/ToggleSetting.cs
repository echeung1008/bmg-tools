using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace BlueMuffinGames.Tools.SettingsSystem
{
    public class ToggleSetting : TypedSetting<bool>
    {
        [SerializeField] private Toggle _toggle;

        public override void Initialize()
        {
            base.Initialize();

            if (_toggle == null)
            {
                Debug.LogError($"(ToggleSetting) No Toggle component is assigned on {name} with id {ID}.");
                return;
            }

            _toggle.onValueChanged.AddListener(OnValueChanged);
        }

        protected override void SetVisualValue(bool value, bool silent = false)
        {
            if (_toggle == null) return;

            if (silent) _toggle.SetIsOnWithoutNotify(value);
            else _toggle.isOn = value;
        }
    }
}