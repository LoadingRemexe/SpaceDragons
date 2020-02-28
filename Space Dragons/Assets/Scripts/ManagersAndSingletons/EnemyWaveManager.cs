﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyWaveManager : Singleton<EnemyWaveManager>
{
    [SerializeField] float waveSpawnTimer = 25.0f;

    [SerializeField] List<GameObject> Bosses = new List<GameObject>();
    [SerializeField] List<Wave> waves = new List<Wave>();
    [SerializeField] TextMeshProUGUI WaveText = null;
    [SerializeField] TextMeshProUGUI EnemiesText = null;

    public GameObject Player = null;

    public int cycleCount = 1;
    public int currentWave = 0;
    public int aliveEnemies = 0;

    float dt = 0.0f;

    void Start()
    {
        Player = WorldManager.Instance.Head;
        currentWave = LoadManager.Instance.saveData.CurrentWave;
        cycleCount = LoadManager.Instance.saveData.CurrentCycle;

        for (int i = 0; i < cycleCount; i++)
        {
            waves[currentWave].StartWave();
        }
        dt = 0.0f;
    }

    void FixedUpdate()
    {
        if(Input.GetKeyDown(KeyCode.F7))
        {
            currentWave = 9;
        }

        dt += Time.deltaTime;

        if (aliveEnemies == 0)
        {
            if (dt >= waveSpawnTimer)
            {
                for (int i = 0; i < cycleCount; i++)
                {
                    waves[currentWave].StartWave();
                }
                dt = 0.0f;

                if (currentWave == 9)
                {
                    for (int i = 0; i < cycleCount; i++)
                    {
                        SpawnRandomBoss();
                        Debug.Log("Spawned Boss");
                    }
                    currentWave = 11;
                }
                else
                {
                    currentWave++;
                }
            }
        }

        if(currentWave > 9)
        {
            cycleCount++;
            currentWave = 1;
        }

        WaveText.text = "Wave: " + ((currentWave+1) + ((cycleCount-1) * 10));
        EnemiesText.text = "Enemies Alive: " + aliveEnemies;
    }

    public void SpawnRandomBoss()
    {
        Vector3 newlocation = new Vector3(Random.Range(30.0f, 50.0f), Random.Range(30.0f, 50.0f), 0);
        newlocation.x *= Random.Range(-1, 1);
        newlocation.y *= Random.Range(-1, 1);
        Vector3 spawnPosition = new Vector3(Player.transform.position.x + newlocation.x, Player.transform.position.y + newlocation.y, 0.0f);

        Instantiate(Bosses[Random.Range(0, Bosses.Count)], spawnPosition, Quaternion.identity, null);
         aliveEnemies++;
    }

    public void ReduceCycleCount(int amount)
    {
        if (cycleCount - amount > 0)
        {
            cycleCount -= amount;
        }
        else
        {
            cycleCount = 0;
        }
    }

    public void IncreaseCycleCount(int amount)
    {
        cycleCount += amount;
    }
}
