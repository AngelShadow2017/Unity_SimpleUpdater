using System;
using UnityEngine;
[DefaultExecutionOrder(int.MaxValue)]
public class Killer : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log("Killer is coming");
        DestroyImmediate(gameObject);
    }
}
