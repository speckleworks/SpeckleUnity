using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using SpeckleCore;
using Newtonsoft.Json;

using System.Reflection;

public class UnityReceiver : MonoBehaviour
{

    public SpeckleApiClient Client { get; set; }
    public List<SpeckleObject> SpeckleObjects { get; set; }
    public List<object> ConvertedObjects;
    
    private Dictionary<String, SpeckleObject> ObjectCache = new Dictionary<string, SpeckleObject>();
    private bool bUpdateDisplay = false;
    private bool bRefreshDisplay = false;

    public GameObject rootGameObject;
    
    private string authToken = "put auth token here"; //TODO - actually login to get this
    private string StreamID;

    

    // Use this for initialization
    void Start()
    {
                

    }

    // Update is called once per frame
    void Update()
    {

        if (bUpdateDisplay)
        {
            //Initial creation of objects
            bUpdateDisplay = false;
            CreateObjects();

            //call event on SpeckleManager to allow users to do their own thing when a stream is updated
            transform.GetComponent<UnitySpeckle>().OnUpdateReceived.Invoke(this);
        }
        if (bRefreshDisplay)
        {
            //Clears existing objects first
            bRefreshDisplay = false;
            RefreshObjects();
        }

    }

    //Wrapper to call coroutine
    public void Init(string inStreamID, string URL) 
    {
        StartCoroutine(InitAsync(inStreamID, URL));       
    }

    //Initialize receiver
    IEnumerator InitAsync(string inStreamID, string URL)
    {
        Client = new SpeckleApiClient(URL);

        //Assign events
        Client.OnReady += Client_OnReady;
        Client.OnLogData += Client_OnLogData;
        Client.OnWsMessage += Client_OnWsMessage;
        Client.OnError += Client_OnError;

        //make sure convereter is loaded
        var hack = new ConverterHack();

        SpeckleObjects = new List<SpeckleObject>();

        StreamID = inStreamID;
        Client.IntializeReceiver(StreamID, "UnityTest", "Unity", "UnityGuid", authToken);

        var streamGetResponse = Client.StreamGetAsync(StreamID, null);
        while (!streamGetResponse.IsCompleted) yield return null;

        Client.Stream = streamGetResponse.Result.Resource;

        if (rootGameObject == null)
            rootGameObject = new GameObject(Client.Stream.Name);

        //call event on SpeckleManager to allow users to do their own thing when a stream is created
        transform.GetComponent<UnitySpeckle>().OnReceiverCreated.Invoke(this);

        UpdateGlobal();
    }

    //Wrapper for update coroutine
    public void UpdateGlobal()
    {        
        StartCoroutine(UpdateGlobalAsync());
    }

    IEnumerator UpdateGlobalAsync()
    {
        SpeckleObjects.Clear();
        ObjectCache.Clear();
        var streamGetResponse = Client.StreamGetAsync(StreamID, null).Result;
        if (streamGetResponse.Success == false)
        {
            Debug.Log(streamGetResponse.Message);
        }

        Client.Stream = streamGetResponse.Resource;
        
        Debug.Log("Getting objects....");
        var payload = Client.Stream.Objects.Select(obj => obj._id).ToArray();
	
	    var getTask = Client.ObjectGetBulkAsync(payload, null);
        while (!getTask.IsCompleted) yield return null;
        var getObjectResult = getTask.Result;
               
        foreach (var x in getObjectResult.Resources)
            ObjectCache.Add(x._id, x);
        
        foreach (var obj in Client.Stream.Objects)
            SpeckleObjects.Add(ObjectCache[obj._id]);             

        bUpdateDisplay = true;       
    }

    public void RefreshObjects()
    {
        //Clear existing objects
        //TODO - update existing objects instead of destroying/recreating all of them
        foreach (var co in ConvertedObjects)
        {
            GameObject tempObj = (GameObject)co;
            Destroy(tempObj);
        }
        ConvertedObjects.Clear();

        UpdateGlobal();        
    }

    public void CreateObjects()
    {
        //Generate native GameObjects with methods from SpeckleUnityConverter 
        ConvertedObjects = SpeckleCore.Converter.Deserialise(SpeckleObjects);
        

        foreach (GameObject go in ConvertedObjects)        
            go.transform.SetParent(rootGameObject.transform, false);
        

        ////Set layer information
        int objectCount = 0;
        GameObject LayerObject;
        foreach (var layer in Client.Stream.Layers)
        {
            string LayerName = layer.Name;
                        
            LayerObject = (GameObject.Find(LayerName));
            if (LayerObject == null)
            {
                LayerObject = new GameObject(LayerName);
                LayerObject.transform.SetParent(rootGameObject.transform);
            }
            
            for (int i = 0; i < layer.ObjectCount; i++)
            {
                GameObject go = (GameObject)ConvertedObjects[objectCount];
                go.GetComponent<UnitySpeckleObjectData>().LayerName = LayerName;
                go.transform.SetParent(LayerObject.transform);               
                objectCount++;
            }
        }

        
    }
    

    public virtual void Client_OnReady(object source, SpeckleEventArgs e)
    {
        Debug.Log("Client ready");
        //Debug.Log(JsonConvert.SerializeObject(e.EventData));
    }
    public virtual void Client_OnLogData(object source, SpeckleEventArgs e)
    {
        //Debug.Log("Client LogData");
        //Debug.Log(JsonConvert.SerializeObject(e.EventData));
    }
    public virtual void Client_OnWsMessage(object source, SpeckleEventArgs e)
    {
        //Debug.Log("Client WsMessage");
        //Debug.Log(JsonConvert.SerializeObject(e.EventData));

        //Set refresh to true to prompt recreating geometry
        bRefreshDisplay = true;
        
    }
    public virtual void Client_OnError(object source, SpeckleEventArgs e)
    {
        //Debug.Log("Client Error");
        //Debug.Log(JsonConvert.SerializeObject(e.EventData));
    }
}
