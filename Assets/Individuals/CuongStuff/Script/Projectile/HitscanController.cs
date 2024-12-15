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
    float opacity = 1f;
    private Vector3[] linepos;
     
    protected virtual void Awake()
    {
        layerMask = LayerMask.GetMask("Enemy");
        lineRenderer = GetComponent<LineRenderer>();
        defaultColor = lineRenderer.material.color;
    }

    protected void Update()
    {
        Color rendererColor = defaultColor; 
        if (opacity > 0f && lingering)
        {
            opacity -= 0.005f;
            rendererColor.a = opacity;
            lineRenderer.material.SetColor("_Color", rendererColor);
        }
    }

    public void SetTarget(GameObject enemy)
    {
        if (target != enemy)
            target = enemy;

        opacity = 1f;
        Vector3 pos = target.transform.position;
        linepos = PlayZap(transform.position, pos);
        
        Vector3 dir = (pos - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, pos) + 2f;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, dist, layerMask))
        { 
            if (hit.collider != null)
            {
                //target = hit.collider.gameObject;
                Vector3 newpos = hit.collider.transform.position;
                linepos = PlayZap(transform.position, hit.point);
                if (damagable && lingering)
                    ApplyDamage(hit.collider.gameObject);
                    
            }
        }

        lineRenderer.SetPositions(linepos);
    }

    private void OnEnable()
    {
        damagable = true;
        lingering = true;
        opacity = 1f;
        StartCoroutine(DisableObject());
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
        yield return new WaitForSeconds(3f);
        Color rendererColor = defaultColor;
        lingering = false;

        while (opacity > 0f)
        {
            opacity -= 0.05f;
            rendererColor.a = opacity;
            lineRenderer.material.SetColor("_Color", rendererColor);
            yield return new WaitForFixedUpdate();
        }

        gameObject.SetActive(false);
    }

    private Vector3[] PlayZap(Vector3 original, Vector3 end)
    {
        Vector3[] linepos = new Vector3[10];
        linepos[0] = original;
        linepos[9] = end;
        for (int i = 0; i < 8; i++)
        { 
            float point = ((float)i + 1f) / 10f;
            Vector3 offsetline = Vector3.Lerp(original, end, point);
            offsetline = new Vector3(offsetline.x, offsetline.y + Random.Range(-1f, 1f), offsetline.z);
            linepos[i + 1] = offsetline;     
        }
        return linepos;
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
