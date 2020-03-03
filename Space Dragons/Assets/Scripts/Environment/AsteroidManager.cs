﻿using System.Collections.Generic;
using UnityEngine;

public class AsteroidManager : Singleton<AsteroidManager>
{
    [SerializeField] int ClusterNum = 100;
    [SerializeField] int AsteroidMinimum = 2;
    [SerializeField] int AsteroidMaximum = 5;

    public int AsteroidsDestroyed = 0;

    WorldManager worldManager;
    void Start()
    {
        worldManager = WorldManager.Instance;

        for (int i = 0; i < ClusterNum; i++)
        {
            Vector3 location = new Vector3(Random.Range(-150, 150), Random.Range(-150, 150), 0);
            for (int j = 0; j < Random.Range(AsteroidMinimum, AsteroidMaximum); j++)
            {
                GameObject asteroid = worldManager.SpawnFromPool("Asteroids", location + new Vector3(Random.value, Random.value, 0), Quaternion.identity);

                if (Vector3.Distance(asteroid.transform.position, worldManager.Head.transform.position) < 50)
                {
                    worldManager.AsteroidsToRender.Add(asteroid);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        float val = Random.Range(-1, 1);
        float val2 = Random.Range(-1, 1);
        if (val < 0)
        {
            val = -1;
        }
        else
        {
            val = 1;
        }
        if (val2 < 0)
        {
            val2 = -1;
        }
        else
        {
            val2 = 1;
        }
        Vector3 location = new Vector3(Random.Range(50, 100) * val, Random.Range(50, 100) * val2, 0);

        if (AsteroidsDestroyed > 8)
        {
            location += worldManager.Head.transform.position;
            for (int j = 0; j < Random.Range(AsteroidMinimum, AsteroidMaximum); j++)
            {
                worldManager.SpawnFromPool("Asteroids", location, Quaternion.identity);
            }
            AsteroidsDestroyed = 0;
        }

        foreach (GameObject asteroid in worldManager.objectPools["Asteroids"])
        {
            if (Vector3.Distance(asteroid.transform.position, worldManager.transform.position) > 150)
            {
                asteroid.transform.position = location;
            }
        }
    }
}
