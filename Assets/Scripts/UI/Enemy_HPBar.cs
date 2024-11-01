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

    [Header("Positioning:")]
    [SerializeField] private Camera camera;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;

    void Update()
    {
        transform.rotation = camera.transform.rotation;
        transform.position = target.position + offset;
        number.rectTransform.position = target.position + offset;
    }

    public void setHealth(float curHealth, float maxHealth)
    {
        slider.value = curHealth/maxHealth;
        number.text = curHealth.ToString() + '/' + maxHealth.ToString();
    }
}
