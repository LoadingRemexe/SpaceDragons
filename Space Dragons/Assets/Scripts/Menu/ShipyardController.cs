﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.UI.Extensions;

public class ShipyardController : MonoBehaviour
{
    public GameObject Shipyard;
    public Ship MotherShip;
    public List<GameObject> Ships;
    public List<GameObject> ShopShips;
    public List<ShipData> CommonShips;
    public List<ShipData> RareShips;
    public List<ShipData> EpicShips;
    public List<MothershipData> Motherships;
    [Range(0, 2)] public int ShopDifficulty;
    public GameObject ShipScrollContent;
    public GameObject ShopShipScrollContent;
    public GameObject ShipButtonPrefab;
    public GameObject ShipMenu;
    public GameObject ShopMenu;
    public GameObject MothershipMenu;
    public GameObject MothershipDisplay;
    public GameObject SelectionDisplay;
    public TextMeshProUGUI ShipCounter;
    public TextMeshProUGUI ShopTimer;
    public TextMeshProUGUI MoneyNum;
    public GameObject MaxShipWarning;
    public List<GameObject> SelectionInfoPanels;
    public List<GameObject> SelectionInfoButtons;
    public Ship.eMotherShip TradeInMothership;
    public Ship.eMotherShip CurrentMothership;
    public GameObject TradeInButton;
    public ScrollSnap scrollSnap;
    public Sprite EmptySlot;

    //public int selectedPurchase = 0;
    int NumOfShips;
    float Timer = 0;
    float MaxTime = 300;
    Button buyButton = null;
    Button sellButton = null;
    Slider MotherHealthBar;

    int num = 0;

    public void Start()
    {
        MotherShip = WorldManager.Instance.Ship;
        ShipyardShipSetup();
        ShipyardShopSetup();
        MothershipPanelSwap(true);
        ShipyardMotherSetup((int)CurrentMothership, false);
        ShipMenu.SetActive(false);
        ShopMenu.SetActive(false);
        SelectionDisplay.SetActive(false);
        buyButton = SelectionDisplay.GetComponentsInChildren<Button>().Where
            (o => o.name == "Buy").FirstOrDefault();
        sellButton = SelectionDisplay.GetComponentsInChildren<Button>().Where
            (o => o.name == "Sell").FirstOrDefault();        

        Shipyard.SetActive(false);

    }

    public void Update()
    {
        #region Dev Debug Controls
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Timer = 5;
        }
        if (Input.GetKeyDown(KeyCode.F1))
        {
            OpenShop();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
        #endregion

        if (Timer > 0)
        {
            int minutes = (int)Timer / 60;
            int seconds = (int)Timer % 60;
            ShopTimer.text = minutes.ToString("00") + ":" + seconds.ToString("00");
            Timer -= 1 * Time.unscaledDeltaTime;
        }
        else if (Timer <= 0)
        {
            ShopShips = new List<GameObject>();
            ShipyardShopSetup();
            int num = 0;
            for (int i = 0; i < SelectionInfoPanels.Count; i++)
            {
                if (SelectionInfoPanels[i].activeInHierarchy)
                {
                    num = i;
                    break;
                }
            }

            if(ShopMenu.activeInHierarchy)
            {
                OpenSelectedPanel(0);
                GetSelectionInfo(true, ShopShips[scrollSnap.CurrentPage()]);
                CheckIfSpecial(ShopShips[scrollSnap.CurrentPage() + 1]);
                OpenSelectedPanel(num);
            }
        }

        ShipCounter.text = NumOfShips + "/" + Ships.Count;

        int money = MotherShip.GetComponent<PlayerController>().money;
        int switchCase = 0;

        switchCase = money < 10000 ? 0
            : money >= 10000 && money < 100000 ? 1
            : money >= 100000 && money < 1000000 ? 2
            : money >= 1000000 && money < 1000000000 ? 3
            : 4;

        switch (switchCase)
        {
            case 0:
                MoneyNum.text = money.ToString();
                break;
            case 1:
                int thousands = money / 1000;
                int hundreds = money % 1000;
                char[] hundie = { '0' };
                if (hundreds >= 100)
                {
                     hundie = hundreds.ToString().ToCharArray();
                }
                MoneyNum.text = thousands.ToString() + "." + hundie[0] + "k";
                break;
            case 2:
                thousands = money / 1000;
                MoneyNum.text = thousands.ToString() + "k";
                break;
            case 3:
                int millions = money / 1000000;
                MoneyNum.text = millions.ToString() + "m";
                break;
            case 4:
                int billions = money / 1000000000;
                MoneyNum.text = billions.ToString() + "b";
                break;
            default:
                break;
        }
    }

    public void ShipyardMotherSetup(int MotherToDisplay, bool isPanelSwap)
    {
        if(!isPanelSwap)
        {
            MothershipMenu.GetComponentsInChildren<Image>().Where
                (o => o.name == "Mothership").FirstOrDefault().sprite = MotherShip.ShipHeadSprite.sprite;
            MotherHealthBar = MothershipMenu.GetComponentsInChildren<Slider>().Where
            (o => o.name == "HealthBar").FirstOrDefault();
            Health MotherHealth = MotherShip.GetComponentInChildren<Health>();

            MotherHealthBar.minValue = 0;
            MotherHealthBar.maxValue = MotherHealth.healthMax;
            MotherHealthBar.value = MotherHealth.healthCount;
        }

        MothershipDisplay.GetComponentsInChildren<Image>().Where
            (o => o.name == "DisplayMothership").FirstOrDefault().sprite = MotherShip.ShipHeadSprites[MotherToDisplay];

        MothershipDisplay.GetComponentsInChildren<TextMeshProUGUI>().Where
            (o => o.name == "Type").FirstOrDefault().text = Motherships[MotherToDisplay].Title;

        MothershipDisplay.GetComponentsInChildren<TextMeshProUGUI>().Where
            (o => o.name == "Description").FirstOrDefault().text = Motherships[MotherToDisplay].Description;
    }

    public void MothershipPanelSwap(bool DisplayCurrent)
    {
        if(DisplayCurrent)
        {
            ShipyardMotherSetup((int)CurrentMothership, true);
            MothershipDisplay.GetComponentsInChildren<Button>().Where
                    (o => o.name == "CurrentMotherButton").FirstOrDefault().interactable = false;
            MothershipDisplay.GetComponentsInChildren<Button>().Where
                    (o => o.name == "ShopMotherButton").FirstOrDefault().interactable = true;
            TradeInButton.SetActive(false);

        }
        else
        {
            ShipyardMotherSetup((int)TradeInMothership, true);
            MothershipDisplay.GetComponentsInChildren<Button>().Where
                    (o => o.name == "CurrentMotherButton").FirstOrDefault().interactable = true;
            MothershipDisplay.GetComponentsInChildren<Button>().Where
                    (o => o.name == "ShopMotherButton").FirstOrDefault().interactable = false;
            TradeInButton.SetActive(true);

        }
    }

    public void ShipyardShipSetup()
    {
        int size = MotherShip.maxShipsAllowed;
        Ships = new List<GameObject>(size);
        for (int i = 0; i < size; i++)
        {
            if (i + 1 < MotherShip.bodyPartObjects.Count && MotherShip.bodyPartObjects[i + 1] != null)
            {
                MotherShip.bodyPartObjects[i + 1].SetActive(true);
                Ships.Add(MotherShip.bodyPartObjects[i + 1]);
            }
            else
            {
                Ships.Add(null);
            }
        }

        foreach (Transform child in ShipScrollContent.transform)
        {
            Destroy(child.gameObject);
            NumOfShips = 0;
        }

        for (int i = 0; i < Ships.Count; i++)
        {
            GameObject obj = Instantiate(ShipButtonPrefab);
            Button button = obj.GetComponentInChildren<Button>();
            button.gameObject.AddComponent<ShipSelector>();
            ShipSelector selector = button.GetComponent<ShipSelector>();
            selector.ShipMenu = ShipMenu;
            selector.ShopMenu = ShopMenu;
            selector.SelecionDisplay = SelectionDisplay;
            selector.controller = this;
            if (Ships[i] != null)
            {
                NumOfShips++;
                Turret turret = Ships[i].GetComponent<Turret>();

                for (int j = 1; j < 5; j++)
                {
                    Image buttonChildImage = button.transform.GetChild(j).GetComponent<Image>();

                    switch (j)
                    {
                        case 1:
                            buttonChildImage.sprite = turret.spriteRendererWings.sprite;
                            break;
                        case 2:
                            buttonChildImage.sprite = turret.spriteRendererBase.sprite;
                            break;
                        case 3:
                            buttonChildImage.sprite = turret.spriteRendererBadge.sprite;
                            break;
                        case 4:
                            buttonChildImage.sprite = turret.spriteRendererTurret.sprite;
                            break;
                    }
                    buttonChildImage.color = new Color(buttonChildImage.color.r, buttonChildImage.color.g, buttonChildImage.color.b, 1);
                }

                selector.IsSlotFilled = true;
                selector.SelectedShip = Ships[i];
            }
            button.onClick.AddListener(delegate { selector.OpenMenu(); });
            obj.transform.SetParent(ShipScrollContent.transform);
            obj.gameObject.transform.localScale = new Vector3(1, 1);

        }
    }

    public void ShipyardShopSetup()
    {
        foreach (Transform child in ShopShipScrollContent.transform)
        {
            Destroy(child.gameObject);
        }

        if (Timer <= 0)
        {
            if (ShopDifficulty == 0)
            {
                GenerateShopInventory(80, 99);
            }
            else if (ShopDifficulty == 1)
            {
                GenerateShopInventory(40, 95);
            }
            else
            {
                GenerateShopInventory(20, 90);
            }
            Timer = MaxTime;
        }

        for (int i = 0; i < ShopShips.Count; i++)
        {
            GameObject obj = Instantiate(ShipButtonPrefab);
            Button button = obj.GetComponentInChildren<Button>();
            if (ShopShips[i] != null)
            {
                Turret turret = ShopShips[i].GetComponent<Turret>();

                for (int j = 1; j < 5; j++)
                {
                    Image buttonChildImage = button.transform.GetChild(j).GetComponent<Image>();

                    switch (j)
                    {
                        case 1:
                            buttonChildImage.sprite = turret.spriteRendererWings.sprite;
                            break;
                        case 2:
                            buttonChildImage.sprite = turret.spriteRendererBase.sprite;
                            break;
                        case 3:
                            buttonChildImage.sprite = turret.spriteRendererBadge.sprite;
                            break;
                        case 4:
                            buttonChildImage.sprite = turret.spriteRendererTurret.sprite;
                            break;
                    }
                    if (buttonChildImage.sprite != null)
                    {
                        buttonChildImage.color = new Color(buttonChildImage.color.r, buttonChildImage.color.g, buttonChildImage.color.b, 1);
                    }
                }
            }
            obj.transform.SetParent(ShopShipScrollContent.transform);
            obj.gameObject.transform.localScale = new Vector3(1, 1);
        }

    }

    private void GenerateShopInventory(int RareProbability, int EpicProbability)
    {
        for (int i = 0; i < 10; i++)
        {
            float randNum = Random.Range(0.0f, 100.0f);
            if (randNum > EpicProbability)
            {
                //Add Random Epic to Shop List
                int rand = Random.Range(0, EpicShips.Count);
                ShipData EpicShip = EpicShips[rand];
                GameObject Ship = CreateShipFromData(EpicShip);

                ShopShips.Add(Ship);
            }
            else if (randNum > RareProbability)
            {
                //Add Random Rare to Shop List
                int rand = Random.Range(0, RareShips.Count);
                ShipData RareShip = RareShips[rand];
                GameObject Ship = CreateShipFromData(RareShip);

                ShopShips.Add(Ship);
            }
            else
            {
                //Add Random Common to Shop List
                int rand = Random.Range(0, CommonShips.Count);
                ShipData CommonShip = CommonShips[rand];
                GameObject Ship = CreateShipFromData(CommonShip);

                ShopShips.Add(Ship);
            }
        }
    }

    public GameObject CreateShipFromData(ShipData data)
    {
        if (data == null)
        {
            return null;
        }
        GameObject Ship = Instantiate(data.prefab);
        Ship.SetActive(false);
        Turret ShipTurret = Ship.GetComponent<Turret>();

        ShipTurret.data = data;
        int badgeColor = 0;

        Sprite randBase = null;
        Sprite randTurret = null;
        Sprite randWings = null;

        switch (data.type)
        {
            case ShipData.eTurretType.FLAME:
                randBase = data.spriteBasesRed[Random.Range(0, data.spriteBasesRed.Length)];
                randTurret = data.spriteTurretsRed[Random.Range(0, data.spriteTurretsRed.Length)];
                randWings = data.spriteWingsRed[Random.Range(0, data.spriteWingsRed.Length)];
                badgeColor = 0;
                break;
            case ShipData.eTurretType.HEALING:
                randBase = data.spriteBasesGreen[Random.Range(0, data.spriteBasesGreen.Length)];
                randTurret = data.spriteTurretsGreen[Random.Range(0, data.spriteTurretsGreen.Length)];
                randWings = data.spriteWingsGreen[Random.Range(0, data.spriteWingsGreen.Length)];
                badgeColor = 1;
                break;
            case ShipData.eTurretType.LIGHTNING:
                randBase = data.spriteBasesBlue[Random.Range(0, data.spriteBasesBlue.Length)];
                randTurret = data.spriteTurretsBlue[Random.Range(0, data.spriteTurretsBlue.Length)];
                randWings = data.spriteWingsBlue[Random.Range(0, data.spriteWingsBlue.Length)];
                badgeColor = 2;
                break;
            case ShipData.eTurretType.RUSTY:
                randBase = data.spriteBasesOrange[Random.Range(0, data.spriteBasesOrange.Length)];
                randTurret = data.spriteTurretsOrange[Random.Range(0, data.spriteTurretsOrange.Length)];
                randWings = data.spriteWingsOrange[Random.Range(0, data.spriteWingsOrange.Length)];
                badgeColor = 3;
                break;
            case ShipData.eTurretType.ATTACK_DRONE:
                randBase = data.spriteBasesPurple[Random.Range(0, data.spriteBasesPurple.Length)];
                randTurret = data.spriteTurretsPurple[Random.Range(0, data.spriteTurretsPurple.Length)];
                randWings = data.spriteWingsPurple[Random.Range(0, data.spriteWingsPurple.Length)];
                badgeColor = 4;
                break;
        }

        ShipTurret.spriteRendererBase.sprite = randBase;
        ShipTurret.spriteRendererTurret.sprite = randTurret;
        ShipTurret.spriteRendererWings.sprite = randWings;
        switch (data.rarity)
        {
            case ShipData.eTurretRarity.COMMON:
                ShipTurret.spriteRendererBadge.sprite = data.spriteBadgesCommon[badgeColor];
                break;
            case ShipData.eTurretRarity.RARE:
                ShipTurret.spriteRendererBadge.sprite = data.spriteBadgesRare[badgeColor];
                break;
            case ShipData.eTurretRarity.EPIC:
                ShipTurret.spriteRendererBadge.sprite = data.spriteBadgesEpic[badgeColor];
                break;
        }


        ShipTurret.price = data.price;
        ShipTurret.turretRarity = data.rarity;

        return Ship;
    }

    public void OpenSelectedPanel(int num)
    {
        for (int i = 0; i < SelectionInfoPanels.Count; i++)
        {
            if(i == num)
            {
                SelectionInfoPanels[i].SetActive(true);
                SelectionInfoButtons[i].GetComponent<Button>().interactable = false;
            }
            else
            {
                SelectionInfoPanels[i].SetActive(false);
                SelectionInfoButtons[i].GetComponent<Button>().interactable = true;
            }
        }
    }

    public void CheckIfSpecial(GameObject selectedShip)
    {
        if(selectedShip.GetComponent<Turret>().data.isSpecial)
        {
            SelectionInfoButtons[2].SetActive(true);

        }
        else
        {
            int num = 0;
            for (int i = 0; i < SelectionInfoPanels.Count; i++)
            {
                if (SelectionInfoPanels[i].activeInHierarchy)
                {
                    num = i;
                    break;
                }
            }
            if(num == 2) { OpenSelectedPanel(0); }
            SelectionInfoButtons[2].SetActive(false);
        }
    }

    public void GetSelectionInfo(bool isBuying, GameObject selectedShip)
    {
        if (selectedShip != null)
        {
            ShipData data = selectedShip.GetComponent<Turret>().data;

            SelectionDisplay.GetComponentsInChildren<TextMeshProUGUI>().Where
                (o => o.name == "Type").FirstOrDefault().text = data.shipName;

            SelectionDisplay.GetComponentsInChildren<TextMeshProUGUI>().Where
                (o => o.name == "Rarity").FirstOrDefault().text = "Rarity: " + data.rarity;

            SelectionDisplay.GetComponentsInChildren<TextMeshProUGUI>().Where
                (o => o.name == "Description").FirstOrDefault().text = "Description:\n" + data.description;

            GameObject turret = null;
            foreach (Transform child in SelectionDisplay.transform)
            {
                if (child.name == "ShipDisplay")
                {
                    turret = child.gameObject;
                }
            }
            for (int i = 0, j = turret.transform.childCount - 1; i < turret.transform.childCount; i++, j--)
            {
                switch (i)
                {
                    case 0:
                        turret.transform.GetChild(1).GetComponent<Image>().sprite = selectedShip.transform.GetChild(0).transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
                        break;
                    case 1:
                        turret.transform.GetChild(3).GetComponent<Image>().sprite = selectedShip.transform.GetChild(1).transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
                        break;
                    case 2:
                        turret.transform.GetChild(0).GetComponent<Image>().sprite = selectedShip.transform.GetChild(2).transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
                        break;
                    case 3:
                        turret.transform.GetChild(2).GetComponent<Image>().sprite = selectedShip.transform.GetChild(3).transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
                        break;
                }
            }

            if(isBuying)
            {
                buyButton.gameObject.SetActive(true);

                buyButton.interactable = true;

                sellButton.gameObject.SetActive(false);

            }
            else
            {
                sellButton.gameObject.SetActive(true);

                sellButton.interactable = true;

                buyButton.gameObject.SetActive(false);
            }

            OpenSelectedPanel(1);
            GetSelectionStats(selectedShip);
            OpenSelectedPanel(0);
        }
        else
        {
            SelectionDisplay.GetComponentsInChildren<TextMeshProUGUI>().Where
                (o => o.name == "Type").FirstOrDefault().text = "EMPTY";

            SelectionDisplay.GetComponentsInChildren<TextMeshProUGUI>().Where
                (o => o.name == "Rarity").FirstOrDefault().text = "Rarity: " + "EMPTY";

            SelectionDisplay.GetComponentsInChildren<TextMeshProUGUI>().Where
                (o => o.name == "Description").FirstOrDefault().text = "Description:\n" + "EMPTY";

            buyButton.gameObject.SetActive(true);

            buyButton.interactable = false;

            GameObject turret = null;
            foreach (Transform child in SelectionDisplay.transform)
            {
                if (child.name == "ShipDisplay")
                {
                    turret = child.gameObject;
                }
            }
            for (int i = 0, j = turret.transform.childCount - 1; i < turret.transform.childCount; i++, j--)
            {
                switch (i)
                {
                    case 0:
                        turret.transform.GetChild(1).GetComponent<Image>().sprite = EmptySlot;
                        break;
                    case 1:
                        turret.transform.GetChild(3).GetComponent<Image>().sprite = EmptySlot;
                        break;
                    case 2:
                        turret.transform.GetChild(0).GetComponent<Image>().sprite = EmptySlot;
                        break;
                    case 3:
                        turret.transform.GetChild(2).GetComponent<Image>().sprite = EmptySlot;
                        break;
                }
            }
        }
    }

    public void GetSelectionStats(GameObject selectedShip)
    {
        SelectionDisplay.GetComponentsInChildren<TextMeshProUGUI>().Where
            (o => o.name == "FireRateNum").FirstOrDefault().text = (selectedShip.GetComponent<Turret>().attackSpeed).ToString();

        SelectionDisplay.GetComponentsInChildren<TextMeshProUGUI>().Where
            (o => o.name == "DamageNum").FirstOrDefault().text = (selectedShip.GetComponent<Turret>().damage).ToString();

        SelectionDisplay.GetComponentsInChildren<TextMeshProUGUI>().Where
            (o => o.name == "RangeNum").FirstOrDefault().text = (selectedShip.GetComponent<Turret>().range).ToString();
    }

    public void SelectionIncrement()
    {
        int num = 0;
        for (int i = 0; i < SelectionInfoPanels.Count; i++)
        {
            if(SelectionInfoPanels[i].activeInHierarchy)
            {
                num = i;
                break;
            }
        }

        OpenSelectedPanel(0);
        GetSelectionInfo(true, ShopShips[scrollSnap.CurrentPage() + 1]);
        CheckIfSpecial(ShopShips[scrollSnap.CurrentPage() + 1]);

        OpenSelectedPanel(num);
    }

    public void SelectionDecrement()
    {
        int num = 0;
        for (int i = 0; i < SelectionInfoPanels.Count; i++)
        {
            if (SelectionInfoPanels[i].activeInHierarchy)
            {
                num = i;
                break;
            }
        }

        OpenSelectedPanel(0);
        GetSelectionInfo(true, ShopShips[scrollSnap.CurrentPage() -1]);
        CheckIfSpecial(ShopShips[scrollSnap.CurrentPage() - 1]);

        OpenSelectedPanel(num);
    }

    public void Purchase()
    {
        if (ShopShips[scrollSnap.CurrentPage()] != null)
        {
            if (NumOfShips != Ships.Count)
            {
                GameObject purchase = ShopShips[scrollSnap.CurrentPage()];
                for (int i = 0; i < Ships.Count; i++)
                {
                    if (Ships[i] == null)
                    {
                        if (WorldManager.Instance.PlayerController.RemoveMoney(purchase.GetComponent<Turret>().data.price))
                        {
                            if (i + 1 < MotherShip.bodyPartObjects.Count)
                            {
                                MotherShip.bodyPartObjects[i + 1] = purchase;
                                MotherShip.SortBody();
                            }
                            else
                            {
                                MotherShip.AddBodyPart(purchase);
                                MotherShip.SortBody();
                            }
                        }
                        else
                        {
                            Debug.Log("You're poor");
                            return;// YOU DON'T HAVE ENOUGH MONEY >:C
                        }
                        Ships[i] = purchase;

                        break;
                    }
                }
                ShopShips[scrollSnap.CurrentPage()] = null;
                SortShips();
                GetSelectionInfo(true, ShopShips[scrollSnap.CurrentPage()]);
                ShipyardShipSetup();
                ShipyardShopSetup();
            }
            else
            {
                //Maybe some kind of ERROR message to let the player know they're full on ships
                MaxShipWarning.SetActive(true);
            }
        }
    }

    public void SortShips()
    {
        for (int i = 1; i < ShopShips.Count; i++)
        {
            if (ShopShips[i - 1] == null)
            {
                ShopShips[i - 1] = ShopShips[i];
                ShopShips[i] = null;
            }
        }
    }

    public void AddToShop(GameObject ship)
    {
        GameObject newShip = Instantiate<GameObject>(ship);
        bool added = false;
        for (int i = 0; i < ShopShips.Count; i++)
        {
            if (ShopShips[i] == null)
            {
                ShopShips[i] = newShip;
                added = true;
                break;
            }
        }
        if (!added)
        {
            ShopShips.Add(newShip);
        }

        newShip.SetActive(false);
    }

    public void TradeIn()
    {
        int tradeInValue = 300;
        if (WorldManager.Instance.PlayerController.RemoveMoney(tradeInValue))
        {
            Ship.eMotherShip temp = MotherShip.motherShip;

            MotherShip.SetShipHead((int)TradeInMothership);

            CurrentMothership = TradeInMothership;
            TradeInMothership = temp;


            ShipyardMotherSetup((int)TradeInMothership, false);
            MothershipPanelSwap(true);
        }
        else
        {
            // You don't have enough money >:C
        }
    }

    public void RepairAll()
    {
        float repairCostPerHP = 1f;
        float hpToRestore = 0f;
        for (int i = 0; i < MotherShip.bodyPartObjects.Count; i++)
        {
            if (MotherShip.bodyPartObjects[i] != null)
            {
                hpToRestore += MotherShip.bodyPartObjects[i].GetComponent<Health>().healthMax - MotherShip.bodyPartObjects[i].GetComponent<Health>().healthCount;
            }
        }

        if (WorldManager.Instance.PlayerController.RemoveMoney((int)(hpToRestore * repairCostPerHP)))
        {
            for (int i = 0; i < MotherShip.bodyPartObjects.Count; i++)
            {
                if (MotherShip.bodyPartObjects[i] != null)
                {
                    MotherShip.bodyPartObjects[i].GetComponent<Health>().healthCount = MotherShip.bodyPartObjects[i].GetComponent<Health>().healthMax;
                }
            }
        }
    }

    public void RepairMother()
    {
        float repairCostPerHP = 1f;
        float hpToRestore = 0f;
        Health MotherHealth = MotherShip.GetComponentInChildren<Health>();
        hpToRestore += MotherHealth.healthMax - MotherHealth.healthCount;

        if (WorldManager.Instance.PlayerController.RemoveMoney((int)(hpToRestore * repairCostPerHP)))
        {
            MotherHealth.healthCount = MotherHealth.healthMax;
            MotherHealthBar.value = MotherHealth.healthCount;
        }
    }

    public void CloseMessage()
    {
        MaxShipWarning.SetActive(false);
    }

    public void OpenShop()
    {
        AudioManager.Instance.StopAll();
        AudioManager.Instance.PlayRandomMusic("Shop");
        ShipyardShipSetup();
        ShipyardMotherSetup((int)CurrentMothership, false);
        Shipyard.SetActive(true);
        Time.timeScale = 0;
    }

    public void CloseShop()
    {
        AudioManager.Instance.StopAll();
        AudioManager.Instance.PlayRandomMusic("Battle");
        Shipyard.SetActive(false);
        ShipMenu.SetActive(false);
        ShopMenu.SetActive(false);
        SelectionDisplay.SetActive(false);
        Time.timeScale = 1;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OpenShop();
        }
    }

}
