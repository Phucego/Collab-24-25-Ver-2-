using UnityEngine;

public class CloudMover : MonoBehaviour
{
    public float speed;
    private Transform target;
    private CloudSpawner spawner;

    public void Initialize(float newSpeed, Transform targetTransform, CloudSpawner cloudSpawner)
    {
        speed = newSpeed;
        target = targetTransform;
        spawner = cloudSpawner;
    }

    void Update()
    {
        if (target == null)
        {
            spawner.RemoveCloud(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            spawner.RemoveCloud(gameObject);
        }
    }
}