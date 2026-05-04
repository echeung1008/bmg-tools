using UnityEngine;

namespace BlueMuffinGames.Tools.SettingsSystem
{
    public class ApplyChangesButton : BaseActionButton
    {
        protected override void HandleClicked()
        {
            if (BaseSettingsManager.Instance == null) return;

            BaseSettingsManager.Instance.PushAllChanges();
        }
    }
}
