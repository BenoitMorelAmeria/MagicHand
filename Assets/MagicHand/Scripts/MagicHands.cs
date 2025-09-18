using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Net.Mail;
using UnityEngine;

public class MagicHands : MonoBehaviour
{
    [SerializeField] List<MagicHand> hands;
    [SerializeField] HandPoseManager handPoseManager;

    Dictionary<int, int> labelToHandIndex = new Dictionary<int, int>();
    HashSet<int> assignedHandIndices = new HashSet<int>();
    [SerializeField] bool debugMouse = false;

    // Start is called before the first frame update
    void Start()
    {
        foreach (MagicHand hand in hands)
        {
            hand.gameObject.SetActive(false);
        }
        MqttHandPose.OnKeypointsReceived += UpdateHand;
        MqttHandPose.OnHandPoseDetected += UpdateHandPoseDetected;
        MqttHandPose.OnPinchStateReceived += UpdatePinchState;
    }


    // Update is called once per frame
    void Update()
    {
        if (debugMouse)
        {
            hands[0].gameObject.SetActive(true);
        }
        if (handPoseManager != null)
        {
            Debug.Log("hands: " + handPoseManager.GetHandsCount());
            hands[0].SetHandPoseEnabled(handPoseManager.GetHandsCount() > 0);
            if (handPoseManager.GetHandsCount() > 0)
            {
                List<Vector3> keypoints = new List<Vector3>();
                for (int i = 0; i < 21; ++i)
                {
                    Vector3 v = handPoseManager.GetKeypointPosition(0, i);
                    v.z = -v.z; // Invert Z axis
                    keypoints.Add(v);
                }
                hands[0].UpdateHand(keypoints);
            }
        }
    }

    public void UpdateHandPoseDetected(bool detected)
    {
        if (!detected)
        {
            // If no hand pose is detected, deactivate all hands
            foreach (MagicHand hand in hands)
            {
                hand.gameObject.SetActive(false);
                hand.SetHandPoseEnabled(false);
            }
            labelToHandIndex.Clear();
            assignedHandIndices.Clear();
        } else
        {
            hands[0].gameObject.SetActive(true);
            hands[0].SetHandPoseEnabled(true);
        }
    }

    private void OnEnable()
    {
        MqttHandPose.OnKeypointsReceived += UpdateHand;
        MqttHandPose.OnHandPoseDetected += UpdateHandPoseDetected;
        MqttHandPose.OnPinchStateReceived += UpdatePinchState;
    }
    private void OnDisable()
    {
        MqttHandPose.OnKeypointsReceived -= UpdateHand;
        MqttHandPose.OnHandPoseDetected -= UpdateHandPoseDetected;
        MqttHandPose.OnPinchStateReceived -= UpdatePinchState;
    }
    public void UpdatePinchState(bool pinchState)
    {
        foreach (MagicHand hand in hands)
        {
            hand.SetPinchState(pinchState);
        }
    }

    public void UpdateHand(List<HandKeypoints> inputHands)
    {
        if (inputHands.Count > 0)
        {
            hands[0].UpdateHand(inputHands[0].Keypoints);
        }
    }
  

}
