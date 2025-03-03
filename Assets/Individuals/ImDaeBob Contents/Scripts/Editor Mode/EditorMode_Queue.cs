using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class EditorMode_Queue : MonoBehaviour
{
    [HideInInspector] public static EditorMode_Queue editorQueue;

    [Header("Time")]
    [SerializeField] TMP_Text _timerUI;
    public float _timer = 6f;
    private bool _eHeld = false;
    private float _lastUpdateTime = 0f;

    [Header("Transition")]
    [SerializeField] GameObject _transition;

    [Header("Editors")]
    [SerializeField] GameObject _editor;

    void Awake()
    {
        if (editorQueue == null)
            editorQueue = this;
    }

    void OnEnable()
    {
        _editor.SetActive(false);
    }

    void Start()
    {
        this.gameObject.GetComponent<CanvasGroup>().alpha = 0;
        _editor.gameObject.SetActive(false);
    }

    void Update()
    {
        //-------------------------- Queuing to Editor --------------------------//
        if (Input.GetKeyDown(KeyCode.E) && !_eHeld)
        {
            Queuing(true);
            _eHeld = true;
            _timer = 6f;
            _timerUI.text = _timer.ToString();
        }
        if (Input.GetKeyUp(KeyCode.E) && _eHeld)
        {
            Queuing(false);
            _eHeld = false;
            _timerUI.text = "Cancelled";
        }

        //---------------------------- Handle timer ----------------------------//
        if (_eHeld && _timer > 0 && Time.time - _lastUpdateTime >= 1f)
        {
            _timer--;
            _timerUI.text = _timer.ToString();
            _lastUpdateTime = Time.time;
        }
        else if (_eHeld && _timer <= 0)
        {
            _eHeld = false;
            _transition.SetActive(true);
            //_transition.GetComponent<Animator>().SetTrigger("In");

            StartCoroutine(ExitQueue());
        }
    }

    private Tween _curTween;
    void Queuing(bool mode) //Queue Fade
    {
        if (_curTween != null && _curTween.IsActive())
            _curTween.Kill();

        if (mode)
            _curTween = this.gameObject.GetComponent<CanvasGroup>().DOFade(1f, 0.25f).SetEase(Ease.InOutCirc); 
        else
            _curTween = this.gameObject.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).SetEase(Ease.InOutCirc);
    }

    IEnumerator ExitQueue()
    {
        yield return new WaitForSeconds(0.5f);
        this.gameObject.GetComponent<CanvasGroup>().alpha = 0;
        _transition.GetComponent<Animator>().SetTrigger("Out");
        _editor.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        _transition.SetActive(false);
        this.gameObject.SetActive(false);
    }
}
