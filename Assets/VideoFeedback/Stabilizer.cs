
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
    private bool hasPriorPlayerPosition = false;
    private Vector3 priorPlayerPosition;
    private VRCPlayerApi priorPlayer;
    private Vector3[] samplePositions = new Vector3[64];
    private Quaternion[] sampleRotations = new Quaternion[64];
    private bool shouldResetNextUpdate = false;
    private bool wasStabilizing = false;
    private bool wasHeld = false;

    public void OnTeleport()
    {
        if (IsStabilizing)
        {
            shouldResetNextUpdate = true;
        }
    }

    private void Update()
    {
        UpdateStabilizingState();

        if (IsStabilizing)
        {
            SampleParentTransform();
            UpdateTransform();
        }
    }

    private void SampleParentTransform()
    {
        if (shouldResetNextUpdate || trackingPickup != null && trackingPickup.IsHeld && !wasHeld && trackingPickup.currentPlayer != null)
        {
            // When picked up or teleported, reinitialize the history of samples.
            SetAllSamplesToCurrentTransform();
            shouldResetNextUpdate = false;
        }
        else if (trackingPickup != null && !trackingPickup.IsHeld && wasHeld && hasPriorPlayerPosition)
        {
            // When dropped, retain the samples so that the camera comes to a smooth stop.
            // However, convert them so that they're no longer relative to the player position.
            ShiftSamplesWithOffset(priorPlayerPosition);
            hasPriorPlayerPosition = false;
            priorPlayer = null;
        }
        else if (trackingPickup != null && priorPlayer != null && trackingPickup.currentPlayer != null && trackingPickup.currentPlayer != priorPlayer && hasPriorPlayerPosition)
        {
            // When the pickup is stolen, convert the samples so they're relative to the thief.
            var playerPosition = trackingPickup.currentPlayer.GetPosition();
            ShiftSamplesWithOffset(priorPlayerPosition - playerPosition);
        }
        else
        {
            ShiftSamplesWithOffset(Vector3.zero);
        }

        // Sample the current transform.
        samplePositions[0] = trackingTransform.position;
        sampleRotations[0] = trackingTransform.rotation;

        // Store the sample position relative to the camera when the camera is held.
        // Also, update the hold state.
        if (trackingPickup != null)
        {
            if (trackingPickup.IsHeld && trackingPickup.currentPlayer != null)
            {
                var playerPosition = trackingPickup.currentPlayer.GetPosition();
                samplePositions[0] -= playerPosition;
                priorPlayerPosition = playerPosition;
                hasPriorPlayerPosition = true;
                priorPlayer = trackingPickup.currentPlayer;
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
        if (trackingPickup != null && trackingPickup.IsHeld && trackingPickup.currentPlayer != null)
        {
            smoothPosition += trackingPickup.currentPlayer.GetPosition();
        }

        // Update the transform.
        transform.SetPositionAndRotation(smoothPosition, smoothRotation);
    }

    private void UpdateStabilizingState()
    {
        if (!wasStabilizing && IsStabilizing)
        {
            // When stabilizing starts, reinitialize the history of samples.
            SetAllSamplesToCurrentTransform();
        }

        wasStabilizing = IsStabilizing;
    }

    /// <summary>
    /// Move each of the prior samples back one frame.
    /// </summary>
    /// <param name="positionOffset">A positional offset, if the samples are relative to another object.</param>
    private void ShiftSamplesWithOffset(Vector3 positionOffset)
    {
        for (var i = samplePositions.Length - 1; i >= 1; i--)
        {
            samplePositions[i] = samplePositions[i - 1] + positionOffset;
            sampleRotations[i] = sampleRotations[i - 1];
        }
    }

    /// <summary>
    /// Set all samples to the current tracking position.
    /// </summary>
    private void SetAllSamplesToCurrentTransform()
    {
        var playerPosition = Vector3.zero;
        if (trackingPickup != null && trackingPickup.IsHeld && trackingPickup.currentPlayer != null)
        {
            playerPosition = trackingPickup.currentPlayer.GetPosition();
        }

        for (var i = 0; i < samplePositions.Length; i++)
        {
            samplePositions[i] = trackingTransform.position - playerPosition;
            sampleRotations[i] = trackingTransform.rotation;
        }
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
