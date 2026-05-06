using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BlueMuffinGames.Utility.Registry
{
    public abstract class BaseRegistry : ScriptableObject
    {
        [SerializeField] private bool _autoGenerate = true;

        [Header("Folders")]
        [SerializeField] private List<Object> _folders = new();

        public bool AutoGenerate => _autoGenerate;

#if UNITY_EDITOR
        public void Rescan()
        {
            var folderPaths = ResolveFolderPaths(_folders);

            ProcessFolders(folderPaths);

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        protected abstract void ProcessFolders(string[] folderPaths);

        protected static List<GameObject> FindPrefabsWithComponent<T>(string[] folderPaths)
        {
            // Find all prefabs in folders
            var guids = AssetDatabase.FindAssets("t:Prefab", folderPaths);

            var results = new List<GameObject>(guids.Length);
            var seen = new HashSet<string>(); // de-dupe by GUID

            foreach (var guid in guids)
            {
                if (!seen.Add(guid)) continue;

                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                // Check for component on root or children.
                if (prefab.TryGetComponent(out T component))
                    results.Add(prefab);
            }

            return results;
        }

        protected static List<T> FindScriptableObjects<T>(string[] folderPaths) where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", folderPaths);

            var results = new List<T>(guids.Length);
            var seen = new HashSet<string>();

            foreach (var guid in guids)
            {
                if (!seen.Add(guid)) continue;

                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    results.Add(asset);
            }

            return results;
        }

        private static string[] ResolveFolderPaths(List<Object> folderObjects)
        {
            var paths = new List<string>(folderObjects.Count);

            foreach (var obj in folderObjects)
            {
                if (obj == null) continue;

                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path)) continue;

                if (AssetDatabase.IsValidFolder(path))
                    paths.Add(path);
                else
                    Debug.LogWarning($"'{obj.name}' is not a folder. Remove it from {typeof(BaseRegistry)}.");
            }

            // If empty, search whole project (optional — you may prefer to return empty to force selection)
            return paths.Count > 0 ? paths.ToArray() : new[] { "Assets" };
        }
#endif
    }
}
