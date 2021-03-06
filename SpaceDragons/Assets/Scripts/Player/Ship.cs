﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Ship : MonoBehaviour
{
    #region Enums
    public enum eMotherShip
    {
        BASIC = 0,
        FLAMETHROWER = 1,
        LIGHTNING = 2,
        HEALING = 3,
        GUARD_DRONE = 4,
    }
    #endregion

    #region Variables

    public Camera cam = null;
    bool isDead = false;
    PlayerController controller;

    [Header("Body Info")]
    public List<Transform> bodyPartTransforms = new List<Transform>(5);
    public List<GameObject> bodyPartObjects = new List<GameObject>(5);
    public GameObject[] bodyPartPrefabs = null;

    [Header("Head Info")]
    public GameObject head = null;
    public Sprite[] ShipHeadSprites;
    public SpriteRenderer ShipHeadSprite = null;
    public Health headHealth = null;

    [Header("Snake Info")]
    public float minDst = 1.0f;
    public int startSize = 2;
    public int maxShipsAllowed = 4;

    [Header("Movement Info")]
    public float speed = 5f;
    public float defaultSpeed = 7.5f;
    public float driftSpeed = 5f;
    public float rotationSpeed = 50.0f;
    public bool thrustersOn = true;

    public VJHandler joystickMovement;

    private bool joystickdragging = false;

    [Header("Control UI")]
    [SerializeField] RectTransform joystickknob;

    [Header("Enum Info")]
    public eMotherShip motherShip = eMotherShip.BASIC;

    private float dst = 1.0f;
    private Transform curBodyPart = null;
    private Transform prevBodyPart = null;

    [Header("Boost Info")]
    [SerializeField] float boostSpeed = 0f;
    [SerializeField] float boostrotateSpeed = 0f;
    [SerializeField] float returnrotateSpeed = 0f;
    [SerializeField] bool boosting = false;
    [SerializeField] public int boostFuel = 0;
    [SerializeField] public int boostFuelMAX = 4;
    [SerializeField] GameObject boostParticles;
    [SerializeField] Slider boostSliderJoystick;
    [SerializeField] Slider boostSliderTouch;
    [SerializeField] float boostCooldownReset = 0f;
    [SerializeField] float boostCooldownTimer = 0f;
    #endregion

    private void Start()
    {
        if (!headHealth)
        {
            headHealth = head.GetComponent<Health>();
        }

        controller = WorldManager.Instance.PlayerController;

        bodyPartObjects.Add(bodyPartTransforms[0].gameObject);
        LoadData();
        speed = defaultSpeed;
        returnrotateSpeed = rotationSpeed;
        switch (motherShip)
        {
            case eMotherShip.BASIC:
                SetShipHead(0);
                break;
            case eMotherShip.FLAMETHROWER:
                SetShipHead(1);
                break;
            case eMotherShip.LIGHTNING:
                SetShipHead(2);
                break;
            case eMotherShip.HEALING:
                SetShipHead(3);
                break;
            case eMotherShip.GUARD_DRONE:
                SetShipHead(4);
                break;
        }
    }

    private void LoadData()
    {
        motherShip = LoadManager.Instance.saveData.motherShipType;
        SetShipHead((int)motherShip);
        headHealth.healthCount = LoadManager.Instance.saveData.PlayerHealth;
        boostFuelMAX = LoadManager.Instance.saveData.PlayerFuelMax;
        boostFuel = LoadManager.Instance.saveData.PlayerFuelCurrent;
        LoadManager.ShipDataSavable[] ships = LoadManager.Instance.saveData.Ships;
        if (ships != null)
        {
            for (int i = 0; i < ships.Length; i++)
            {
                if (ships[i].prefabName != null)
                {
                    GameObject ship = FindBodyPartFromPrefabs(ships[i].prefabName);
                    Turret shipTurret = ship.GetComponent<Turret>();
                    shipTurret.turretRarity = ships[i].rarity;
                    shipTurret.Initialize();
                    ship.GetComponent<Health>().healthCount = ships[i].shipHealth;
                    AddBodyPart(ship);
                }
            }
        }
        foreach (Transform t in bodyPartTransforms)
        {
            t.position = LoadManager.Instance.saveData.PlayerPosition.ToVector();
        }
    }

    private void FixedUpdate()
    {
        CalculateSpeed();

        if (PlayerPrefs.GetInt("JoystickControls") == 0)
        {
            AdjustJoystick();
            MoveWithJoystick();
        }
        else
        {
            Move();
        }

        UpdateFeulGauge();
        StartCoroutine("CheckForDie");
    }

    #region Movement

    public void ActivateThrusters(bool isOn)
    {
        thrustersOn = isOn;
    }

    public void CalculateSpeed()
    {
        if (boosting)
        {
            AndroidManager.HapticFeedback();

            boostCooldownTimer -= Time.deltaTime;

            if (boostCooldownTimer < 0.0f)
            {
                boostParticles.SetActive(false);
                rotationSpeed = returnrotateSpeed;
                boosting = false;
            }
        }
        else if (!thrustersOn)
        {
            speed = Mathf.Lerp(speed, 0, Time.deltaTime);
        }
        else
        {
            if (PlayerPrefs.GetInt("JoystickControls") == 0)
            {
                if (joystickdragging)
                {
                    speed = Mathf.Lerp(speed, defaultSpeed, Time.deltaTime);
                }
                else
                {
                    speed = Mathf.Lerp(speed, driftSpeed, Time.deltaTime);
                }

            }
            else
            {
                if (Input.touchCount > 0)
                {
                    if (!UIManager.Instance.IsPointerOverUIObject())
                    {
                        speed = Mathf.Lerp(speed, defaultSpeed, Time.deltaTime);
                    }
                }
                else
                {
                    speed = Mathf.Lerp(speed, driftSpeed, Time.deltaTime);
                }
            }
        }
    }

    public void onPressJoystick()
    {
        joystickdragging = (true);
    }

    public void onReleaseJoystick()
    {
        joystickdragging = (false);
    }

    public void AdjustJoystick()
    {
        if (joystickdragging)
        {
            joystickknob.anchoredPosition = Vector2.Lerp(joystickknob.anchoredPosition, joystickknob.anchoredPosition + Input.GetTouch(0).deltaPosition, .25f);
            joystickknob.anchoredPosition = Vector2.ClampMagnitude(joystickknob.anchoredPosition, 150.0f);
        }
        else
        {
            joystickknob.anchoredPosition = Vector2.Lerp(joystickknob.anchoredPosition, new Vector2(0, 0), 5 * Time.deltaTime);
        }
    }

    public void MoveWithJoystick()
    {
        if (joystickdragging)
        {
            Vector3 targetPos = joystickknob.anchoredPosition;
            targetPos.z = 0;

            Vector3 direction = joystickMovement.InputDirection;
            float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(-angle, Vector3.forward);
            bodyPartTransforms[0].rotation = Quaternion.Slerp(bodyPartTransforms[0].rotation, rotation, rotationSpeed * Time.deltaTime);
        }

        bodyPartTransforms[0].Translate(bodyPartTransforms[0].up * speed * Time.smoothDeltaTime, Space.World);

        for (int i = 1; i < bodyPartTransforms.Count; i++)
        {
            if (bodyPartTransforms[i] != null)
            {
                curBodyPart = bodyPartTransforms[i];
                prevBodyPart = bodyPartTransforms[i - 1];

                dst = Vector3.Distance(prevBodyPart.position, curBodyPart.position);

                Vector3 newPos = prevBodyPart.position;
                newPos.z = bodyPartTransforms[0].position.z;

                float t = Time.deltaTime * dst / minDst * speed;
                if (t > .5f)
                {
                    t = 0.5f;
                }
                curBodyPart.position = Vector3.Slerp(curBodyPart.position, newPos, t);
                curBodyPart.rotation = Quaternion.Slerp(curBodyPart.rotation, prevBodyPart.rotation, t);
            }
        }
    }

    public void Move()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (!UIManager.Instance.IsPointerOverUIObject())
            {
                Vector3 targetPos = Camera.main.ScreenToWorldPoint(touch.position);
                targetPos.z = 0;

                Vector3 direction = targetPos - bodyPartTransforms[0].transform.position;
                float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.AngleAxis(-angle, Vector3.forward);
                bodyPartTransforms[0].rotation = Quaternion.Slerp(bodyPartTransforms[0].rotation, rotation, rotationSpeed * Time.deltaTime);
            }
        }

        bodyPartTransforms[0].Translate(bodyPartTransforms[0].up * speed * Time.smoothDeltaTime, Space.World);

        for (int i = 1; i < bodyPartTransforms.Count; i++)
        {
            if (bodyPartTransforms[i] != null)
            {

                curBodyPart = bodyPartTransforms[i];
                prevBodyPart = bodyPartTransforms[i - 1];

                dst = Vector3.Distance(prevBodyPart.position, curBodyPart.position);

                Vector3 newPos = prevBodyPart.position;
                newPos.z = bodyPartTransforms[0].position.z;

                float t = Time.deltaTime * dst / minDst * speed;

                if (t > .5f)
                {
                    t = 0.5f;
                }

                curBodyPart.GetComponent<Turret>().travelDirection = (newPos - transform.position).normalized;

                curBodyPart.position = Vector3.Slerp(curBodyPart.position, newPos, t);
                curBodyPart.rotation = Quaternion.Slerp(curBodyPart.rotation, prevBodyPart.rotation, t);
            }
        }
    }
    #endregion

    #region Boost and Fuel
    public void Boost()
    {
        if (boostFuel > 0 && !boosting)
        {
            boostFuel--;
            boostCooldownTimer = boostCooldownReset;
            rotationSpeed = boostrotateSpeed;
            speed = boostSpeed;
            boosting = true;
            boostParticles.SetActive(true);
        }
    }

    public void RefillBoost()
    {
        boostFuel = boostFuelMAX;
    }

    private void UpdateFeulGauge()
    {
        float min = 23;
        float max = 64;
        float scale = max - min;
        float gauge = scale * ((float)boostFuel / (float)boostFuelMAX);
        boostSliderJoystick.value = Mathf.Lerp(boostSliderJoystick.value, min + gauge, 1.5f * Time.deltaTime);

        boostSliderTouch.maxValue = boostFuelMAX;
        boostSliderTouch.value = Mathf.Lerp(boostSliderTouch.value, boostFuel, 1.5f * Time.deltaTime);
    }
    #endregion

    #region Body Parts
    int index = 0;

    public void AddBodyPart(GameObject bodyPart)
    {
        Transform newPart = Instantiate(bodyPart, bodyPartTransforms[bodyPartTransforms.Count - 1].position, bodyPartTransforms[bodyPartTransforms.Count - 1].rotation, transform).transform;
        index++;

        newPart.SetParent(transform);
        bodyPartTransforms.Add(newPart);
        bodyPartObjects.Add(newPart.gameObject);

        HealthBarManager.Instance.CreateAllHealthBars();
    }

    public GameObject FindBodyPartFromPrefabs(string partName)
    {
        GameObject bPart = null;

        for(int i = 0; i < bodyPartPrefabs.Length; i++)
        {
            if (bodyPartPrefabs[i].name.Equals(partName))
            {
                bPart = bodyPartPrefabs[i];
                break;
            }
        }

        return bPart;
    }

    public void RemoveBodyPart(GameObject bodyPart, bool selling)
    {
        int removeIndex = 0;
        for (int i = 0; i < bodyPartObjects.Count; i++)
        {
            if (bodyPartObjects[i] == bodyPart)
            {
                removeIndex = i;
            }
        }

        if (!selling)
        {
            if (removeIndex != 0)
            {
                int partCount = bodyPartObjects.Count;

                for (int j = partCount; j > removeIndex; j--)
                {
                    Destroy(bodyPartObjects[j - 1]);
                    bodyPartObjects.RemoveAt(j - 1);
                    bodyPartTransforms.RemoveAt(j - 1);
                    HealthBarManager.Instance.CreateAllHealthBars();
                }
            }
        }
        else
        {
            Destroy(bodyPartObjects[removeIndex]);
            bodyPartObjects.RemoveAt(removeIndex);
            bodyPartTransforms.RemoveAt(removeIndex);
            HealthBarManager.Instance.CreateAllHealthBars();
        }


    }

    public void SetShipHead(int val)
    {
        switch (val)
        {
            case 0:
                motherShip = eMotherShip.BASIC;
                break;
            case 1:
                motherShip = eMotherShip.FLAMETHROWER;
                break;
            case 2:
                motherShip = eMotherShip.LIGHTNING;
                break;
            case 3:
                motherShip = eMotherShip.HEALING;
                break;
            case 4:
                motherShip = eMotherShip.GUARD_DRONE;
                break;
        }
        PlayerController playerController = GetComponent<PlayerController>();
        playerController.SwitchFireMode(motherShip);
        ShipHeadSprite.sprite = ShipHeadSprites[val];
    }

    public void SortBody()
    {
        for (int i = 1; i < bodyPartObjects.Count; i++)
        {
            if (bodyPartObjects[i - 1] == null)
            {
                bodyPartObjects[i - 1] = bodyPartObjects[i];
                bodyPartObjects[i] = null;
            }
        }
        for (int i = 0; i < bodyPartObjects.Count; i++)
        {
            if (bodyPartObjects[i] != null)
            {
                bodyPartTransforms[i] = bodyPartObjects[i].transform;
            }
        }

        HealthBarManager.Instance.CreateAllHealthBars();
    }
    #endregion

    IEnumerator CheckForDie()
    {
        if (headHealth.healthCount <= 0)
        {
            AndroidManager.HapticFeedback();
            if (!isDead)
            {
                isDead = true;
                LoadManager.Instance.Save();
                LoadingScreen.Instance.Show(SceneManager.LoadSceneAsync("Death"));
            }
        }

        yield return new WaitForFixedUpdate();
    }
}
