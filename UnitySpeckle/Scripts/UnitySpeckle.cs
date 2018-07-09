using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using System.Linq;
using SpeckleCore;
using Newtonsoft.Json;

using System.Reflection;

[System.Serializable]
public class ReceiverEvent : UnityEvent<UnityReceiver>
{
}

public class UnitySpeckle : MonoBehaviour
{
      
    public List<string> StreamIDs;
    public string ServerURL;

    //TODO - Enable login
    //Currently URL and AuthToken are being defined directly in UnityReceiver

    
    //public string UserName; 
    //public string Password;

    //Prefabs for displaying objects. Maybe easier to simply build these in code than having prefabs
    public GameObject MeshPrefab;
    public GameObject LinePrefab;
    public GameObject PointPrefab;

    //public delegate void OnUpdateReceived(UnityReceiver rec);
    public ReceiverEvent OnReceiverCreated; //Called after the initial creation of the stream
    public ReceiverEvent OnUpdateReceived; //Provide event to access outside unity speckle

    private UnityReceiver Receiver;
   
    
    // Use this for initialization
    void Start()
    {
        if (OnUpdateReceived == null)
            OnUpdateReceived = new ReceiverEvent();

        foreach (var stream in StreamIDs)
        {
            Receiver = transform.gameObject.AddComponent<UnityReceiver>();
            Receiver.Init(stream, ServerURL);
        }
              

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
