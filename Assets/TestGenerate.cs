using System;
using UnityEngine;
using ZeroAs.ZeroAs_Core.ManualUpdaters;

[ManualUpdater]
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
    void ManualDisable()
    {
        Debug.Log("ManualDestroy called");
    }
}
