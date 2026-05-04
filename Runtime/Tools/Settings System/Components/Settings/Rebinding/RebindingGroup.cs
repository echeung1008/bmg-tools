using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlueMuffinGames.Tools.SettingsSystem.Rebinding
{
    public class RebindingGroup : MonoBehaviour
    {
        [SerializeField] private List<RebindingComponent> _externalRebindingComponents = new();

        public virtual void SetPlayerInputIndex(int playerInputIndex)
        {
            IEnumerable<RebindingComponent> allRebindingComponents = _externalRebindingComponents.Concat(GetComponentsInChildren<RebindingComponent>());
            foreach (var rebindingComponent in allRebindingComponents)
            {
                rebindingComponent.SetPlayerInputIndex(playerInputIndex);
            }
        }
    }
}
