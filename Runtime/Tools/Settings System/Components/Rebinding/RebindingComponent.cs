using UnityEngine;
using UnityEngine.InputSystem;

namespace BlueMuffinGames.Tools.SettingsSystem.Rebinding
{
    public class RebindingComponent : MonoBehaviour
    {
        [SerializeField] private InputActionProperty _inputActionProperty;

        protected int PlayerInputIndex { get; private set; } = -1;
        protected InputAction TargetAction => _inputActionProperty.action;

        public void SetPlayerInputIndex(int index)
        {
            if (PlayerInputIndex == index) return;

            PlayerInputIndex = index;

            OnPlayerInputIndexChanged();
        }

        protected virtual void OnPlayerInputIndexChanged() { }
    }
}
