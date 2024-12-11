using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class TowerInteract : MonoBehaviour, I_Interactable
{
    public InputActionAsset towerAction;
    public UnityEvent UpgradeTower;

    private GameObject _RadiusSphere;
    private GameObject _CanvasInfo;
    private InputActionMap mapAction;
    private InputAction upgradeAction, sellAction;
    private float Radius;
    private Camera MainCam;

    void Awake()
    {
        mapAction = towerAction.FindActionMap("Tower");
        upgradeAction = mapAction.FindAction("UpgradeTower");
        //_CallChangeStat.AddListener(ChangeStat);
        //_Radius = GameObject.Find("RadiusDetection");
    }

    void OnEnable()
    {
        _RadiusSphere = gameObject.transform.GetChild(3).gameObject;
        _CanvasInfo = gameObject.transform.GetChild(4).gameObject;      
        upgradeAction.Enable();
        upgradeAction.performed += Upgrade;
    }

    void OnDisable()
    {
        upgradeAction.Disable();
        upgradeAction.performed -= Upgrade;
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

    // Update is called once per frame
    void LateUpdate()
    {
        if (MainCam != null && _CanvasInfo.activeInHierarchy)
        {
            _CanvasInfo.transform.LookAt(_CanvasInfo.transform.position + MainCam.transform.forward);
        }
    }

    public void ChangeStat(UpgradeType type, float value)
    {
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
        MainCam = camera;
        _RadiusSphere.transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
        _RadiusSphere.SetActive(true);
        _CanvasInfo.SetActive(true);
    }

    public void Deselect()
    {
        _RadiusSphere.SetActive(false);
        _CanvasInfo.SetActive(false);
    }

}
