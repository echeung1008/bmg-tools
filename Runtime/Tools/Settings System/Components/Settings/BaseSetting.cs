using UnityEngine;

namespace BlueMuffinGames.Tools.SettingsSystem
{
    public abstract class BaseSetting : MonoBehaviour
    {
        [SerializeField] private BaseSettingDefinition _settingDefinition;
        
        public string ID => _settingDefinition != null ? _settingDefinition.ID : string.Empty;

        public virtual void Initialize() { }

        public virtual void UpdateVisual()
        {
            if (BaseSettingsManager.Instance != null) BaseSettingsManager.Instance.ResetSetting(ID);
        }

        protected virtual void SetSetting(object value)
        {
            if (BaseSettingsManager.Instance != null) BaseSettingsManager.Instance.RecordChange(ID, value);
        }
    }
}
