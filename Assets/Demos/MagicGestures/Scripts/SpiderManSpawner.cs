using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderManSpawner : MonoBehaviour
{
    [SerializeField] private MagicHandGestures magicHandGestures;
    [SerializeField] private GameObject webPrefab;
    [SerializeField] private float shootCooldown = 0.05f;
    [SerializeField] private float impulseForce = 5f;
    [SerializeField] private float webLifeTime = 5f;
    [SerializeField] private float spawnOffset = 0.002f;

    float lastShootTime = -Mathf.Infinity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (magicHandGestures.magicHand.IsAvailable() && magicHandGestures.IsSpiderMan)
        {
            if (Time.time - lastShootTime > shootCooldown)
            {
                lastShootTime = Time.time;
                Shoot();
            }
        }
    }

    void Shoot()
    {
        Vector3 pos = magicHandGestures.magicHand.GetKeyPoint(8);
        Vector3 direction = (magicHandGestures.magicHand.GetKeyPoint(8) - magicHandGestures.magicHand.GetKeyPoint(6)).normalized;
        GameObject web = Instantiate(webPrefab, transform);
        web.transform.position = pos + direction.normalized * spawnOffset;
        web.transform.rotation = Quaternion.LookRotation(direction);
        Rigidbody rb = web.GetComponent<Rigidbody>();
        rb.velocity = direction * impulseForce;   // starts moving immediately this frame

       // rb.AddForce(direction.normalized * impulseForce, ForceMode.Impulse);
        Destroy(rb.gameObject, webLifeTime);
    }

}
