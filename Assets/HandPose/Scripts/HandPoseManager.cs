using System.Collections.Generic;
using UnityEngine;

public class HandPoseManager : MonoBehaviour
{
    [SerializeField] private MonoBehaviour providerBehaviour;
    [SerializeField] List<MagicHand> hands;

    private IHandPoseProvider provider;
    public HandPoseFrameData data;

    private void Awake()
    {
        provider = providerBehaviour as IHandPoseProvider;
        if (provider == null && providerBehaviour != null)
        {
            Debug.LogError($"{providerBehaviour.name} does not implement IHandPoseProvider!");
        }
    }

    private void Update()
    {
        if (provider != null)
        {
            // should we rather let the provider push updates?
            UpdateHandPoseFrameData(provider.GetLatestHandPoseFrame());
        }
        Debug.Log("Hand number: " + data.Hands.Count);
    }

    public void UpdateHandPoseFrameData(HandPoseFrameData frame)
    {
        data = frame;
        if (hands.Count == 0) return;

        if (data.Hands.Count > 0)
        {
            hands[0].UpdateHand(new List<Vector3>(data.Hands[0].Keypoints));
            hands[0].SetHandPoseEnabled(true);
        }
        else
        {
            hands[0].SetHandPoseEnabled(false);
        }
    }
}