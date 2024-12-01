using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy_HPBar : MonoBehaviour
{
    [Header("UI:")]
    [SerializeField] Slider _slider;
    [SerializeField] TMP_Text _number;

    [Header("Camera:")]
    [SerializeField] Camera _cameraToTrack;

    [Header("Positioning:")]
    [SerializeField] Transform _target;
    [SerializeField] Vector3 _offset;

    void Start()
    {
        _cameraToTrack = Camera.main;
    }

    void Update()
    {
        if (_cameraToTrack == null)
            return;

        transform.rotation = _cameraToTrack.transform.rotation;
        transform.position = _target.position + _offset;

        _number.rectTransform.rotation = _cameraToTrack.transform.rotation;
        _number.rectTransform.position = _target.position + _offset;
    }

    public void setHealth(float curHealth, float maxHealth)
    {
        _slider.value = curHealth/maxHealth;
        _number.text = curHealth.ToString() + '/' + maxHealth.ToString();
    }
}
