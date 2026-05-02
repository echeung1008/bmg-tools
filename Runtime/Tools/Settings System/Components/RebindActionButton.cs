using TMPro;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BlueMuffinGames.Tools.SettingsSystem.Rebinding
{
    [RequireComponent(typeof(Button))]
    public class RebindActionButton : MonoBehaviour
    {
        [SerializeField] private InputActionProperty _inputActionProperty;
        [SerializeField] private TMP_Text _display;
        [SerializeField] private TMP_Text _label;

        private int _playerInputIndex = -1;

        private Button _button;

        public virtual void SetPlayerInputIndex(int index)
        {
            _playerInputIndex = index;

            UpdateDisplay();
        }

        protected virtual void UpdateLabel()
        {
            if (_inputActionProperty.action == null) return;

            SetLabel(_inputActionProperty.action.name);
        }

        protected virtual void UpdateDisplay()
        {
            if (_inputActionProperty.action == null) return;
            if (RebindingManager.Instance == null) return;

            SetDisplay(RebindingManager.Instance.GetActionBindingDisplayString(_playerInputIndex, _inputActionProperty.action));
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
            if (TryGetComponent(out _button))
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
            if (_inputActionProperty == null)
            {
                Debug.LogError($"({nameof(RebindActionButton)}) No {nameof(_inputActionProperty)} is assigned. Select an action from a selected {nameof(InputActionAsset)}.");
                return;
            }

            if (_playerInputIndex == -1)
            {
                Debug.LogError($"({nameof(RebindActionButton)}) {nameof(_playerInputIndex)} is not initialized yet. Ensure it is set to a positive index.");
                return;
            }

            if (RebindingManager.Instance == null)
            {
                Debug.LogError($"({nameof(RebindActionButton)}) No {nameof(RebindingManager)} found.");
                return;
            }

            RebindingManager.Instance.BeginRebinding(_inputActionProperty.action, _playerInputIndex);
        }

        protected virtual void HandleOverridesChanged(int playerInputIndex)
        {
            Debug.Log($"(RebindActionButton) HandleOverridesChanged({playerInputIndex}) @ {_inputActionProperty.action.name}");
            if (playerInputIndex != _playerInputIndex) return;

            UpdateDisplay();
        }

#if UNITY_EDITOR
        [ContextMenu("Print PlayerInputIndex")]
        public void PrintPlayerInputIndex()
        {
            Debug.Log(_playerInputIndex);
        }
#endif
    }
}
