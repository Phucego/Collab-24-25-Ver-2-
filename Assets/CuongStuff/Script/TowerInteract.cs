using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TowerInteract : MonoBehaviour, I_Interactable
{
    [SerializeField] public GameObject _RadiusSphere;
    [SerializeField] public GameObject _CanvasInfo;
    private float Radius;
    private Camera MainCam;
    // Start is called before the first frame update
    void Start()
    {
        //_Radius = GameObject.Find("RadiusDetection");
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

    public void Interact(Camera camera)
    {
        MainCam = camera;
        _RadiusSphere.transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
        _RadiusSphere.SetActive(true);
        _CanvasInfo.SetActive(true);
        Debug.Log("Tower interact!");
    }

    public void Deselect()
    {
        _RadiusSphere.SetActive(false);
        _CanvasInfo.SetActive(false);
    }

}
