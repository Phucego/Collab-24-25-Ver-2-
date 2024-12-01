using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy_HPBar : MonoBehaviour
{
    [Header("UI:")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text number;

    [Header("Camera:")]
    [SerializeField] private Camera cameraToTrack;

    [Header("Positioning:")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;

    void Start()
    {
        cameraToTrack = Camera.main;
    }

    void Update()
    {
        if (cameraToTrack == null)
            return;

        transform.rotation = cameraToTrack.transform.rotation;
        transform.position = target.position + offset;

        number.rectTransform.rotation = cameraToTrack.transform.rotation;
        number.rectTransform.position = target.position + offset;
    }

    public void setHealth(float curHealth, float maxHealth)
    {
        slider.value = curHealth/maxHealth;
        number.text = curHealth.ToString() + '/' + maxHealth.ToString();
    }
}
