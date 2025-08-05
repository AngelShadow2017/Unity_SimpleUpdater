using UnityEngine;
using UnityEngine.InputSystem;
using ZeroAs.ZeroAs_Core.ManualUpdaters;

public class PauserScript : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (!ManualUpdateManager.pauseLock.IsOpen)
            {
                ManualUpdateManager.pauseLock.Open();
            }
            else
            {
                ManualUpdateManager.pauseLock.Close();
            }
        }
    }
}
