using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class NaiveDrawer : InkDrawerBase
{
    [SerializeField] float DrawSpaceInterval = 0.01f;
    [SerializeField] GameObject ObjectToAddPrefab;

    Vector3 _lastDrawPosition;
    bool _newCurveStarted = false;
    List<List<GameObject>> _lastCurve = new List<List<GameObject>>();
    int _currentColorIndex = 0;
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public override void StartNewCurve()
    {
        _newCurveStarted = true;
        _lastCurve.Add(new List<GameObject>());
    }

    public override void NextPoint(Vector3 p, Color color, float brushSize)
    {
        if (_newCurveStarted)
        {
            _lastDrawPosition = p;
            AddInk(_lastDrawPosition, color, Quaternion.identity, brushSize);
            _newCurveStarted = false;
            return;
        }
        while ((p - _lastDrawPosition).magnitude > DrawSpaceInterval * brushSize)
        {
            Vector3 diff = p - _lastDrawPosition;
            _lastDrawPosition += diff.normalized * DrawSpaceInterval * brushSize;
            AddInk(_lastDrawPosition, color, Quaternion.identity, brushSize);
            
        }
    }

    public override void Rollback()
    {
        while (_lastCurve.Count > 0 && _lastCurve.Last().Count == 0)
        {
            _lastCurve.RemoveAt(_lastCurve.Count - 1);
        }
        if (_lastCurve.Count == 0)
        {
            return;
        }
        foreach (GameObject go in _lastCurve.Last())
        {
            Destroy(go);
        }
        _lastCurve.RemoveAt(_lastCurve.Count - 1);
    }

    private void AddInk(Vector3 position, Color color, Quaternion orientation, float brushSize)
    {
        GameObject go = Instantiate(ObjectToAddPrefab, transform);
        go.transform.position = position;
        go.transform.rotation = orientation;
        go.transform.localScale *= brushSize;
        Material mat = go.GetComponent<Renderer>().material;
        mat.SetColor("_EmissionColor", color);
        mat.color = color;

        _lastCurve.Last().Add(go);
    }
}
