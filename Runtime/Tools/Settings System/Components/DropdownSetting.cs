using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace BlueMuffinGames.Tools.SettingsSystem
{
    public class DropdownSetting : TypedSetting<string>
	{
		[SerializeField] private Dropdown _dropdown;

        private List<DropdownOption> _dropdownOptions = new();

        public override void Initialize()
        {
            if (_dropdown == null)
            {
                Debug.LogError($"(DropdownSetting) No Dropdown component is assigned on {name} with id {ID}.");
                return;
            }

            if (BaseSettingsManager.Instance == null)
            {
                Debug.LogError($"(DropdownSetting) BaseSettingsManager isn't initialized. Unable to retrieve dropdown options.");
                return;
            }

            if (!BaseSettingsManager.Instance.TryGetOptionProvider(ID, out var uncastedProvider))
            {
                Debug.LogError($"(DropdownSetting) No option provider is registered for the id {ID} in BaseSettingsManager.");
                return;
            }

            if (uncastedProvider is not DropdownOptionProvider provider)
            {
                Debug.LogError($"(DropdownSetting) The registered options provider for id {ID} is not a DropdownOptionProvider.");
                return;
            }

            provider.onOptionsChanged -= Initialize;
            provider.onOptionsChanged += Initialize;
            _dropdownOptions = provider.GetOptions();

            List<Dropdown.OptionData> optionDatas = new();
            foreach (var option in _dropdownOptions)
            {
                optionDatas.Add(new Dropdown.OptionData(provider.OptionToString(option)));
            }

            _dropdown.options = optionDatas;

            // call base initialize after options were fetched
            base.Initialize();

            _dropdown.onValueChanged.AddListener(HandleValueChanged);
        }

        protected override void SetVisualValue(string optionId, bool silent = false)
        {
            if (_dropdown == null) return;

            var index = IndexOf(optionId);

            if (index == -1)
            {
                Debug.LogError($"(DropdownSetting) Failed to set the visual value to option: {optionId}");
                return;
            }

            if (silent) _dropdown.SetValueWithoutNotify(index);
            else _dropdown.value = index;
        }

        private int IndexOf(string optionId)
        {
            if (_dropdownOptions == null || _dropdownOptions.Count == 0)
            {
                Debug.Log("No dropdown options registered yet");
                return -1;
            }

            for (int i = 0; i < _dropdownOptions.Count; i++)
            {
                if (optionId == _dropdownOptions[i].id) return i;
            }

            return -1;
        }

        private void HandleValueChanged(int index)
        {
            if (index < 0) return;
            if (_dropdownOptions == null || _dropdownOptions.Count == 0) return;
            if (index >= _dropdownOptions.Count) return;

            OnValueChanged(_dropdownOptions[index].id);
        }
    }
}