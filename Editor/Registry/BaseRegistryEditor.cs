#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BlueMuffinGames.Utility.Registry
{
    [CustomEditor(typeof(BaseRegistry), editorForChildClasses: true)]
    public class BaseRegistryEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            var reg = target as BaseRegistry;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Rescan"))
                {
                    reg.Rescan();
                    Debug.Log($"Rescanned {reg.name}.");
                }
            }
        }
    }
#endif
}
