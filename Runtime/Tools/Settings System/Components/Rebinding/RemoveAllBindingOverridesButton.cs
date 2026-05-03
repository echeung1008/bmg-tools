using UnityEngine;
using UnityEngine.UI;

namespace BlueMuffinGames.Tools.SettingsSystem.Rebinding
{
    public class RemoveAllBindingOverridesButton : RebindingComponent
    {
        [SerializeField] private Button _button;

        protected virtual void OnEnable()
        {
            _button?.onClick.AddListener(HandleClicked);
        }

        protected virtual void OnDisable()
        {
            _button?.onClick.RemoveListener(HandleClicked);
        }

        protected virtual void HandleClicked()
        {
            if (RebindingManager.Instance == null) return;
            if (PlayerInputIndex == -1) return;

            RebindingManager.Instance.RemoveAllBindingOverrides(PlayerInputIndex);
        }
    }
}
