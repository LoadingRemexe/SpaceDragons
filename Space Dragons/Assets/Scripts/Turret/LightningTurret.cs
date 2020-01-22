﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningTurret : Turret
{
    public float chainDistance = 15.0f;

    List<Enemy> shockedBois = new List<Enemy>();

    void FixedUpdate()
    {
        if (enemies.Count > 0)
        {
            RotateTurret();
        }
        CheckForDie();
    }

    public void RotateTurret()
    {
        Enemy enemy = enemies.Peek();
        if (enemy)
        {
            Vector3 direction = enemy.transform.position - rotateBoi.gameObject.transform.position;
            float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            if (angle < 15)
            {
                ShockNext(enemy);
            }
            Quaternion rotation = Quaternion.AngleAxis(-angle, Vector3.forward);
            rotateBoi.gameObject.transform.rotation = Quaternion.Slerp(rotateBoi.gameObject.transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            enemies.Dequeue();
        }
    }

    public override void Attack()
    {
        if (enemies.Count > 0)
        {
            if(enemies.Peek() != null)
            {
                ShockNext(enemies.Peek());
            }
        }
    }

    public void ShockNext(Enemy enemy)
    {
        Collider2D[] enemyColliders = Physics2D.OverlapCircleAll(enemy.transform.position, chainDistance);
        foreach (Collider2D col in enemyColliders)
        {
            Enemy en = null;
            col.TryGetComponent(out en);
            if (en != null)
            {
                if (shockedBois.Contains(en))
                {
                    return;
                }
                else
                {
                    shockedBois.Add(en);
                    ShockNext(en);
                }
                en.GetComponent<Health>().healthCount -= damage;
            }
        }
    }
}