using UnityEngine;
using UnityEngine.UI;

namespace BlueMuffinGames.Tools.SettingsSystem
{
    public class BaseActionButton : MonoBehaviour
    {
        [SerializeField] private Button _button;

        protected virtual void OnEnable()
        {
            _button?.onClick.AddListener(HandleClicked);
        }

        protected virtual void OnDisable()
        {
            _button?.onClick.RemoveListener(HandleClicked);
        }

        protected virtual void HandleClicked() { }
    }
}
