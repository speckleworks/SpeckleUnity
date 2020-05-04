using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpeckleUnity;

public class CycleRenderingRules : MonoBehaviour
{
    public RenderingRule[] rules;
    public SpeckleUnityManager manager;

    private int currentIndex = 0;

    private void Awake ()
    {
        manager.SetRenderingRule (rules[currentIndex]);
    }

    public void CycleRule ()
    {
        currentIndex++;

        if (currentIndex >= rules.Length) currentIndex = 0;

        manager.SetRenderingRule (rules[currentIndex]);
    }
}
