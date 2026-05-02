using UnityEngine;
using Newtonsoft.Json;
using System;

namespace BlueMuffinGames.Utility.SaveAndLoad
{
    public class PlayerPrefsSaveAndLoad : MonoBehaviour, ISaveAndLoad
    {
        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        public void Save<T>(string key, T value)
        {
            switch (value)
            {
                case bool boolValue:
                    PlayerPrefs.SetInt(key, boolValue ? 1 : 0);
                    break;
                case int intValue:
                    PlayerPrefs.SetInt(key, intValue);
                    break;
                case float floatValue:
                    PlayerPrefs.SetFloat(key, floatValue);
                    break;
                case string stringValue:
                    PlayerPrefs.SetString(key, value.ToString());
                    break;
                default:
                    PlayerPrefs.SetString(key, JsonConvert.SerializeObject(value));
                    break;
            }

            PlayerPrefs.Save();
        }

        public bool TryLoad<T>(string key, out T value)
        {
            value = default;
            if (!TryLoad(key, typeof(T), out object uncastedValue)) return false;
            
            if (uncastedValue is not T casted) return false;

            value = casted;
            return true;
        }

        public bool TryLoad(string key, Type type, out object value)
        {
            value = null;

            if (!PlayerPrefs.HasKey(key))
                return false;

            object result;

            if (type == typeof(bool))
            {
                result = PlayerPrefs.GetInt(key) == 1;
            }
            else if (type == typeof(int))
            {
                result = PlayerPrefs.GetInt(key);
            }
            else if (type == typeof(float))
            {
                result = PlayerPrefs.GetFloat(key);
            }
            else if (type == typeof(string))
            {
                result = PlayerPrefs.GetString(key);
            }
            else
            {
                string json = PlayerPrefs.GetString(key);
                result = JsonConvert.DeserializeObject(json, type);
            }

            if (result == null)
                return false;

            if (!type.IsInstanceOfType(result))
                return false;

            value = result;
            return true;
        }
    }
}
