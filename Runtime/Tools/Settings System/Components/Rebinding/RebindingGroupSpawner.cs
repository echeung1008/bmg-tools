using System.Collections.Generic;
using UnityEngine;

namespace BlueMuffinGames.Tools.SettingsSystem.Rebinding
{
    public class RebindingGroupSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _tabPrefab;

        private Dictionary<int, GameObject> _rebindingTabRegistry = new();
        
        public void OnPlayerJoined(int playerIndex)
        {
            if (_rebindingTabRegistry.TryGetValue(playerIndex, out var go))
            {
                go.SetActive(true);
                return;
            }

            var newTab = Instantiate(_tabPrefab);

            newTab.transform.SetParent(transform);

            _rebindingTabRegistry[playerIndex] = newTab;

            if (newTab.TryGetComponent(out RebindingGroup rebindingGroup)) rebindingGroup.SetPlayerInputIndex(playerIndex);
            else
            {
                foreach (var rebindingComponent in newTab.GetComponentsInChildren<RebindingComponent>())
                {
                    rebindingComponent.SetPlayerInputIndex(playerIndex);
                }
            }
        }

        public void OnPlayerLeft(int playerIndex)
        {
            if (_rebindingTabRegistry.TryGetValue(playerIndex, out var go)) go.SetActive(false);
        }
    }
}
