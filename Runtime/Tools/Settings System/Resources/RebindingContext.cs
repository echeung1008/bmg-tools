using UnityEngine;
using UnityEngine.InputSystem;

namespace BlueMuffinGames.Tools.SettingsSystem.Rebinding
{
    public struct RebindingContext
    {
        public readonly InputAction inputAction;
        public readonly string bindingGroup;
        public readonly int bindingIndex;
        public readonly string description;
        public readonly string cancelationInput;
        public readonly object customPayload;

        public RebindingContext(
            InputAction inputAction,
            string bindingGroup,
            int bindingIndex,
            string description = "",
            string cancelationInput = "",
            object customPayload = null
        )
        {
            this.inputAction = inputAction;
            this.bindingGroup = bindingGroup;
            this.bindingIndex = bindingIndex;
            this.description = description;
            this.cancelationInput = cancelationInput;
            this.customPayload = customPayload;
        }

        public bool TryGetPayload<T>(out T payload)
        {
            payload = default;
            if (customPayload == null) return false;

            if (customPayload is not T casted) return false;
            payload = casted;
            return true;
        }
    }
}
