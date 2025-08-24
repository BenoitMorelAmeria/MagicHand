using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            Destroy(collision.gameObject); // destroy the projectile 
            StartDissolving(); // start the dissolving effect
        }
    }

    void StartDissolving()
    {
        if (GetDissolveAmount() == 0.0f) { 
            StartCoroutine(DissolveRoutine());
        }
    }


    private void SetDissolveAmount(float amount)
    {
        Material material = GetComponent<Renderer>().material;
        if (material != null)
        {
            material.SetFloat("_DissolveAmount", amount);
        }
    }
    private float GetDissolveAmount()
    {
        Material material = GetComponent<Renderer>().material;
        if (material == null)
        {
            return 0.0f;
        }
        float dissolveValue = material.GetFloat("_DissolveAmount");
        return dissolveValue;
    }

    private IEnumerator DissolveRoutine()
    {
        //bool dropped = false;
        SetDissolveAmount(0.2f);
        while (GetDissolveAmount() < 1.0f)
        {
            float dissolveAmount = GetDissolveAmount();
            /*
            if (dissolveAmount > 0.5f && !dropped)
            {
                dropped = true;
                Drop();
            }
            */
            SetDissolveAmount(dissolveAmount + Time.deltaTime * 0.5f);

            yield return null;
        }
        Destroy(gameObject); // destroy the asteroid after dissolving
    }
}
