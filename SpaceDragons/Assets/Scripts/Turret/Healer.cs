﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : Turret
{
    [SerializeField] float healTimer = 0.25f;
    [SerializeField] float healAmount = 1.0f;
    float dt = 0f;

    Health turretImHealing = null;

    private void Start()
    {
        healAmount = (4 * ((int)turretRarity+1));
    }

    public override void Attack()
    {
        //This method is not used by this class.
    }

    void FixedUpdate()
    {
        dt += Time.deltaTime;

        if (turretImHealing == null)
        {
            turretImHealing = FindTurretToHeal();
        }

        if (dt >= healTimer)
        {
            Heal();
            dt = 0;
        }

        CheckForDie();
    }

    public void Heal()
    {
        if (Time.timeScale != 0)
        {
            if (turretImHealing.healthCount >= turretImHealing.healthMax)
            {
                turretImHealing = FindTurretToHeal();
            }

            turretImHealing.healthCount += healAmount * Time.deltaTime;
        }
    }

    public Health FindTurretToHeal()
    {
        List<GameObject> turretobjs = WorldManager.Instance.Ship.bodyPartObjects;
        Health turretToHeal = null;

        for(int i = 0; i < turretobjs.Count; i++)
        {
            Health health;
            if(!turretobjs[i])
            {
                continue;
            }

            if (turretobjs[i].TryGetComponent(out health))
            {
                if (turretToHeal == null)
                {
                    turretToHeal = turretobjs[i].GetComponent<Health>();
                }
                else if ((turretobjs[i].GetComponent<Health>().healthCount/ turretobjs[i].GetComponent<Health>().healthMax) < (turretToHeal.healthCount/turretToHeal.healthMax))
                {
                    turretToHeal = turretobjs[i].GetComponent<Health>();
                }
            }
        }

        return turretToHeal;
    }
}
