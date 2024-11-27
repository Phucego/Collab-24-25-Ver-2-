using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;

public class TowerInteract : MonoBehaviour, I_Interactable
{
    public InputActionAsset towerAction;
    public GameObject _RadiusSphere;
    public GameObject _CanvasInfo;
    public UnityEvent UpgradeTower;

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
            UpgradeTower.Invoke();
        }
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
                break;
        }
    }
    public void ToggleOutline(bool enable)
    {
        // Assuming the outline is managed via a Renderer or an Outline component
        Outline outline = GetComponent<Outline>(); // Replace with your outline logic
        if (outline != null)
        {
            outline.enabled = enable;

        }
    }

    public void TowerInfo(bool enable)
    {
        _RadiusSphere.transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
        _RadiusSphere.SetActive(enable);
        _CanvasInfo.SetActive(enable);
    }

}
