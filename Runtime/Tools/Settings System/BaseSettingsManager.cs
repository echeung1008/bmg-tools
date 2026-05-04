using BlueMuffinGames.Utility.SaveAndLoad;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlueMuffinGames.Tools.SettingsSystem
{
    public class BaseSettingsManager : MonoBehaviour
    {
        public static BaseSettingsManager Instance { get; private set; }

        [SerializeField] protected SettingsRegistry _registry;
        [SerializeField] protected bool _debug;
        [SerializeField] private GameObject _wrappedSaveAndLoad;

        public event Action<string, object> OnChangeRecorded = delegate { };

        public bool HasUnsavedChanges => _changeRegistry != null ? _changeRegistry.Count > 0 : false;

        protected Dictionary<string, BaseSettingDefinition> _registeredSettingDefinitions = new();

        protected Dictionary<string, object> _registeredValues = new();
        protected Dictionary<string, BaseSettingBehaviour> _registeredBehaviours = new();
        protected Dictionary<string, BaseOptionProvider> _registeredOptionProviders = new();
        protected Dictionary<string, object> _changeRegistry = new();

        protected ISaveAndLoad _saveAndLoad;

        public virtual bool TryGetValue<T>(string id, out T value, bool onlyApplied = true)
        {
            value = default;
            if (!TryGetValue(id, typeof(T), out var uncastedValue, onlyApplied)) return false;

            if (uncastedValue is not T castedValue)
            {
                Debug.LogError($"(BaseSettingsManager) Failed to cast value {uncastedValue} to type {typeof(T)}");
                return false;
            }

            value = castedValue;
            return true;
        }

        public virtual bool TryGetValue(string id, Type type, out object value, bool onlyApplied = true)
        {
            value = default;

            // allow the change registry to return value if not onlyApplied
            if (!onlyApplied && _changeRegistry.TryGetValue(id, out value))
            {
                return true;
            }

            if (!_registeredValues.TryGetValue(id, out value))
            {
                Debug.LogError($"(BaseSettingsManager) Registry does not contain the id {id}");
                return false;
            }
            
            return true;
        }

        public virtual void RecordChange(string id, object value)
        {
            _changeRegistry[id] = value;

            if (_registeredBehaviours.TryGetValue(id, out var behaviour))
            {
                behaviour.OnValueChanged(value);
            }

            OnChangeRecorded?.Invoke(id, value);
        }

        public virtual void PushAllChanges()
        {
            foreach (var pair in _changeRegistry)
            {
                _registeredValues[pair.Key] = pair.Value;

                if (_registeredBehaviours.TryGetValue(pair.Key, out var behaviour))
                {
                    behaviour.OnValueApplied(pair.Value);
                }
                
                SaveSetting(pair.Key, pair.Value);
                
                // check if it was set back to the default value
                if (_registeredSettingDefinitions.TryGetValue(pair.Key, out var definition) && 
                    definition.DefaultValueObject.Equals(pair.Value)
                )
                {
                    var saveKey = GetSaveKey(pair.Key);
                    _saveAndLoad?.Delete(saveKey);

                    if (_debug) Debug.Log($"Reset setting {pair.Key} to its default value.");
                }
            }
        }

        public virtual void ResetSetting(string id)
        {
            if (!_registeredSettingDefinitions.TryGetValue(id, out var definition))
            {
                Debug.LogError($"(BaseSettingsManager) Failed to get the definition of setting {id} for resetting.");
                return;
            }

            RecordChange(id, definition.DefaultValueObject);
        }

        public virtual void ResetAllSettings()
        {
            foreach (var id in _registeredSettingDefinitions.Keys)
            {
                ResetSetting(id);
            }
        }

        public virtual void ClearAllChanges()
        {
            // reset changed values to the applied values
            List<Action> pendingActions = new();
            foreach (var pair in _changeRegistry)
            {
                if (!_registeredSettingDefinitions.TryGetValue(pair.Key, out var definition)) continue;
                if (!TryGetValue(pair.Key, definition.ValueType, out var originalValue, onlyApplied: true)) continue;

                pendingActions.Add(() => RecordChange(pair.Key, originalValue));
            }

            foreach (var action in pendingActions) action?.Invoke();

            _changeRegistry.Clear();
        }

        public virtual bool TryGetBehaviour(string id, out BaseSettingBehaviour behaviour)
        {
            behaviour = null;
            if (!_registeredBehaviours.TryGetValue(id, out behaviour))
            {
                Debug.LogError($"(BaseSettingsManager) Behaviour Registry does not contain a behaviour for id {id}");
                return false;
            }
            return true;
        }

        public virtual bool TryGetOptionProvider(string id, out BaseOptionProvider provider)
        {
            provider = null;
            if (!_registeredOptionProviders.TryGetValue(id, out provider))
            {
                Debug.LogError($"(BaseSettingsManager) Option Provider Registry does not contain a provider for id {id}");
                return false;
            }
            return true;
        }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                _saveAndLoad = _wrappedSaveAndLoad != null ? _wrappedSaveAndLoad.GetComponent<ISaveAndLoad>() : null;

                RegisterSettings();

                if (_debug) PrintRegisteredValues();
            }
            else
            {
                if (_debug) Debug.LogWarning("(BaseSettingsManager) Tried to have more than one BaseSettingsManager instance. Destroying extra...");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        protected virtual bool TryLoadSetting(string id, Type type, out object value)
        {
            value = default;
            var key = GetSaveKey(id);

            if (_saveAndLoad == null) return false;

            return _saveAndLoad.TryLoad(key, type, out value);
        }

        protected virtual void SaveSetting<T>(string id, T value)
        {
            var key = GetSaveKey(id);

            _saveAndLoad?.Save(key, value);

            if (_debug) Debug.Log($"Saved setting {id} with value {value?.ToString()}");
        }

        private void RegisterSettings()
        {
            foreach (var definition in _registry.SettingDefinitions)
            {
                if (_registeredSettingDefinitions.ContainsKey(definition.ID))
                {
                    Debug.LogWarning($"(BaseSettingsManager) A SettingDefinition with ID {definition.ID} has already been registered. Skipping...");
                    continue;
                }

                _registeredSettingDefinitions[definition.ID] = definition;
                if (definition.Behaviour != null) _registeredBehaviours[definition.ID] = definition.Behaviour;
                if (definition.OptionProvider != null) _registeredOptionProviders[definition.ID] = definition.OptionProvider;

                object initialValue = definition.DefaultValueObject;
                if (TryLoadSetting(definition.ID, definition.ValueType, out object savedValue)) initialValue = savedValue;

                RecordChange(definition.ID, initialValue);
                if (_debug) Debug.Log($"(BaseSettingsManager) Registered setting {definition.ID} with initial value {initialValue}");
            }

            PushAllChanges();
        }

        protected virtual string GetSaveKey(string id) => $"setting.{id}";


        #if UNITY_EDITOR
        [ContextMenu("Print Registered Values")]
        private void PrintRegisteredValues()
        {
            string result = "Registered Setting Values:";
            foreach (var pair in _registeredValues)
            {
                result += $"\n\t{pair.Key} => {pair.Value?.ToString()}";
            }

            Debug.Log(result);
        }

        [ContextMenu("Apply Changes")]
        private void ApplyChanges()
        {
            PushAllChanges();
        }

        [ContextMenu("Clear Changes")]
        private void ClearChanges()
        {
            ClearAllChanges();
        }

        [ContextMenu("Reset All Settings")]
        private void ResetAllValues()
        {
            ResetAllSettings();
        }
        #endif
    }
}
