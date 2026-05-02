using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

namespace BlueMuffinGames.Tools.SettingsSystem
{
    public class FloatSliderSetting : TypedSetting<float>
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private Vector2 _range;

        public override void Initialize()
        {
            base.Initialize();

            if (_slider == null)
            {
                Debug.LogError($"(FloatSliderSetting) No Slider component is assigned on {name} with id {ID}.");
                return;
            }

            _slider.minValue = _range.x;
            _slider.maxValue = _range.y;
            _slider.wholeNumbers = false;

            _slider.onValueChanged.AddListener(OnValueChanged);
        }

        protected override void SetVisualValue(float value, bool silent = false)
        {
            if (_slider == null) return;

            if (silent) _slider.SetValueWithoutNotify(value);
            else _slider.value = value;
        }
    }
}
