using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;

public class TowerInteract : MonoBehaviour, I_Interactable
{
    public InputActionAsset towerAction;
    public UnityEvent UpgradeTower;
    public UnityEvent SellTower;

    private GameObject _RadiusSphere;
    private GameObject _CanvasInfo;
    private InputActionMap mapAction;
    private InputAction upgradeAction, sellAction;
    private float Radius;
    private Camera MainCam;

    public bool isPlaced;

    void Awake()
    {
        mapAction = towerAction.FindActionMap("Tower");
        upgradeAction = mapAction.FindAction("UpgradeTower");
        sellAction = mapAction.FindAction("SellTower");
        _RadiusSphere = gameObject.transform.GetChild(3).gameObject;
        _CanvasInfo = gameObject.transform.GetChild(4).gameObject;
    }

    void OnEnable()
    {   
        upgradeAction.Enable();
        sellAction.Enable();
        sellAction.performed += Sell;
        upgradeAction.performed += Upgrade;
    }

    void OnDisable()
    {
        upgradeAction.Disable();
        sellAction.Disable();
        upgradeAction.performed -= Upgrade;
        sellAction.performed -= Sell;
    }

    public void Upgrade(InputAction.CallbackContext input)
    {
        if (_CanvasInfo.activeInHierarchy)
        {
            UpgradeSelected();
        }
    }

    public void UpgradeSelected()
    {
        UpgradeTower.Invoke();
        _CanvasInfo.GetComponent<TowerCanvasHandler>().Upgrade();
    }

    public void Sell(InputAction.CallbackContext input)
    {
        if (_CanvasInfo.activeInHierarchy)
        {
            SellSelected();
        }
    }

    public void SellSelected()
    {
        MainCam.GetComponent<InteractController>().DeleteInteractedObject();
        Deselect();
        SellTower.Invoke();
    }

    void LateUpdate()
    {
        if (MainCam != null && _CanvasInfo.activeInHierarchy)
        {
            _CanvasInfo.transform.LookAt(_CanvasInfo.transform.position + MainCam.transform.forward);
        }
    }

    public void ChangeStat(UpgradeType type, float value)
    {
        if (_RadiusSphere == null)
            return;

        switch (type)
        {
            case UpgradeType.Radius:
                Radius = value;
                _RadiusSphere.transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
                break;
        }
    }

    public void Interact(Camera camera)
    {
        if (!isPlaced) return; // Prevent interaction if the tower is not placed

        MainCam = camera;
        _RadiusSphere.transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
        _RadiusSphere.SetActive(true);
        _CanvasInfo.SetActive(true);
    }

    public void Deselect()
    {
        MainCam = null; 
        _RadiusSphere.SetActive(false);
        _CanvasInfo.SetActive(false);
       
    }

    public void TowerInfo(bool enable)
    {
        if (!isPlaced) return; // Prevent showing info if the tower is not placed

        _RadiusSphere.transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
        _RadiusSphere.SetActive(enable);
        _CanvasInfo.SetActive(enable);
    }

    public void ToggleOutline(bool enable)
    {
        Outline outline = GetComponent<Outline>();
        if (!isPlaced)
        {
            outline.enabled = false;
            return;
        }

        if (outline != null)
        {
            outline.enabled = enable;
        }
    }
}
