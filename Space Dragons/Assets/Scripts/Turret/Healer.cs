﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : Turret
{
    [SerializeField] float healTimer = 0.25f;
    [SerializeField] float healAmount = 1.0f;
    float dt = 0f;

    Health turretImHealing = null;

    public override void Attack()
    {
        //This method is not used by this class.
    }

    void FixedUpdate()
    {
        dt += Time.deltaTime;

        if(turretImHealing == null)
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
        if (turretImHealing.healthCount == turretImHealing.healthMax)
        {
            turretImHealing = FindTurretToHeal();
        }

        turretImHealing.healthCount += healAmount;
    }

    public Health FindTurretToHeal()
    {
        List<GameObject> turretobjs = WorldManager.Instance.Ship.bodyPartObjects;
        Health turretToHeal = null;

        foreach (GameObject obj in turretobjs)
        {
            if (turretToHeal == null)
            {
                turretToHeal = obj.GetComponent<Health>();
            }
            else if (obj.GetComponent<Health>().healthCount < turretToHeal.healthCount)
            {
                turretToHeal = obj.GetComponent<Health>();
            }
        }

        return turretToHeal;
    }
}