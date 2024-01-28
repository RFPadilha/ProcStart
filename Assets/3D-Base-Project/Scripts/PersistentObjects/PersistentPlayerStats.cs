using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentPlayerStats : MonoBehaviour
{
    public int maxHealth = 50;
    public int currentHealth = 50;

    public int exp = 0;
    public int expToLevelUp = 100;
    public int level = 1;
    public float[] position = new float[3] { 0,0,0};
}
