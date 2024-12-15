using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    public GameObject selectedObj;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000))
            {
                if (hit.collider.gameObject.CompareTag("Selectable"))
                {
                    Select(hit.collider.gameObject);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Deselect();
        }
    }

    private void Select(GameObject obj)
    {
        if (obj == selectedObj) return;
        OutlineScript outline = obj.GetComponent<OutlineScript>();
        if (selectedObj != null)
        {
            Deselect();
        }
        if (outline == null)
        {
            obj.AddComponent<OutlineScript>();
        }
        else
        {
            outline.enabled = true;
        }

        selectedObj = obj;
    }

    private void Deselect()
    {
        selectedObj.GetComponent<OutlineScript>().enabled = true;
        selectedObj = null;
    }
}
