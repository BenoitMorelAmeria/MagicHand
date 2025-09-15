using UnityEngine;

public class HandAndPinchController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] public MagicHandGestures magicHandGestures;

    [Header("Hand pose")]
    [SerializeField] private float handZCenter = 0.2f;
    [SerializeField] private float handPoseForwardSpeed = 10f;
    [SerializeField] private Vector3 handDeadZone = Vector3.zero;

    [SerializeField] private Vector3 interactionAreaMin = new Vector3(-1, -1, -1);
    [SerializeField] private Vector3 interactionAreaMax = new Vector3(1, 1, 1);

    [SerializeField] private float rotSpeedFromPosX = 200f;
    [SerializeField] private float rotSpeedFromPosY = 200f;

    [SerializeField] private float pinchTransSpeed = 10f;

    private bool mouseVisible = false;

    // Rotation state
    private Quaternion currentRotation = Quaternion.identity;
    private Quaternion handNeutral = Quaternion.LookRotation(Vector3.forward, Vector3.down);




    void Update()
    {
        HandleHandPos();
    }
    

      private bool IsPinching()
    {
        float dist = Vector3.Distance(
            magicHandGestures.magicHand.Data.GetKeypointScreenSpace(8),
            magicHandGestures.magicHand.Data.GetKeypointScreenSpace(4)
        );
        return dist < 0.03f;
    }


    static float ApplyDeadZone(float pos, float deadZone)
    {
        if (Mathf.Abs(pos) < deadZone)
        {
            return 0;
        }
        else if (pos > 0)
        {
            return pos - deadZone;
        }
        else
        {
            return pos + deadZone;
        }
    }

    private bool IsHandInInteractionArea()
    {
        Vector3 pos = magicHandGestures.magicHand.Data.GetKeypointScreenSpace(9);
        return (pos.x >= interactionAreaMin.x && pos.x <= interactionAreaMax.x) &&
               (pos.y >= interactionAreaMin.y && pos.y <= interactionAreaMax.y) &&
               (pos.z >= interactionAreaMin.z && pos.z <= interactionAreaMax.z);
    }

    public void HandleHandPos()
    {
        if (!magicHandGestures.magicHand.IsAvailable() || !IsHandInInteractionArea())
            return;



        Vector3 handPosePos = magicHandGestures.magicHand.Data.GetKeypointScreenSpace(9);
        Vector3 relativePos = handPosePos - new Vector3(0, 0, handZCenter);
        relativePos.x = ApplyDeadZone(relativePos.x, handDeadZone.x);
        relativePos.y = ApplyDeadZone(relativePos.y, handDeadZone.y);
        relativePos.z = ApplyDeadZone(relativePos.z, handDeadZone.z);


        if (IsPinching()) // pinch, we translate the scene
        {
            Vector3 deltaPos = relativePos * pinchTransSpeed * Time.deltaTime;
            deltaPos.z = 0;
            transform.position += currentRotation * deltaPos;
        } else {  // no pinch, classic FPS movement
            // Forward - backward movement with hand depth
            Vector3 move = (currentRotation * new Vector3(0, 0, relativePos.z))
                            * handPoseForwardSpeed * Time.deltaTime;
            transform.position += move;
            

            // Rotate left-right with hand x position
            float deltaRotationX = relativePos.x * rotSpeedFromPosX * Time.deltaTime;
            currentRotation = currentRotation * Quaternion.AngleAxis(deltaRotationX, Vector3.up);

            // Rotate up-down with hand y position
            float deltaRotationY = relativePos.y * rotSpeedFromPosY * Time.deltaTime;
            currentRotation = currentRotation * Quaternion.AngleAxis(deltaRotationY, Vector3.right);
        }
    }

}
