
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Stabilizer : UdonSharpBehaviour
{
    public VRC_Pickup trackingPickup;
    public Transform trackingTransform;
    public int smoothingFrames = 10;

    public bool IsStabilizing
    {
        get { return isStabilizing; }
        set { isStabilizing = value; }
    }

    private bool isStabilizing = true;
    private Vector3[] samplePositions = new Vector3[64];
    private Quaternion[] sampleRotations = new Quaternion[64];
    private bool wasHeld = false;

    private void Start()
    {
        // Initialize all samples to be the current transform.
        for (var i = 0; i < samplePositions.Length; i++)
        {
            samplePositions[i] = trackingTransform.position;
            sampleRotations[i] = trackingTransform.rotation;
        }
    }

    private void Update()
    {
        if (IsStabilizing)
        {
            SampleParentTransform();
            UpdateTransform();
        }
    }

    private void SampleParentTransform()
    {
        if (trackingPickup != null && trackingPickup.IsHeld != wasHeld)
        {
            // When picked up or dropped, reinitialize the history of samples.
            for (var i = 0; i < samplePositions.Length; i++)
            {
                samplePositions[i] = trackingTransform.position - trackingPickup.currentPlayer.GetPosition();
                sampleRotations[i] = trackingTransform.rotation;
            }
        }
        else
        {
            // Move each of the prior samples back one frame.
            for (var i = samplePositions.Length - 1; i >= 1; i--)
            {
                samplePositions[i] = samplePositions[i - 1];
                sampleRotations[i] = sampleRotations[i - 1];
            }
        }

        samplePositions[0] = trackingTransform.position;
        sampleRotations[0] = trackingTransform.rotation;

        if (trackingPickup != null)
        {
            if (trackingPickup.IsHeld)
            {
                samplePositions[0] -= trackingPickup.currentPlayer.GetPosition();
            }

            wasHeld = trackingPickup.IsHeld;
        }
    }

    private void UpdateTransform()
    {
        // Computed a weighted moving average of the transforms in the last few frames.

        // Find the weighted sum of the samples.
        var weightSum = 0.0f;
        var positionSum = Vector3.zero;
        var rotationSum = Vector4.zero;

        for (var i = 0; i < smoothingFrames; i++)
        {
            float weight = WindowTriangular(i, smoothingFrames);
            positionSum += weight * samplePositions[i];
            rotationSum += weight * GetVector4FromQuaternion(sampleRotations[i]);
            weightSum += weight;
        }

        // Normlize the sums.
        var smoothPosition = positionSum / weightSum;
        var smoothRotation = GetQuaternionFromVector4(rotationSum / weightSum).normalized;

        // The position is relative to the pickup when held. So, transform it to a world position.
        if (trackingPickup != null && trackingPickup.IsHeld)
        {
            smoothPosition += trackingPickup.currentPlayer.GetPosition();
        }

        // Update the transform.
        transform.SetPositionAndRotation(smoothPosition, smoothRotation);
    }

    private float WindowTriangular(int x, int n)
    {
        return 1.0f - Mathf.Abs((x - (n / 2.0f)) / ((n + 1.0f) / 2.0f));
    }

    private Vector4 GetVector4FromQuaternion(Quaternion q)
    {
        return new Vector4(q.x, q.y, q.z, q.w);
    }

    private Quaternion GetQuaternionFromVector4(Vector4 v)
    {
        return new Quaternion(v.x, v.y, v.z, v.w);
    }
}
