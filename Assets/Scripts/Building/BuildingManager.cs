using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BuildingManager : MonoBehaviour
{
    public GameObject[] gameObjects;
    public GameObject pendingObj;

    public float rotationAMT;

    private Vector3 pos;
    RaycastHit hit;
    [SerializeField] private LayerMask placeableLayer;

    // Update is called once per frame
    void Update()
    {
        if (pendingObj != null)
        {
            pendingObj.transform.position = pos;

            if (Input.GetMouseButtonDown(0))
            {
                PlaceObject();
            }

        }


        //Take the middle point of the screen instead of the mouse pos
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if(Physics.Raycast(ray, out hit, Mathf.Infinity, placeableLayer))
        {
            pos = hit.point;    
        }
    }
    public void PlaceObject()
    {
        pendingObj = null; 
    }

    public void SelectObject(int index)
    {
        pendingObj = Instantiate(gameObjects[index], pos, transform.rotation);
    }
}
