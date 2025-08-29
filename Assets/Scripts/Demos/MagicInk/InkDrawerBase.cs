using UnityEngine;
public abstract class InkDrawerBase : MonoBehaviour
{
    public abstract void StartNewCurve();
    public abstract void NextPoint(Vector3 p, Color color, float brushSize);
    public abstract void Rollback();

    public abstract void ClearRecent(float timeDelta);

}