using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class EnemyLevel_Editor : MonoBehaviour
{
    private bool _active = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleEditor(bool mode)
    {
        _active = mode;
    }

    public bool GetActive()
    {
        return _active;
    }
}
