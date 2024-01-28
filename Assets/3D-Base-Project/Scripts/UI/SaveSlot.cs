using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] private string profileID = "";

    [Header("Content")]
    [SerializeField] private GameObject noDataContent;
    [SerializeField] private GameObject hasDataContent;

    [SerializeField] private TextMeshProUGUI percentageCompleteText;
    [SerializeField] private TextMeshProUGUI deathCountText;
    [Header("Delete Data Button")]
    [SerializeField] public Button deleteButton;
    public bool hasData { get; private set; } = false;
    public void SaveGame()
    {
        DataPersistenceManager.instance.SaveGame();
    }
    public void SetData(GameData data)
    {
        if(data == null)
        {
            hasData = false;
            noDataContent.SetActive(true);
            hasDataContent.SetActive(false);
            deleteButton.gameObject.SetActive(false);
        }
        else
        {
            hasData = true;
            noDataContent.SetActive(false);
            hasDataContent.SetActive(true);
            deleteButton.gameObject.SetActive(true);

            percentageCompleteText.text = data.GetPercentageComplete() + "% Complete";
            deathCountText.text = "Death Count: " + data.playerData.deathCount;
        }
    }
    public string GetProfileID()
    {
        return this.profileID;
    }
}
