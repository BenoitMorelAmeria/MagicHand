using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Net.Mail;
using UnityEngine;

public class MagicHands : MonoBehaviour
{
    [SerializeField] List<MagicHand> hands;

    Dictionary<int, int> labelToHandIndex = new Dictionary<int, int>();
    HashSet<int> assignedHandIndices = new HashSet<int>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (MagicHand hand in hands)
        {
            hand.gameObject.SetActive(false);
        }
        MqttHandPose.OnKeypointsReceived += UpdateHand;
        MqttHandPose.OnHandPoseDetected += UpdateHandPoseDetected;
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateHandPoseDetected(bool detected)
    {
        if (!detected)
        {
            // If no hand pose is detected, deactivate all hands
            foreach (MagicHand hand in hands)
            {
                hand.gameObject.SetActive(false);
            }
            labelToHandIndex.Clear();
            assignedHandIndices.Clear();
        }
    }

    private void OnEnable()
    {
        MqttHandPose.OnKeypointsReceived += UpdateHand;
    }
    private void OnDisable()
    {
        MqttHandPose.OnKeypointsReceived -= UpdateHand;
    }

    public void UpdateHand(List<HandKeypoints> inputHands)
    {
        // detect which assigned indices are NOT in the input hands
        HashSet<int> labelsToRemove = new HashSet<int>();
        foreach (var kvp in labelToHandIndex)
        {
            int label = kvp.Key;
            bool found = false;
            foreach (HandKeypoints inputHand in inputHands)
            {
                if (inputHand.Label == label)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                labelsToRemove.Add(label);
            }
        }
        foreach (int label in labelsToRemove)
        {

            int index = labelToHandIndex[label];;
            labelToHandIndex.Remove(label);
            assignedHandIndices.Remove(index);
            hands[index].gameObject.SetActive(false); // deactivate the hand
        }
        

        // update labelToHandIndex with new input hands
        foreach (HandKeypoints inputHand in inputHands)
        {
            int label = inputHand.Label;
            if (assignedHandIndices.Contains(label))
            {
                // this label is already assigned, skip it
                continue;
            }
            else
            {
                // we need to find a free index to assign to this new label
                int freeIndex = 0;
                while (freeIndex < hands.Count && assignedHandIndices.Contains(freeIndex))
                {
                    freeIndex++;
                }
                if (freeIndex >= hands.Count)
                {
                    Debug.LogWarning($"No free hand index available for label {label}");
                    continue;
                }
                labelToHandIndex[label] = freeIndex;
                assignedHandIndices.Add(freeIndex);
                hands[freeIndex].gameObject.SetActive(true); // activate the hand
            }
        }

        // now the labelToHandIndex should have all the labels mapped to their assigned indices
        // we can update the hands based on this mapping
        foreach (HandKeypoints inputHand in inputHands)
        {
            int label = inputHand.Label;
            Debug.Log("Label: " + label + ", Index: " + labelToHandIndex[label]);
           
            MagicHand magicHand = hands[labelToHandIndex[label]];
            List<Vector3> keyPoints = inputHand.Keypoints;
            magicHand.UpdateHand(keyPoints);
        }
    }
}
