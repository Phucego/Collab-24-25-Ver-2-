using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerCanvasHandler : MonoBehaviour
{
    private TowerController towerController;

    [SerializeField] private TMP_Text towerName, towerCurrent, towerUpgrade, towerLevel;
    [SerializeField] private GameObject towerBar;
    [SerializeField] private List<Image> bars = new List<Image>();

    private void Awake()
    {
        towerController = gameObject.GetComponentInParent<TowerController>();
        foreach (var bar in towerBar.GetComponentsInChildren<Image>())
        {
            bars.Add(bar);
        }
    }

    private void OnEnable()
    {
        Upgrade();
    }

    public void Upgrade()
    {
        if (towerController != null)
        {
            towerName.text = towerController.GetName();
            towerCurrent.text = towerController.GetCurrentStats();
            towerUpgrade.text = towerController.GetUpgradeStats();
            towerLevel.text = towerController.GetLevelString();
            UpdateBar();
        }
    }

    private void UpdateBar()
    {
        int level = towerController.GetLevelInt();
        for (int i = 0; i < level + 1; i++)
        {
            bars[i].color = Color.yellow;
        }
    }
    /*public void UpdateCurrentStat()
    {
      
    }

    public void UpdateName()
    {

    }
    public void UpdateLevelBar()
    {

    }

    public void UpdateUpgradeStat()
    {

    }*/
}
