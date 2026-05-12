using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlueMuffinGames.Tools.MenuController
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private bool _showFirstPageOnStart;
        [SerializeField] private bool _allowExternalPages;

        public event Action onStackEmptied = delegate { };

        public IReadOnlyDictionary<string, MenuPage> PageRegistry => _pageRegistry;

        private Dictionary<string, MenuPage> _pageRegistry = new();
        private Stack<MenuPage> _navigationStack = new();

        /// <summary>
        /// Push (show) a new page. Starts after hiding the current page, if any.
        /// </summary>
        /// <param name="page">The page to push.</param>
        /// <param name="onHideComplete">Invoked when the current page is hidden.</param>
        /// <param name="onShowComplete">Invoked when the new page is shown.</param>
        public virtual void PushPage(MenuPage page, Action onHideComplete = null, Action onShowComplete = null, params object[] args)
        {
            if (!_allowExternalPages && !_pageRegistry.ContainsKey(page.name)) return;

            if (_navigationStack.TryPeek(out MenuPage currentPage)) currentPage.Hide(() =>
            {
                Push(); // push after the current page finishes hiding
            }, args);
            else Push(); // push immediately

            void Push()
            {
                _navigationStack.Push(page);
                onHideComplete?.Invoke();
                page.Show(onShowComplete, args);
            }
        }
        public void PushPage(string pageName, Action onHideComplete = null, Action onShowComplete = null, params object[] args)
        {
            if (!_pageRegistry.TryGetValue(pageName, out MenuPage page))
            {
                Debug.LogWarning($"[MenuController] Page {pageName} not found in pages.");
                return;
            }
            PushPage(page, onHideComplete, onShowComplete, args);
        }

        /// <summary>
        /// Pop (hide) the current page. Starts showing the previous page after hiding, if any.
        /// </summary>
        /// <param name="onHideComplete">Invoked when the current page is hidden.</param>
        /// <param name="onShowComplete">Invoked when the previous page is shown.</param>
        /// <returns>The popped page.</returns>
        public virtual MenuPage PopPage(Action onHideComplete = null, Action onShowComplete = null, params object[] args)
        {
            MenuPage result = null;

            if (_navigationStack.TryPeek(out result)) result.Hide(() =>
            {
                _navigationStack.Pop();
                onHideComplete?.Invoke();
                if (_navigationStack.TryPeek(out MenuPage previousPage)) previousPage.Show(onShowComplete, args);
                else onStackEmptied?.Invoke();
            }, args);
            
            return result;
        }

        private void Awake()
        {
            foreach(Transform child in transform)
            {
                if (child.TryGetComponent(out MenuPage menuPage))
                {
                    if (_pageRegistry.ContainsKey(menuPage.name))
                    {
                        Debug.LogWarning($"The page registry already contains a page with the name {menuPage.name}.");
                        continue;
                    }

                    _pageRegistry[menuPage.name] = menuPage;
                }
            }
        }

        private void Start()
        {
            foreach(MenuPage page in _pageRegistry.Values)
            {
                page.Initialize();
            }

            if (_showFirstPageOnStart && _pageRegistry.Count > 0)
            {
                PushPage(_pageRegistry.Values.First());
            }
        }
    }
}
