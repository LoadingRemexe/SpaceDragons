﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{

    AudioSource explosionSFX;

    public void Start()
    {
        AudioManager.Instance.Play("Explosion01");
    }

    public float damage = 1.0f;
    void Destoyself()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Health hp = collision.gameObject.GetComponent<Health>();
        if (hp)
        {
            hp.DealDamage(damage);
        }
    }
}
