using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] private bool initializeDataIfNull = false;
    [SerializeField] private bool disableDataPersistence = false;
    [SerializeField] private bool overrideSelectedProfileID = false;
    [SerializeField] private string testSelectedProfileID = "test";

    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;
    public static DataPersistenceManager instance { get; private set; }
    private string selectedProfileID = "";
    public GameData gameData { get; private set; }
    private List<IDataPersistence> dataPersistenceObjects;//references all objects that implement IDataPersistence interface
    private FileDataHandler dataHandler;

    [Header("Autosave Configuration")]
    [SerializeField] private bool autosaveEnabled = true;
    [SerializeField] private float autosaveTimeSeconds = 300f;//5min
    private Coroutine autosaveCoroutine;

    private void Awake()
    {
        if(instance != null)
        {
            Debug.Log("More than one data persistence manager in the scene, destroying newest");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        if(disableDataPersistence)
        {
            Debug.LogWarning("Data persistence is disabled!");
        }
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);
        InitializeSelectedProfileID();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
        if (autosaveCoroutine != null) StopCoroutine(autosaveCoroutine);
        autosaveCoroutine = StartCoroutine(AutoSave());
    }
    public void ChangeSelectedProfileID(string newProfileID)
    {
        this.selectedProfileID = newProfileID;
        LoadGame();
    }
    public void NewGame()
    {
        this.gameData = new GameData();
        gameData.seed = Random.Range(1, 999999999);
    }

    public void LoadGame()
    {
        if (disableDataPersistence) return;
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        //load saved data using data handler
        this.gameData = dataHandler.Load(selectedProfileID);

        if(this.gameData == null && initializeDataIfNull)
        {
            NewGame();
        }else if (this.gameData == null)
        {
            //if no data is found, can't continue
            Debug.Log("No game data found. New Game must be started before loading");
            return;
        }


        //pushes loaded data into all scripts that need it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
        Debug.Log($"Loaded Data Successfully from {selectedProfileID}");
    }

    public void SaveGame()
    {

        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        if (disableDataPersistence) return;
        if (this.gameData == null)
        {
            Debug.LogWarning("No data found. New game must be started before saving data");
            return;
        }
        //passes data into all scripts so they can update it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(gameData);
        }
        //timestamps data
        gameData.lastUpdated = System.DateTime.Now.ToBinary();
        //save data using dataHandler
        dataHandler.Save(gameData, selectedProfileID);
        Debug.Log($"Saved Data Successfully to {selectedProfileID}");

    }
    public void UpdateSeed(int seed)
    {
        gameData.seed = seed;
    }
    public int GetSeed()
    {
        return gameData.seed;
    }

    public void DeleteProfileData(string profileID)
    {
        dataHandler.Delete(profileID);
        InitializeSelectedProfileID();
        LoadGame();
    }
    public void InitializeSelectedProfileID()
    {
        this.selectedProfileID = dataHandler.GetMostRecentlyUpdatedProfileID();
        if (overrideSelectedProfileID)
        {
            this.selectedProfileID = testSelectedProfileID;
            Debug.LogWarning($"Override selected profile id with ID: {this.selectedProfileID}");
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        //scripts to be found must also extend from monobehaviour, "true" includes objects disabled via hierarchy
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>(true)
            .OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }
    public bool HasGameData()
    {
        return this.gameData != null;
    }
    public Dictionary<string, GameData> GetAllProfilesGameData()
    {
        return dataHandler.LoadAllProfiles();
    }
    private IEnumerator AutoSave()
    {
        while (autosaveEnabled)
        {
            yield return new WaitForSeconds(autosaveTimeSeconds);
            SaveGame();
            Debug.Log($"Autosaved game at {selectedProfileID}");
        }
    }
}
