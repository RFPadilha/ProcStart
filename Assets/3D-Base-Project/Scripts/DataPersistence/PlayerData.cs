using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData{
    public int maxHealth;
    public int currentHealth;

    public int exp;
    public int expToLevelUp;
    public int level;
    public float[] position;
    public int deathCount;

    public PlayerData()
    {
        this.maxHealth = 10;
        this.currentHealth = 10;

        this.exp = 0;
        this.expToLevelUp = 100;
        this.level = 1;

        this.position = new float[3];
        this.position[0] = 0;
        this.position[1] = 0;
        this.position[2] = 0;
        deathCount = 0;
    }

    public PlayerData (int _maxHealth, int _currentHealth, int _exp, int _expToLevelUp, int _level, float[] _position, int _deathCount)
    {
        maxHealth = _maxHealth;
        currentHealth = _currentHealth;

        exp = _exp;
        expToLevelUp = _expToLevelUp;
        level = _level;

        position = new float[3];
        position[0] = _position[0];
        position[1] = _position[1];
        position[2] = _position[2];
        deathCount = _deathCount;
    }
}
