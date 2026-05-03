using UnityEngine;
using UnityEngine.InputSystem;

namespace BlueMuffinGames.Tools.SettingsSystem.Rebinding
{
    public class RebindingComponent : MonoBehaviour
    {
        protected int PlayerInputIndex { get; private set; } = -1;

        public void SetPlayerInputIndex(int index)
        {
            if (PlayerInputIndex == index) return;

            PlayerInputIndex = index;

            OnPlayerInputIndexChanged();
        }

        protected virtual void OnPlayerInputIndexChanged() { }
    }
}
