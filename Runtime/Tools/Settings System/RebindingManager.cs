using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

            inputAction.Disable();
            RebindingStarted?.Invoke();
            
            inputAction.PerformInteractiveRebinding(bindingIndex)
                .WithBindingGroup(bindingGroup)
                .WithCancelingThrough(cancelationInput)
                
                .OnComplete(operation =>
                {
                    RebindingStopped?.Invoke();
                    inputAction.Enable();

                    RecordAllActionOverrides();

                    operation.Dispose();
                })
                
                .OnCancel(operation => 
                {
                    RebindingStopped?.Invoke();
                    inputAction.Enable();

                    operation.Dispose();
                })
                
                .Start();
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

            foreach (var playerInput in GetAllPlayerInputs())
            {
                if (playerInput == null) continue;

                _inputRebinds.rebinds[playerInput.playerIndex] = playerInput.actions.SaveBindingOverridesAsJson();
            }

            BaseSettingsManager.Instance.RecordChange(_inputRebindsSettingDefinitionId, JsonConvert.SerializeObject(_inputRebinds));
        }

        public virtual void LoadAllActionOverrides(string inputRebindsJson = "")
        {
            if (BaseSettingsManager.Instance == null) return;
            if (string.IsNullOrEmpty(_inputRebindsSettingDefinitionId))
            {
                Debug.LogError($"({nameof(RebindingManager)}) Failed to load action overrides. No {nameof(_inputRebindsSettingDefinitionId)} is specified.");
                return;
            }

            if (string.IsNullOrEmpty(inputRebindsJson))
            {
                BaseSettingsManager.Instance.TryGetValue(_inputRebindsSettingDefinitionId, out inputRebindsJson, onlyApplied: false);
            }
            
            Debug.Log($"Loaded input rebinds from json: {inputRebindsJson}");
            _inputRebinds = InputRebinds.Deserialize(inputRebindsJson);

            if (_inputRebinds == null) return;

            foreach (var playerInput in GetAllPlayerInputs())
            {
                if (playerInput == null) continue;
                var json = _inputRebinds.rebinds.TryGetValue(playerInput.playerIndex, out var cached) ? cached : string.Empty;

                playerInput.actions.LoadBindingOverridesFromJson(json);
                

                OverridesChanged?.Invoke(playerInput.playerIndex);
            }
        }

        public virtual void RemoveBindingOverride(int playerInputIndex, InputAction inputAction)
        {

        }

        public virtual void RemoveAllBindingOverrides(int playerInputIndex)
        {

        }

        public virtual string GetActionBindingDisplayString(int playerInputIndex, InputAction inputAction)
        {
            const string FAIL_MESSAGE = "ERROR";
            if (!TryGetPlayerInputAtIndex(playerInputIndex, out var playerInput)) return FAIL_MESSAGE;

            var action = playerInput.actions.FindAction(inputAction.id);
            if (action == null) return FAIL_MESSAGE;

            if (!TryFindBindingGroup(playerInput, out var bindingGroup)) return FAIL_MESSAGE;
            if (!TryFindBindingIndex(action, bindingGroup, out var bindingIndex)) return FAIL_MESSAGE;

            return action.bindings[bindingIndex].ToDisplayString();
        }

        protected virtual bool TryGetPlayerInputAtIndex(int playerInputIndex, out PlayerInput playerInput)
        {
            playerInput = null;
            if (playerInputIndex < 0) return false;

            var playerInputs = GetAllPlayerInputs();
            foreach (var input in playerInputs)
            {
                if (input != null && input.playerIndex == playerInputIndex)
                {
                    playerInput = input;
                    return true;
                }
            }

            return false;
        }

        protected virtual List<PlayerInput> GetAllPlayerInputs()
        {
            return FindObjectsByType<PlayerInput>(FindObjectsSortMode.None).ToList();
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
                    var groups = (binding.groups ?? "")
                        .Split(';', StringSplitOptions.RemoveEmptyEntries);

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

            BaseSettingsManager.Instance.OnChangeRecorded += HandleChangeRecorded;
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

        private void HandleChangeRecorded(string id, object value)
        {
            if (_inputRebindsSettingDefinitionId != id) return;
            Debug.Log($"(RebindingManager) HandleChangeRecorded({id}, {value})");

            if (value is not string json) return;

            LoadAllActionOverrides(json);
        }
    }
}
