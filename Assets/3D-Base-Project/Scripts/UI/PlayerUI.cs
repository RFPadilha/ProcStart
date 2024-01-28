using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public GameObject pauseMenuHolder;
    public GameObject optionsMenuHolder;

    public PlayerMovementScript player;
    //PersistentPlayerStats stats;

    bool isPaused = false;
    private void Start()
    {
        //stats = PersistentPlayerStats.instance;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isPaused)
        {
            PauseGame();
        }else if(Input.GetKeyDown(KeyCode.Escape) && isPaused)
        {
            Continue();
        }
    }
    public void Continue()
    {
        pauseMenuHolder.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }
    public void PauseGame()
    {
        Time.timeScale = 0f;
        pauseMenuHolder.SetActive(true);
        isPaused = true;
    }
    public void SaveGame()
    {
        //SaveSystem.SavePlayer(player.data);
    }
    
    public void LoadGame()
    {
        /*
        PlayerData data = SaveSystem.LoadPlayer();
        player.currentHealth = data.currentHealth;
        player.maxHealth = data.maxHealth;
        player.exp = data.exp;
        player.expToLevelUp = data.expToLevelUp;
        player.level = data.level;
        player.transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);
        */
    }
    /*
    public void OptionsMenu()
    {
        optionsMenuHolder.SetActive(true);
        pauseMenuHolder.SetActive(false);
    }
    public void ReturnFromOptionsMenu()
    {
        optionsMenuHolder.SetActive(false);
        pauseMenuHolder.SetActive(true);
    }
    */
    public void QuitToMenu()
    {
        Loader.Load(Loader.Scene.MainMenu);
    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Application ceased execution.");
    }
}
