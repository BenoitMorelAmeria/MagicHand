using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSaber : MonoBehaviour
{
    [SerializeField] Transform lightScaleTransform;
    [SerializeField] float openDuration = 0.1f; // Duration of the opening animation
    enum State
    {
        Opened,
        Closed,
        Opening,
        Closing
    }

    State state = State.Closed;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (state == State.Closed)
            {
                StartCoroutine(OpenSaber());
            }
            else if (state == State.Opened)
            {
                StartCoroutine(CloseSaber());
            }
        }
    }


    private IEnumerator OpenSaber()
    {
        float duration = openDuration; // Duration of the animation
        float elapsedTime = 0f;
        float initialLength = lightScaleTransform.localScale.y;
        float targetLength = 1.0f; // Target length of the saber
        state = State.Opening;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float newLength = Mathf.Lerp(initialLength, targetLength, t);
            SetSaberLength(newLength);
            yield return null;
        }
        SetSaberLength(targetLength); // Ensure it ends at the target length
        state = State.Opened;
    }

    private IEnumerator CloseSaber()
    {
        float duration = openDuration; // Duration of the animation
        float elapsedTime = 0f;
        float initialLength = lightScaleTransform.localScale.y;
        float targetLength = 0.0f; // Target length of the saber
        state = State.Closing;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float newLength = Mathf.Lerp(initialLength, targetLength, t);
            SetSaberLength(newLength);
            yield return null;
        }
        SetSaberLength(targetLength); // Ensure it ends at the target length
        state = State.Closed;
    }

    private void SetSaberLength(float length)
    {
        lightScaleTransform.localScale = new Vector3(1.0f, length, 1.0f);
    }




}
