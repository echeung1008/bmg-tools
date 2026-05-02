using UnityEngine;
using UnityEngine.UI;

namespace BlueMuffinGames.Tools.SettingsSystem
{
    public class IntSliderSetting : TypedSetting<int>
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private Vector2Int _range;

        public override void Initialize()
        {
            base.Initialize();

            if (_slider == null)
            {
                Debug.LogError($"(IntSliderSetting) No Slider component is assigned on {name} with id {ID}.");
                return;
            }

            _slider.minValue = _range.x;
            _slider.maxValue = _range.y;
            _slider.wholeNumbers = true;
            
            _slider.onValueChanged.AddListener(OnValueChangedFloat);
        }

        protected override void SetVisualValue(int value, bool silent = false)
        {
            if (_slider == null) return;

            if (silent) _slider.SetValueWithoutNotify(value);
            else _slider.value = value;
        }

        private void OnValueChangedFloat(float value)
        {
            OnValueChanged(Mathf.RoundToInt(value));
        }
    }
}
