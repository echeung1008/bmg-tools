using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlueMuffinGames.Tools.SettingsSystem.Rebinding
{
    public class RebindingManager : MonoBehaviour
    {
        [SerializeField] private string _inputRebindsSettingDefinitionId;

        public static RebindingManager Instance { get; private set; }

        public event Action RebindingStarted = delegate { };
        public event Action RebindingStopped = delegate { };
        public event Action<int> OverridesChanged = delegate { };
        public event Action<int, InputAction> ActionRebinded = delegate { };

        protected InputRebinds _inputRebinds;

        public virtual void BeginRebinding(InputAction action, int playerInputIndex)
        {
            if (!TryGetPlayerInputAtIndex(playerInputIndex, out var playerInput) || playerInput == null)
            {
                Debug.LogError($"({nameof(RebindingManager)}) Failed to get a {nameof(PlayerInput)} at {nameof(playerInputIndex)} {playerInputIndex}");
                return;
            }

            if (!TryFindBindingGroup(playerInput, out var bindingGroup))
            {
                Debug.LogError($"({nameof(RebindingManager)}) Failed to find a binding group based on PlayerInput");
                return;
            }

            var inputAction = playerInput.actions.FindAction(action.id);
            if (inputAction == null) return;

            if (!TryFindBindingIndex(inputAction, bindingGroup, out int bindingIndex))
            {
                Debug.LogError($"({nameof(RebindingManager)}) Failed to find a binding index based on PlayerInput and binding group {bindingGroup}");
                return;
            }

            var cancelationInput = GetCancelationInput(bindingGroup);

            RebindingStarted?.Invoke();
            inputAction.PerformInteractiveRebinding(bindingIndex)
                .WithBindingGroup(bindingGroup)
                .WithCancelingThrough(cancelationInput)
                
                .OnComplete(operation =>
                {
                    RebindingStopped?.Invoke();

                    ActionRebinded?.Invoke(playerInputIndex, inputAction);
                    OverridesChanged?.Invoke(playerInputIndex);

                    RecordAllActionOverrides();
                })
                
                .OnCancel(operation => 
                {
                    RebindingStopped?.Invoke();
                });
        }

        public virtual void RecordAllActionOverrides()
        {
            if (BaseSettingsManager.Instance == null) return;
            
            if (_inputRebinds == null)
            {
                Debug.LogError($"({nameof(RebindingManager)}) Initialize {nameof(_inputRebinds)} before saving.");
                return;
            }

            if (string.IsNullOrEmpty(_inputRebindsSettingDefinitionId))
            {
                Debug.LogError($"({nameof(RebindingManager)}) Failed to load action overrides. No {nameof(_inputRebindsSettingDefinitionId)} is specified.");
                return;
            }

            foreach (var pair in _inputRebinds.rebinds)
            {
                if (!TryGetPlayerInputAtIndex(pair.Key, out var playerInput)) continue;

                _inputRebinds.rebinds[pair.Key] = playerInput.actions.SaveBindingOverridesAsJson();
            }

            BaseSettingsManager.Instance.RecordChange(_inputRebindsSettingDefinitionId, JsonConvert.SerializeObject(_inputRebinds));
        }

        public virtual void LoadAllActionOverrides()
        {
            if (BaseSettingsManager.Instance == null) return;
            if (string.IsNullOrEmpty(_inputRebindsSettingDefinitionId))
            {
                Debug.LogError($"({nameof(RebindingManager)}) Failed to load action overrides. No {nameof(_inputRebindsSettingDefinitionId)} is specified.");
                return;
            }

            string inputRebindsJson = string.Empty;
            BaseSettingsManager.Instance.TryGetValue(_inputRebindsSettingDefinitionId, out inputRebindsJson);
            _inputRebinds = InputRebinds.Deserialize(inputRebindsJson);

            foreach (var pair in _inputRebinds.rebinds)
            {
                if (!TryGetPlayerInputAtIndex(pair.Key, out var playerInput)) continue;

                playerInput.actions.LoadBindingOverridesFromJson(pair.Value);
            }
        }

        public virtual void RemoveBindingOverride(int playerInputIndex, InputAction inputAction)
        {

        }

        public virtual void RemoveAllBindingOverrides(int playerInputIndex)
        {

        }

        protected virtual bool TryGetPlayerInputAtIndex(int playerIndexIndex, out PlayerInput playerInput)
        {
            playerInput = null;
            if (playerIndexIndex < 0) return false;
            
            var playerInputs = FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
            if (playerInputs.Length == 0) return false;

            playerInput = playerInputs[Mathf.Clamp(playerIndexIndex, 0, playerInputs.Length - 1)];
            return true;
        }

        protected virtual bool TryFindBindingGroup(PlayerInput playerInput, out string bindingGroup)
        {
            bindingGroup = string.Empty;
            if (playerInput == null) return false;

            bindingGroup = playerInput.currentControlScheme ?? playerInput.defaultControlScheme;
            switch(bindingGroup)
            {
                case BindingGroupNames.KEYBOARD_AND_MOUSE: return true;
                case BindingGroupNames.GAMEPAD: return true;
                case BindingGroupNames.TOUCH: return true;
                case BindingGroupNames.JOYSTICK: return true;
                case BindingGroupNames.XR: return true;
                default:
                    Debug.LogError($"({nameof(RebindingManager)}) The binding group {bindingGroup} is not recognized in {nameof(BindingGroupNames)}.");
                    return false;
            }
        }

        protected virtual bool TryFindBindingIndex(InputAction action, string bindingGroup, out int index, string partName = "")
        {
            index = -1;
            if (action == null) return false;

            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];

                if (binding.isComposite) continue;

                if (!string.IsNullOrEmpty(bindingGroup))
                {
                    var groups = binding.groups ?? "";
                    if (!groups.Contains(bindingGroup)) continue;
                }

                if (!string.IsNullOrEmpty(partName))
                {
                    if (!binding.isPartOfComposite || binding.name != partName) continue;
                }

                index = i;
                return true;
            }

            return false;
        }

        protected virtual string GetCancelationInput(string bindingGroup)
        {
            switch (bindingGroup)
            {
                case BindingGroupNames.KEYBOARD_AND_MOUSE: return "<Keyboard>/escape";
                case BindingGroupNames.GAMEPAD: return "<Gamepad>/menu";
                default:
                    return "<Keyboard>/escape";
            }
        }

        protected virtual void Start()
        {
            LoadAllActionOverrides();
        }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else Destroy(gameObject);
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
