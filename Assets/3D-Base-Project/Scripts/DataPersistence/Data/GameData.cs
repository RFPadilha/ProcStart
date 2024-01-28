using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public long lastUpdated;
    public int seed;
    public PlayerData playerData;
    public SerializableDictionary<string, bool> collectedObjects;

    //initial values when theres no data to load
    public GameData()
    {
        this.playerData = new PlayerData();
        collectedObjects = new SerializableDictionary<string, bool>();
    }
    public int GetPercentageComplete()
    {
        //calculate percentage of collectibles collected
        int totalCollected = 0;
        foreach (bool collected in collectedObjects.Values)
        {
            if (collected) totalCollected++;
        }
        int percentageCompleted = -1;
        if(collectedObjects.Count != 0)
        {
            percentageCompleted = (totalCollected*100)/collectedObjects.Count;
        }
        if(percentageCompleted < 0)
        {
            percentageCompleted = 0;
        }
        return percentageCompleted;
    }
}
