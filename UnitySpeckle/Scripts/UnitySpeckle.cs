using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using System.Linq;
using SpeckleCore;
using Newtonsoft.Json;

using System.Reflection;

public class UnitySpeckle : MonoBehaviour
{
   
    public string StreamID;

    //TODO - Enable login
    //Currently URL and AuthToken are being defined directly in UnityReceiver

    //public string ServerURL;
    //public string UserName; /
    //public string Password;

    //Prefabs for displaying objects. Maybe easier to simply build these in code than having prefabs
    public GameObject prefab;
    public GameObject LinePrefab;

    public UnityEvent OnUpdateRecieved; //Provide event to access outside unity speckle

    private UnityReceiver Receiver;
   
    
    // Use this for initialization
    void Start()
    {
        if (OnUpdateRecieved == null)
            OnUpdateRecieved = new UnityEvent();

        Receiver = transform.gameObject.AddComponent<UnityReceiver>();
        Receiver.Init(StreamID);

    }

    // Update is called once per frame
    void Update()
    {
                        
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        if (Receiver != null)
            Receiver.Client.Dispose(true);
    }

}
