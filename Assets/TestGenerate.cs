using System;
using UnityEngine;
using ZeroAs.ZeroAs_Core.ManualUpdaters;

[ManualUpdater(ManualUpdateManager.UpdateOrder.FakePosition)]
public partial class TestGenerate : MonoBehaviour
{
    void Start()
    {
        OnEnable();
    }
    void ManualEnable()
    {
        Debug.Log("ManualEnable called");
    }

    void ManualUpdate()
    {
        Debug.Log("Update called");
    }
    void ManualFixedUpdate()
    {
        Debug.Log("FixedUpdate called");
    }
    void ManualDisable()
    {
        Debug.Log("ManualDisable called");
    }
}
