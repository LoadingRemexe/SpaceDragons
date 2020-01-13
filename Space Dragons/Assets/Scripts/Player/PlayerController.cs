﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float attackSpeed = 0.25f;
    public float attackTimer = 0.0f;

    public int money = 100;
    public float attackDamage = 25.0f;
    public Inventory inventory = null;

    [SerializeField] GameObject head = null;
    [SerializeField] GameObject headBullet = null;
    [SerializeField] float bulletOffsetY = 1.0f;

    void Start()
    {
        inventory = GetComponent<Inventory>();
    }

    void FixedUpdate()
    {
        //shoot from head
        attackTimer += Time.deltaTime;

        if(attackTimer > attackSpeed)
        {
            GameObject projectileGO = (Instantiate(headBullet, head.transform.position + (bulletOffsetY * head.transform.up), Quaternion.identity, null) as GameObject);
            Projectile projectile = projectileGO.GetComponent<Projectile>();
            projectile.parentobj = head;
            projectile.damage = attackDamage;
            projectile.Fire();

            attackTimer = 0;
        }
    }

    public void AddMoney(int amount)
    {
        money += amount;
    }

    public bool RemoveMoney(int amount)
    {
        if(money - amount > 0)
        {
            money -= amount;
            return true;
        }

        return false;
    }
}
