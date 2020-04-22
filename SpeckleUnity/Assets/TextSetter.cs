using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SpeckleUnity;

[RequireComponent (typeof (Text))]
public class TextSetter : MonoBehaviour
{
    Text label;

    // Start is called before the first frame update
    void Start ()
    {
        label = GetComponent<Text> ();
    }

    public void SetText (SpeckleUnityUpdate updateData)
    {
        label.text = string.Format ("Download Progress: {0}%", Mathf.Floor (updateData.updateProgress * 100));
    }
}
