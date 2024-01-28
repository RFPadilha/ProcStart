using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotsMenu : MonoBehaviour
{
    private SaveSlot[] saveSlots;
    private bool isLoadingGame = false;
    [SerializeField] private ConfirmationPopupMenu confirmationPopup;
    private void Awake()
    {
        saveSlots = this.GetComponentsInChildren<SaveSlot>();
    }
    public void OnSaveSlotClicked(SaveSlot saveSlot)
    {
        if (isLoadingGame)
        {
            DataPersistenceManager.instance.ChangeSelectedProfileID(saveSlot.GetProfileID());
            SaveGameAndLoadScene();
        }
        else if (saveSlot.hasData)
        {
            confirmationPopup.ActivateMenu("Starting a new game with this slot will override the currently saved data. Proceed anyway?",
                () =>//confirm action
                {
                    DataPersistenceManager.instance.ChangeSelectedProfileID(saveSlot.GetProfileID());
                    DataPersistenceManager.instance.NewGame();
                    SaveGameAndLoadScene();
                },
                () =>//cancel action
                {

                });
        }
        else
        {
            DataPersistenceManager.instance.ChangeSelectedProfileID(saveSlot.GetProfileID());
            DataPersistenceManager.instance.NewGame();
            SaveGameAndLoadScene();
        }
    }
    private void SaveGameAndLoadScene()
    {
        DataPersistenceManager.instance.SaveGame();
        Loader.Load(Loader.Scene.Game);
    }
    public void ActivateMenu(bool isLoadingGame)
    {
        this.isLoadingGame = isLoadingGame;
        Dictionary<string, GameData> profilesGameData = DataPersistenceManager.instance.GetAllProfilesGameData();

        foreach (SaveSlot saveSlot in saveSlots)
        {
            GameData profileData = null;
            profilesGameData.TryGetValue(saveSlot.GetProfileID(), out profileData);
            saveSlot.SetData(profileData);
            if(profileData == null && isLoadingGame)
            {
                SetSlotState(saveSlot, false);
            }
            else
            {
                SetSlotState(saveSlot, true);
            }
        }
    }
    private void SetSlotState(SaveSlot saveSlot, bool active)
    {
        saveSlot.GetComponent<Button>().interactable = active;
        saveSlot.deleteButton.interactable = active;
    }
    public void OnDeleteClicked(SaveSlot saveSlot)
    {
        confirmationPopup.ActivateMenu("Are you sure you want to delete this save file?",
                () =>//confirm action
                {
                    DataPersistenceManager.instance.DeleteProfileData(saveSlot.GetProfileID());
                    ActivateMenu(isLoadingGame);
                },
                () =>//cancel action
                {
                    ActivateMenu(isLoadingGame);
                });
    }
}
