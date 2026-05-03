using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BlueMuffinGames.Tools.SettingsSystem.Rebinding
{
    public class RebindActionButton : RebindingComponent
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _display;
        [SerializeField] private TMP_Text _label;

        protected override void OnPlayerInputIndexChanged()
        {
            UpdateDisplay();
        }

        protected virtual void UpdateLabel()
        {
            if (TargetAction == null) return;

            SetLabel(TargetAction.name);
        }

        protected virtual void UpdateDisplay()
        {
            if (TargetAction == null) return;
            if (RebindingManager.Instance == null) return;

            SetDisplay(RebindingManager.Instance.GetActionBindingDisplayString(PlayerInputIndex, TargetAction));
        }

        protected virtual void SetLabel(string labelString)
        {
            if (_label != null) _label.text = labelString;
        }

        protected virtual void SetDisplay(string displayString)
        {
            if (_display != null) _display.text = displayString;
        }

        protected virtual void Awake()
        {
            UpdateLabel();
        }

        protected virtual void OnEnable()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(HandleOnClick);
            }

            if (RebindingManager.Instance != null)
            {
                RebindingManager.Instance.OverridesChanged += HandleOverridesChanged;
            }
        }

        protected virtual void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleOnClick);
            }

            if (RebindingManager.Instance != null)
            {
                RebindingManager.Instance.OverridesChanged -= HandleOverridesChanged;
            }
        }

        protected virtual void HandleOnClick()
        {
            if (TargetAction == null)
            {
                Debug.LogError($"({nameof(RebindActionButton)}) No {nameof(TargetAction)} is assigned. Select an action from a selected {nameof(InputActionAsset)}.");
                return;
            }

            if (PlayerInputIndex == -1)
            {
                Debug.LogError($"({nameof(RebindActionButton)}) {nameof(PlayerInputIndex)} is not initialized yet. Ensure it is set to a positive index.");
                return;
            }

            if (RebindingManager.Instance == null)
            {
                Debug.LogError($"({nameof(RebindActionButton)}) No {nameof(RebindingManager)} found.");
                return;
            }

            RebindingManager.Instance.BeginRebinding(TargetAction, PlayerInputIndex);
        }

        protected virtual void HandleOverridesChanged(int playerInputIndex)
        {
            if (playerInputIndex != PlayerInputIndex) return;

            UpdateDisplay();
        }

#if UNITY_EDITOR
        [ContextMenu("Print PlayerInputIndex")]
        public void PrintPlayerInputIndex()
        {
            Debug.Log(PlayerInputIndex);
        }
#endif
    }
}
