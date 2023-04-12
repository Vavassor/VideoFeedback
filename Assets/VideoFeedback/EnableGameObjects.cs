using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EnableGameObjects : UdonSharpBehaviour
{
    public GameObject[] gameObjects;
    public GameObject[] behaviourObjects;

    override public void OnPlayerTriggerEnter(VRCPlayerApi playerApi)
    {
        foreach (var obj in gameObjects)
        {
            obj.SetActive(true);
        }

        foreach (var obj in behaviourObjects)
        {
            var behaviours = obj.GetComponents<UdonBehaviour>();
            foreach (var behaviour in behaviours)
            {
                behaviour.enabled = true;
            }
        }
    }

    override public void OnPlayerTriggerExit(VRCPlayerApi playerApi)
    {
        foreach (var obj in gameObjects)
        {
            obj.SetActive(false);
        }

        foreach (var obj in behaviourObjects)
        {
            var behaviours = obj.GetComponents<UdonBehaviour>();
            foreach (var behaviour in behaviours)
            {
                behaviour.enabled = false;
            }
        }
    }
}