using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Path_Editor_Handler : MonoBehaviour
{
    private bool _inEditor = false; 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            _inEditor = !_inEditor;
            ToggleEditor();
        }
    }

    private void ToggleEditor()
    {
        if (_inEditor)
        {

        }
        else
        {

        }
    }
}
