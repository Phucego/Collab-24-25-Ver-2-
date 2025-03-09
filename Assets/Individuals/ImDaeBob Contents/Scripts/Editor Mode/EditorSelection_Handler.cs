using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EditorSelection_Handler : MonoBehaviour
{
    //---------------------------------------------------------------- < VARIABLES > ----------------------------------------------------------------//
    [Header("Editor Choose Screen")]
    [SerializeField] Button _pathButton;
    [SerializeField] GameObject _pathEditor;
    [SerializeField] Button _waveButton;
    [SerializeField] GameObject _waveEditor;

    [Header("Transition")]
    [SerializeField] GameObject _transition;

    bool _inSelect = true;

    //-------------------------------------------------------------- < MAIN FUNCTIONS > --------------------------------------------------------------//
    void OnEnable()
    {
        _inSelect = true;
        this.gameObject.GetComponent<CanvasGroup>().alpha = 1;
    }

    void Start()
    {
    }

    void Update()
    {
        if (_inSelect && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace)))
        {
            _inSelect = false;
            _transition.SetActive(true);
            StartCoroutine(EndTransition(0));
        }
    }

    //-------------------------------------------------------------- < MINI FUNCTIONS > --------------------------------------------------------------//
    // [ BUTTON HANDLERS ] //
    public void Enabale_PathEditor()
    {
        if (_inSelect)
        {
            _inSelect = false;
            _transition.SetActive(true);
            StartCoroutine(EndTransition(1));
        }
    }

    public void Enable_WaveEditor()
    {
        if (_inSelect)
        {
            _inSelect = false;
            _transition.SetActive(true);
            StartCoroutine(EndTransition(2));
        }
    }

    // [ IENUMERATOR HANDLERS ] //
    IEnumerator EndTransition(int mode)
    {
        yield return new WaitForSeconds(0.5f);
        this.gameObject.GetComponent<CanvasGroup>().alpha = 0;
        _transition.GetComponent<Animator>().SetTrigger("Out");
        switch (mode)
        {
            case 1:
                _pathEditor.SetActive(true);
                break;
            case 2:
                _waveEditor.SetActive(true);
                break;
        }

        yield return new WaitForSeconds(0.5f);
        _transition.SetActive(false);
        if (mode == 0)
        {
            if (EditorMode_Queue.editorQueue != null)
                EditorMode_Queue.editorQueue.gameObject.SetActive(true);
        }
        else
            this.gameObject.SetActive(false);
    }
}
