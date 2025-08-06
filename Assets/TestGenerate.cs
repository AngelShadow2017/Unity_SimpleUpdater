using System;
using UnityEngine;
using ZeroAs.ZeroAs_Core.ManualUpdaters;

[ManualUpdater(ManualUpdateManager.UpdateOrder.FakePosition)]
public partial class TestGenerate : MonoBehaviour
{
    private static int maxNum = 6;
    private void Awake()
    {
    }

    void ManualEnable()
    {
        //Debug.Log("ManualEnable called");
    }

    void ManualStart()
    {
        Debug.Log("ManualStart called");
        if (maxNum-- > 0)
        {
            gameObject.AddComponent<TestGenerate>();

        }
    }

    void ManualUpdate()
    {
        //Debug.Log("Update called");
    }
    /*void ManualFixedUpdate()
    {
        Debug.Log("FixedUpdate called");
    }
    void ManualDisable()
    {
        Debug.Log("ManualDisable called");
    }*/
}