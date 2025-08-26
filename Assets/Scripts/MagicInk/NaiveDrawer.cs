using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class NaiveDrawer : InkDrawerBase
{
    [SerializeField] float DrawSpaceInterval = 0.01f;
    [SerializeField] GameObject ObjectToAddPrefab;

    struct Point
    {
        public GameObject go;
        public float time;
    }

    Vector3 _lastDrawPosition;
    bool _newCurveStarted = false;
    List<List<Point>> _history = new List<List<Point>>();
    
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
        _history.Add(new List<Point>());
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
        while (_history.Count > 0 && _history.Last().Count == 0)
        {
            _history.RemoveAt(_history.Count - 1);
        }
        if (_history.Count == 0)
        {
            return;
        }
        foreach (Point p in _history.Last())
        {
            Destroy(p.go);
        }
        _history.RemoveAt(_history.Count - 1);
    }

    public override void ClearRecent(float timeDelta)
    {
        if (_history.Count == 0)
        {
            return;
        }
        List<Point> lastCurve = _history.Last();
        float threshold = Time.time - timeDelta;
        while (lastCurve.Count > 0 && lastCurve.Last().time > threshold)
        {
            Point p = lastCurve.Last();
            Destroy(p.go);
            lastCurve.RemoveAt(lastCurve.Count - 1);
        }
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
        Point p = new Point();
        p.go = go;
        p.time = Time.time;
        _history.Last().Add(p);
    }
}
