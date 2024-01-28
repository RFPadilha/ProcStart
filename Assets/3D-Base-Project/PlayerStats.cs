using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IDataPersistence
{
    /*
     * Any additional player stats should be added to "PlayerData"
     * */
    [Header("SaveData")]
    public PlayerData player;

    //Data Manipulation--------------------------------------------------------------
    public void LevelUp()
    {
        player.level++;
        player.exp = 0;
    }
    public void GainExp(int expGain)
    {
        player.exp += expGain;
    }
    public void TakeDamage(int damage)
    {
        player.currentHealth -= damage;
    }
    public void LoadData(GameData data)
    {
        this.player = data.playerData;
    }
    public void SaveData(GameData data)
    {
        Debug.Log($"Player position ({player.position}) was saved as ({transform.position})");
        player.position[0] = transform.position.x;
        player.position[1] = transform.position.y;
        player.position[2] = transform.position.z;
        data.playerData = this.player;
    }

}
