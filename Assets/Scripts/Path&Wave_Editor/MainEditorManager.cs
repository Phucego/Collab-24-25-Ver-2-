using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainEditorManager : MonoBehaviour
{
    [HideInInspector] public static MainEditorManager inEditor; //Global access just in case

    [Header("Editors")]
    [SerializeField] GameObject _pathManager;
    private EnemyPath_Editor _path;
    [SerializeField] GameObject _waveManager;
    private EnemyWave_Editor _wave;

    [Header("UI")]
    [SerializeField] CanvasGroup _toggles;

    
    private int _inSection = 0; //1 = Path | 2 = Wave

    //public delegate void OnFadeCompleted();

    private void Awake()
    {
        if (inEditor == null)
            inEditor = this;
    }

    void Start()
    {
        _path = _pathManager.GetComponent<EnemyPath_Editor>();
        _wave = _waveManager.GetComponent<EnemyWave_Editor>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1) && _inSection != 2)
        {
            _pathManager.SetActive(true);

            _path.toggleEditor(!_path.getActive());
            if (_path.getActive())
            {
                toggleEditors(1);
                _pathManager.GetComponent<CanvasGroup>().DOFade(0f, 0.25f).SetEase(Ease.InOutCirc).OnComplete(()
                    => _pathManager.SetActive(false));
            }
            else
            {
                toggleEditors(0);
                _pathManager.GetComponent<CanvasGroup>().DOFade(1f, 0.25f).SetEase(Ease.InOutCirc);
            }
        }

        //if (Input.GetKeyDown(KeyCode.F2))
        //{
        //    _wave.toggleEditor(!_wave.getActive());
        //}
    }

    private Tween _curTween;
    private void toggleEditors(int section)
    {
        _inSection = section;

        if (_curTween != null && _curTween.IsActive())
            _curTween.Kill();

        if (section == 0)
        {
            if (_toggles.alpha < 1f)
                _curTween = _toggles.DOFade(1f, 0.25f);
        }
        else if (section > 0)
        {
            if (_toggles.alpha > 0.15f)
                _curTween = _toggles.DOFade(0.15f, 0.25f);
        }
    }
}
