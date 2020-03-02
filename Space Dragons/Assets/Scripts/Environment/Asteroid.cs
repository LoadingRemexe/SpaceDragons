﻿using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] List<Sprite> asteroidImages = null;

    public ItemObject itemPrefab = null;
    public GameObject asteroidSmaller = null;

    public float sizeAndWeight = 1;
    public float maxHp = 50.0f;
    Rigidbody2D rb;
    SpriteRenderer sr;
    Health hp;

    public void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = asteroidImages[Random.Range(0, asteroidImages.Count)];

        hp = GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
        setSizeAndWeight(sizeAndWeight);

        Vector2 randomForce = new Vector2(Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f)); // Sends them in any random direction
        rb.AddForce(randomForce, ForceMode2D.Force);
        rb.AddTorque(Random.value *15, ForceMode2D.Force);

    }

    public void setSizeAndWeight(float sizeweight)
    {
        sizeAndWeight = sizeweight;
        hp.healthMax = maxHp/2 * sizeweight;
        hp.ResetHealth();
        transform.localScale = new Vector3(sizeweight, sizeweight, 1);
        rb.mass = sizeweight;
    }

    public void FixedUpdate()
    {
        if (hp.healthCount < 0.0f)
        {
            KillAsteroid();
        }
    }

    public void KillAsteroid()
    {
        if (itemPrefab)
        {
            ItemObject g = Instantiate(itemPrefab, transform.position, transform.rotation, null); // drops item in world space
            g.itemData = WorldManager.Instance.GetRandomItemDataStepped();
            g.image.sprite = g.itemData.itemImage;
        }
        hp.healthCount = hp.healthMax;
        gameObject.SetActive(false);
        WorldManager.Instance.AsteroidsToRender.Remove(gameObject);
    }
}

