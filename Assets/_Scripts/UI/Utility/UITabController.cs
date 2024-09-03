using System;
using UnityEngine;
using UnityEngine.UI;

public class UITabController : MonoBehaviour
{
    [Serializable]
    public struct UiTab {
        public Button button;
        public GameObject Tab;
    }

    [SerializeField] public UiTab[] tabs = new UiTab[0];

    void Awake()
    {
        foreach (var tab in tabs)
        {
            tab.button.onClick.AddListener(() => {
                foreach (var t in tabs)
                {
                    if (t.Tab != null)
                    {
                        t.Tab.SetActive(false);
                    }
                }
                tab.Tab.SetActive(true);
            });
        }
    }

    void Start()
    {
        foreach (var tab in tabs)
        {
            if (tab.Tab != null)
            {
                tab.Tab.SetActive(false);
            }
        }
    }
}
