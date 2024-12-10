using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitscanController : MonoBehaviour, I_TowerProjectile
{
    protected LineRenderer lineRenderer;
    protected GameObject target;
    protected GameObject[] enemies;
    protected LayerMask layerMask;
    protected float Damage = 10f;
    protected float Debounce = 0.1f;
    protected Color defaultColor;

    private bool damagable = true;
    private bool lingering;
     
    protected virtual void Awake()
    {
        layerMask = LayerMask.GetMask("Enemy");
        lineRenderer = GetComponent<LineRenderer>();
        defaultColor = lineRenderer.material.color;
    }

    private void OnEnable()
    {
        damagable = true;
        lingering = false;
        StartCoroutine(DisableObject());
    }

    public void SetTarget(GameObject enemy)
    {
        if (target != enemy)
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
                //target = hit.collider.gameObject;
                Vector3 newpos = hit.collider.transform.position;
                linepos[1] = hit.point;
                if (damagable && lingering)
                    ApplyDamage(hit.collider.gameObject);
                    
            }
        }

        lineRenderer.SetPositions(linepos);
    }

    private void OnDisable()
    {
        lineRenderer.SetPositions(new Vector3[] { });
        lineRenderer.material.SetColor("_Color", defaultColor);
        target = null;
    }

    private IEnumerator SetDamagable()
    {
        damagable = false;
        yield return new WaitForSeconds(0.7f);
        damagable = true;
    }

    private IEnumerator DisableObject()
    {
        lingering = true;
        yield return new WaitForSeconds(3f);
        Color rendererColor = lineRenderer.startColor;
        float opacity = 1f;
        yield return new WaitForSeconds(0.3f);
        lingering = false;
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

    protected virtual void ApplyDamage(GameObject target)
    {
        target.GetComponent<I_Damagable>().TakeDamage(Damage);
        if (gameObject.activeInHierarchy)
            StartCoroutine(SetDamagable());
    }

    public virtual void SetDamage(float dmg)
    {
        Damage = dmg;
    }

    public virtual void SetDebuff(float duration)
    {
        throw new System.NotImplementedException();
    }

    public virtual void SetRadius(float radius)
    {
        throw new System.NotImplementedException();
    }
}
