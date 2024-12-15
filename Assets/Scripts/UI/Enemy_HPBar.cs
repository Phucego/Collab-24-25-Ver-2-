using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

    [Header("Visibility Settings:")]
    [SerializeField] float _visibilityDistance = 50f;

    private bool _isVisible = false;

    void Start()
    {
        _cameraToTrack = Camera.main;
        gameObject.GetComponent<CanvasGroup>().alpha = 0; 
    }

    void Update()
    {
        float _distance = Vector3.Distance(_cameraToTrack.transform.position, _target.position);
        if (_distance <= _visibilityDistance)
        {
            if (!_isVisible)
                SetVisibility(true);

            // Update health bar position and rotation
            transform.rotation = _cameraToTrack.transform.rotation;
            transform.position = _target.position + _offset;

            _number.rectTransform.rotation = _cameraToTrack.transform.rotation;
            _number.rectTransform.position = _target.position + _offset;
        }
        else
        {
            if (_isVisible)
                SetVisibility(false);
        }
    }

    public void setHealth(float curHealth, float maxHealth)
    {
        _slider.value = curHealth / maxHealth;
        _number.text = $"{curHealth}/{maxHealth}";
    }

    private Tween _curTween;
    private void SetVisibility(bool isVisible)
    {
        _isVisible = isVisible;

        if (_curTween != null && _curTween.IsActive())
            _curTween.Kill();
        if (isVisible)
            _curTween = gameObject.GetComponent<CanvasGroup>().DOFade(1f, 0.15f).SetEase(Ease.OutCirc);
        else
            _curTween = gameObject.GetComponent<CanvasGroup>().DOFade(0f, 0.15f).SetEase(Ease.InCirc);
    }
}
