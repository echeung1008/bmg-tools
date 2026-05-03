using UnityEngine;
using UnityEngine.InputSystem;

namespace BlueMuffinGames.Tools.SettingsSystem.Rebinding
{
    public class ActionRebindingComponent : RebindingComponent
    {
        [SerializeField] private InputActionReference _inputActionReference;

        protected InputAction TargetAction => _inputActionReference.action;
    }
}
