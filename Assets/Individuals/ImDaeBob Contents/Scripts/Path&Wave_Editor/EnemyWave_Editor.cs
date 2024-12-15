using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyWave_Editor : MonoBehaviour
{
    private bool _active = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void toggleEditor(bool mode)
    {
        _active = mode;
    }

    public bool getActive()
    {
        return _active;
    }
}
