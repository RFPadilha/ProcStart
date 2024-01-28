using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Profiling;

public class FileDataHandler
{
    private string dataDirPath = "";
    private string dataFileName = "";
    private bool useEncryption = false;
    private readonly string encryptionCodeWord = "capivara";
    private string backupExtension = ".bak";

    public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.useEncryption = useEncryption;
    }
    public GameData Load(string profileID, bool allowRestoreFromBackup = true)
    {
        if (profileID == null) return null;
        //Path.Combine guarantees the correct path on different OS's
        string fullPath = Path.Combine(dataDirPath, profileID, dataFileName);
        GameData loadedData = null;

        if (File.Exists(fullPath))
        {
            try
            {
                //load serialized data
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                //optionally decrypt data
                if (useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                //deserialize from JSON to C# object
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);

            }
            catch (Exception e)
            {
                if (allowRestoreFromBackup)
                {
                    Debug.LogWarning($"Failed to load data, attempting to roll back\n {e}");
                    bool rollbackSuccess = AttemptRollback(fullPath);
                    if (rollbackSuccess) loadedData = Load(profileID, false);

                }
                else
                {
                    Debug.LogError($"Error occured when loading file at path {fullPath}, and backup did not work \n{e}");
                }
            }
        }
        return loadedData;

    }
    public void Save(GameData data, string profileID)
    {
        if (profileID == null) return;
        //Path.Combine guarantees the correct path on different OS's
        string fullPath = Path.Combine(dataDirPath, profileID, dataFileName);
        string backupFilePath = fullPath + backupExtension;
        try
        {
            //creates directory if it doesnt exist yet
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            //serialize data as JSON
            string dataToStore = JsonUtility.ToJson(data, true);

            //optionally encrypt file data
            if (useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            //write data to file
            using(FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
            GameData verifiedData = Load(profileID);
            if (verifiedData != null)
            {
                File.Copy(fullPath, backupFilePath, true);
            }
            else throw new Exception("Save file could not be verified. Backup couldn't be created.");

        }
        catch (Exception e)
        {
            Debug.LogError("Error when trying to save data to file: " + fullPath + "\n" + e);
        }
    }
    public void Delete(string profileID)
    {
        if (profileID == null) return;
        string fullPath = Path.Combine(dataDirPath, profileID, dataFileName);
        try
        {
            if (File.Exists(fullPath))
            {
                Directory.Delete(Path.GetDirectoryName(fullPath), true);
            }
            else
            {
                Debug.LogWarning($"Tried to delete profile data, but no data exists at {fullPath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete profile data for {profileID} at {fullPath} + \n {e}");
        }

    }
    public Dictionary<string,GameData> LoadAllProfiles()
    {
        Dictionary<string, GameData> profileDictionary = new Dictionary<string,GameData>();

        //gets all directories in the dataDirPath
        IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(dataDirPath).EnumerateDirectories();
        foreach (DirectoryInfo dirInfo in dirInfos)
        {
            string profileID = dirInfo.Name;

            //skips file if it doesn't exist
            string fullPath = Path.Combine(dataDirPath, profileID, dataFileName);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"Directory {profileID} doesn't contain save data, skipping...");
                continue;
            }
            GameData profileData = Load(profileID);
            if(profileData != null)
            {
                profileDictionary.Add(profileID, profileData);
            }
            else
            {
                Debug.LogError($"Unexpected error happened loading profileID: {profileID}");
            }
            //load and place in dictionary
        }


        return profileDictionary;
    }
    public string GetMostRecentlyUpdatedProfileID()
    {
        string mostRecentProfileID = null;
        Dictionary<string, GameData> profilesGameData = LoadAllProfiles();
        foreach (KeyValuePair<string, GameData> pair in profilesGameData)
        {
            string profileID = pair.Key;
            GameData gameData = pair.Value;

            if (gameData == null) 
            {
                continue;
            }

            if(mostRecentProfileID == null)
            {
                mostRecentProfileID = profileID;
            }
            else
            {
                DateTime mostRecentDateTimeSoFar = DateTime.FromBinary(profilesGameData[mostRecentProfileID].lastUpdated);
                DateTime thisIterationDateTime = DateTime.FromBinary(gameData.lastUpdated);
                if (thisIterationDateTime > mostRecentDateTimeSoFar)
                {
                    mostRecentProfileID = profileID;
                }
            }
        }
        return mostRecentProfileID;
    }

    //simple implementation of XOR encryption
    private string EncryptDecrypt(string data)
    {
        string modifiedData = "";
        for (int i = 0; i < data.Length; i++)
        {
            modifiedData += (char)(data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
        }
        return modifiedData;
    }
    private bool AttemptRollback(string fullpath)
    {
        bool success = false;

        string backupFilePath = fullpath + backupExtension;
        try
        {
            if (File.Exists(backupFilePath))
            {
                File.Copy(backupFilePath, fullpath, true);
                success = true;
                Debug.LogWarning($"Had to roll back to backup file at {backupFilePath}");
            }
            else
            {
                throw new Exception("Tried to roll back, but no backup file exists to roll back to.");
            }
        }catch(Exception e)
        {
            Debug.LogError($"Error occured when trying to roll back to backup file at {backupFilePath} : \n{e}");
        }



        return success;
    }
}

