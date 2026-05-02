using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BlueMuffinGames.Tools.SettingsSystem.Rebinding
{
    [RequireComponent(typeof(Button))]
    public class RebindActionButton : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _inputActionAsset;
        [SerializeField] private InputActionReference _inputActionReference;

        public int playerInputIndex = -1;

        private Button _button;

        private void OnEnable()
        {
            if (TryGetComponent(out _button))
            {
                _button.onClick.AddListener(HandleOnClick);
            }
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleOnClick);
            }
        }

        private void HandleOnClick()
        {
            if (_inputActionReference == null)
            {
                Debug.LogError($"({nameof(RebindActionButton)}) No {nameof(_inputActionReference)} is assigned. Select an action from a selected {nameof(InputActionAsset)}.");
                return;
            }

            if (playerInputIndex == -1)
            {
                Debug.LogError($"({nameof(RebindActionButton)}) {nameof(playerInputIndex)} is not initialized yet. Ensure it is set to a positive index.");
                return;
            }

            if (RebindingManager.Instance == null)
            {
                Debug.LogError($"({nameof(RebindActionButton)}) No {nameof(RebindingManager)} found.");
                return;
            }

            RebindingManager.Instance.BeginRebinding(_inputActionReference.action, playerInputIndex);
        }
    }
}
