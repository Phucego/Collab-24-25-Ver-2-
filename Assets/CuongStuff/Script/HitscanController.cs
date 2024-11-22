using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HitscanController : MonoBehaviour
{
    protected LineRenderer lineRenderer;
    protected GameObject target;
    protected LayerMask layerMask;

    protected Color defaultColor;
     
    protected virtual void Awake()
    {
        layerMask = LayerMask.GetMask("Enemy");
        lineRenderer = GetComponent<LineRenderer>();
        defaultColor = lineRenderer.material.color;
    }

    private void FixedUpdate()
    { 

    }

    private void OnEnable()
    {
        StartCoroutine(DisableObject());
    }

    public void SetTarget(GameObject enemy)
    {
        target = enemy;
        Vector3 pos = target.transform.position;
        Vector3[] linepos = new Vector3[2] { transform.position, pos};
        
        Vector3 dir = (pos - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, pos) + 2f;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, dist, layerMask))
        { 
            if (hit.collider != null)
            {
                target = hit.collider.gameObject;
                Debug.Log("Hit an enemy: " + target.name);
            }
        }

        lineRenderer.SetPositions(linepos);
        
    }

    private void OnDisable()
    {
        lineRenderer.SetPositions(new Vector3[] { });
        lineRenderer.material.SetColor("_Color", defaultColor);
    }

    private IEnumerator DisableObject()
    {
        Color rendererColor = lineRenderer.startColor;
        float opacity = 1f;
        yield return new WaitForSeconds(0.3f);
        while (opacity > 0f)
        {
            opacity -= 0.1f;
            rendererColor.a = opacity;
            lineRenderer.material.SetColor("_Color", rendererColor);
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }

}
