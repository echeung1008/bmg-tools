using UnityEngine;
using UnityEngine.UI;

namespace BlueMuffinGames.Tools.SettingsSystem
{
    public class ResetAllSettingsButton : BaseActionButton
    {
        protected override void HandleClicked()
        {
            if (BaseSettingsManager.Instance == null) return;

            BaseSettingsManager.Instance.ResetAllSettings();
        }
    }
}
