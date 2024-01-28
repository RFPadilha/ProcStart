using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    public List<UITabButton> tabButtons;
    public Sprite tabIdle;
    public Sprite tabHover;
    public Sprite tabActive;
    public UITabButton selectedTab;
    public List<GameObject> pagesToSwap;
    public void Subscribe(UITabButton button)
    {
        if (tabButtons == null)
        {
            tabButtons = new List<UITabButton>();
        }
        tabButtons.Add(button);
        if(tabButtons.Count == pagesToSwap.Count) 
        {
            selectedTab = tabButtons[0];
            OnTabClick(selectedTab);
        }
    }
    
    public void OnTabEnter(UITabButton button)
    {
        ResetTabs();
        if(selectedTab == null || button != selectedTab)
        {
            button.background.sprite = tabHover;
        }
    }

    public void OnTabClick(UITabButton button)
    {
        if (selectedTab != null)
        {
            selectedTab.Deselect();
        }
        selectedTab = button;
        selectedTab.Select();
        ResetTabs();
        button.background.sprite = tabActive;
        int index = button.transform.GetSiblingIndex();
        for (int i = 0; i < pagesToSwap.Count; i++)
        {
            if (i == index)
            {
                pagesToSwap[i].SetActive(true);
            }
            else pagesToSwap[i].SetActive(false);
        }
    }

    public void OnTabExit(UITabButton button)
    {
        ResetTabs();
    }

    public void ResetTabs()
    {
        foreach (UITabButton button in tabButtons)
        {
            if (selectedTab == button && selectedTab != null) continue;
            button.background.sprite = tabIdle;
        }
    }
}
