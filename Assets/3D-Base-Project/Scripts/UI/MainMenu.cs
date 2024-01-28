using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public bool startAtMainMenu = true;
    [Header("Menu Navigation")]
    [SerializeField] private GameObject mainMenuHolder;
    [SerializeField] private Button firstSelectedMain;
    [SerializeField] private GameObject saveSlotsMenu;
    [SerializeField] private Button firstSelectedSaveSlots;
    [SerializeField] private GameObject optionsMenuHolder;
    [SerializeField] private Button firstSelectedOptions;

    [Header("MenuButtons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    private void OnEnable()
    {
        if (firstSelectedMain.interactable == false)
        {
            SetFirstSelected(newGameButton);
        }
        else
        {
            SetFirstSelected(firstSelectedMain);
        }
    }
    private void Start()
    {
        DisableButtonsDependingOnData();
        if (startAtMainMenu) 
        {
            StartAtMainMenu();
        }
    }
    public void DisableButtonsDependingOnData()
    {
        if (!DataPersistenceManager.instance.HasGameData())
        {
            //disable continue button
            continueButton.interactable = false;
            loadGameButton.interactable = false;
        }
    }
    public void SetFirstSelected(Button firstSelectedButton)
    {
        firstSelectedButton.Select();
    }
    //------------------------------------------------------------------
    public void Continue()
    {
        //Loads Latest Game
        DataPersistenceManager.instance.LoadGame();
        Loader.Load(Loader.Scene.Game);
    }
    //------------------------------------------------------------------
    public void NewGame()
    {
        ActivateSaveSlotsMenu(false);
    }
    //------------------------------------------------------------------
    public void LoadGame()
    {
        ActivateSaveSlotsMenu(true);
    }
    //------------------------------------------------------------------
    public void OptionsMenu()
    {
        optionsMenuHolder.SetActive(true);
        mainMenuHolder.SetActive(false);
        SetFirstSelected(firstSelectedOptions);
    }
    public void ReturnFromOptionsMenu()
    {
        ReturnToMainMenu(optionsMenuHolder);
    }
    //------------------------------------------------------------------
    public void ActivateSaveSlotsMenu(bool isLoadingGame)
    {
        saveSlotsMenu.SetActive(true);
        mainMenuHolder.SetActive(false);
        saveSlotsMenu.GetComponent<SaveSlotsMenu>().ActivateMenu(isLoadingGame);
        SetFirstSelected(firstSelectedSaveSlots);
    }
    public void ReturnFromSaveSlotsMenu()
    {
        ReturnToMainMenu(saveSlotsMenu);
    }
    //------------------------------------------------------------------
    private void ReturnToMainMenu(GameObject currentMenu)
    {
        currentMenu.SetActive(false);
        mainMenuHolder.SetActive(true);
        DisableButtonsDependingOnData();
        if (firstSelectedMain.interactable == false)
        {
            SetFirstSelected(newGameButton);
        }
        else
        {
            SetFirstSelected(firstSelectedMain);
        }
    }
    
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Application ceased execution.");
    }
    
    
    //Used for quick testing
    public void StartAtMainMenu()
    {
        mainMenuHolder.SetActive(true);
        optionsMenuHolder.SetActive(false);
        saveSlotsMenu.SetActive(false);
        if (firstSelectedMain.interactable == false)
        {
            SetFirstSelected(newGameButton);
        }
        else
        {
            SetFirstSelected(firstSelectedMain);
        }
    }
    
}
