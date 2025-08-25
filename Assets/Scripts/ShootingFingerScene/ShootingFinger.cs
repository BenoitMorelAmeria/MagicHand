using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingFinger : MonoBehaviour
{
    [SerializeField] MagicHand magicHand;
    [SerializeField] MagicHandGestures magicHandGestures;
    [SerializeField] GameObject ProjectilePrefab;
    [SerializeField] float shootTimeInterval = 1.0f;
    [SerializeField] float projVelocity = 0.5f;
    [SerializeField] AudioClip shootClip;

    private AudioSource shootSource; // Sound to play when opening the saber

    float timeSinceLastShoot;

    // Start is called before the first frame update
    void Start()
    {
        timeSinceLastShoot = Time.time;
        shootSource = gameObject.AddComponent<AudioSource>();
        shootSource.clip = shootClip;
        shootSource.playOnAwake = false;
        shootSource.loop = false;
        shootSource.volume = 0.5f;

    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - timeSinceLastShoot > shootTimeInterval)
        {
            timeSinceLastShoot = timeSinceLastShoot + shootTimeInterval;
            shoot();
        }
    }

    private void shoot()
    {
        if (!magicHand.IsAvailable() || !magicHandGestures.IndexPointing)
        {
            return;
        }
        Vector3 p1 = magicHand.GetKeyPoint(8);
        Vector3 p2 = magicHand.GetKeyPoint(7);
        Vector3 direction = (p1 - p2).normalized;
        Vector3 spawnPosition = p2 + direction * 0.05f; // Offset from the finger tip

        GameObject proj = Instantiate(ProjectilePrefab, spawnPosition, Quaternion.identity);
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        rb.velocity = direction * projVelocity;
        shootSource.Play();
    }
}
