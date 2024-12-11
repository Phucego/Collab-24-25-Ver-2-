using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainEditorManager : MonoBehaviour
{
    [SerializeField] CanvasGroup _pathManager;
    private EnemyPath_Editor _path;
    [SerializeField] CanvasGroup _waveManager;
    private EnemyWave_Editor _wave;

    public bool _inEditor = true;

    public delegate void OnFadeCompleted();


    private void Start()
    {
        _path = _pathManager.GetComponent<EnemyPath_Editor>();
        _wave = _waveManager.GetComponent<EnemyWave_Editor>();
    }

    void Update()
    {
        if (_inEditor)
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _path.toggleEditor(!_path.getActive());
                if (_path.getActive())
                    _pathManager.DOFade(0, 0.25f).SetEase(Ease.InOutCirc);
                else
                    _pathManager.DOFade(1, 0.25f).SetEase(Ease.InOutCirc);
            }

            //if (Input.GetKeyDown(KeyCode.F2))
            //{
            //    _wave.toggleEditor(!_wave.getActive());
            //}
        }
    }
}
