using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolvableObject : MonoBehaviour
{
    [SerializeField] private Material dissolvableMaterial;
    private Material _material;

    // Start is called before the first frame update
    void Start()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            // Ensure the material is set to the dissolvable material
            renderer.material = Instantiate(dissolvableMaterial);
            _material = renderer.material;
        }
        else
        {
            Debug.LogError("MeshRenderer not found on the GameObject. Please attach a MeshRenderer component.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(TriggerDissolve());
        }
    }

    private float GetDissolveValue()
    {
        return _material.GetFloat("_DissolveAmount");
    }

    private void SetDissolveValue(float value)
    {
        _material.SetFloat("_DissolveAmount", value);
    }

    private IEnumerator TriggerDissolve()
    {        
        while (GetDissolveValue() < 1.0f)
        {
            SetDissolveValue(GetDissolveValue() + Time.deltaTime * 0.5f);
            yield return null;
        }
        SetDissolveValue(0.0f);
    }
}
