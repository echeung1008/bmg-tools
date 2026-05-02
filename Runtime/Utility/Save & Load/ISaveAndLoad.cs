using System;
using UnityEngine;

namespace BlueMuffinGames.Utility.SaveAndLoad
{
    public interface ISaveAndLoad
    {
        public void Save<T>(string key, T value);

        public bool TryLoad<T>(string key, out T value);

        public bool TryLoad(string key, Type type, out object value);

        public void Delete(string key);
    }
}
