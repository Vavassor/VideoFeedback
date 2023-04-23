
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class FloorLevel : UdonSharpBehaviour
{
    public Slider slider;

    private BoxCollider boxCollider;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    public void OnChangeFloorLevel()
    {
        boxCollider.size = new Vector3(boxCollider.size.x, 2.0f * slider.value, boxCollider.size.z);
    }
}
