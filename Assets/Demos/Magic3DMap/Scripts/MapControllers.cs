
using System.Collections.Generic;
using UnityEngine;

public class MapControllers : MonoBehaviour
{


    [SerializeField] public Transform sceneRoot; // we simulate the camera by moving part of the scene
    [SerializeField] public CameraManager cameraManager;
    [SerializeField] public List<GameObject> controllers;
    [SerializeField] private bool moveCamera = false;
    private int _currentIndex = 0;

    public void Start()
    {
        SetControllerIndex(_currentIndex);
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            _currentIndex = (_currentIndex + 1) % controllers.Count;
            SetControllerIndex(_currentIndex);
        }
    }

    private void SetControllerIndex(int index)
    {
        _currentIndex = index;
        for (int i = 0; i < controllers.Count; i++)
        {
            controllers[i].SetActive(i == _currentIndex);
        }
    }

    GameObject GetController()
    {
        return controllers[_currentIndex];
    }



    void LateUpdate()
    {
        Transform t = GetController().transform;
        if (moveCamera)
        {
            // Move the camera normally
            cameraManager.camerasParentTransform.SetPositionAndRotation(
                t.position,
                t.rotation
            );
        }
        else
        {
            // Move the world instead
            Quaternion inverseRot = Quaternion.Inverse(t.rotation);
            Vector3 inversePos = -(inverseRot * t.position);
            sceneRoot.SetPositionAndRotation(inversePos, inverseRot);
        }
    }
}