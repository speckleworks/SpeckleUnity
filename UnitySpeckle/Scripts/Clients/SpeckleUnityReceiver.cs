using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using SpeckleCore;


    public class SpeckleUnityReceiver : SpeckleUnityClient
{
  
    public List<SpeckleObject> SpeckleObjects { get; set; }
    public List<object> ConvertedObjects = new List<object>();
            
    private Dictionary<String, SpeckleObject> ObjectCache = new Dictionary<string, SpeckleObject>();
      
    private bool messageReceived = false;
    private string messageContent;

    //Provides event to access outside unity speckle
    [Header("Events")]    
    public ReceiverEvent OnUpdateReceived; 

    private void Start()
    {
        if (OnUpdateReceived == null)
            OnUpdateReceived = new ReceiverEvent();
    }

    private void Update()
    {
        //Update global was not working when triggered from the socket events, due to threading conflicts?
        //Current solution is to set a value with the socket event, and then check every frame to see if an action needs to be taken
        OnWsMessageCheck(); 
    }

    /// <summary>
    /// Initialize Unity Receiver
    /// </summary>
    /// <param name="inStreamID"></param>
    /// <param name="URL"></param>
    public override void InitializeClient(string URL)
    {
        StartCoroutine(IntializeReceiverAsync(StreamId, URL));
    }
    private IEnumerator IntializeReceiverAsync(string inStreamID, string URL)
    {        
        Client = new SpeckleApiClient(URL, true);
        Client.BaseUrl = URL;
       
        AssignEvents();       
       
        //Initialize receiver
        Client.IntializeReceiver(StreamId, "UnityTest", "Unity", Guid.NewGuid().ToString(), authToken);
    
        //wait for receiver to be connected
        while (!Client.IsConnected) yield return null;

        SpeckleObjects = new List<SpeckleObject>();

        //after connected, call update global to get geometry
        UpdateGlobal();
       
    }

   

    
    public override void Client_OnWsMessage(object source, SpeckleEventArgs e)
    {        
        var wSMessageData = JsonUtility.FromJson<WSMessageData>(e.EventData);

        if (e == null) return;
        if (e.EventObject == null) return;

        //Events aren't firing coroutines directly, so using boolean value to trigger firing in update
        messageReceived = true;
        messageContent = wSMessageData.args.eventType;        
    }

    private void OnWsMessageCheck()
    {
        //Events aren't firing coroutines directly, so putting this here in update
        if (messageReceived)
        {
            messageReceived = false;
            switch (messageContent)
            {
                case "update-global":
                    UpdateGlobal();
                    break;
                case "update-meta":
                    //UpdateMeta();
                    break;
                case "update-name":
                    //UpdateName();
                    break;
                case "update-object":
                    break;
                case "update-children":
                    //UpdateChildren();
                    break;
                default:
                    Debug.Log("Client WsMessage");
                    Debug.Log(messageContent);
                    break;
            }
        }
    }

   /// <summary>
   /// Update incoming objects
   /// </summary>
    public void UpdateGlobal()
    {
        StartCoroutine(UpdateGlobalAsync());       
    }
    private IEnumerator UpdateGlobalAsync()
    {
        //TODO - use LocalContext for caching, etc
      
        var streamGet = Client.StreamGetAsync(StreamId, null);       
        while (!streamGet.IsCompleted) yield return null;
                
        if (streamGet.Result == null)
        {
            Debug.Log("error");
        }
        else 
        {
            Client.Stream = streamGet.Result.Resource;

            Debug.Log("Getting objects....");
            var payload = Client.Stream.Objects.Where(o => o.Type == "Placeholder").Select(obj => obj._id).ToArray();

            // how many objects to request from the api at a time
            int maxObjRequestCount = 20;

            // list to hold them into
            var newObjects = new List<SpeckleObject>();

            // jump in `maxObjRequestCount` increments through the payload array
            for (int i = 0; i < payload.Length; i += maxObjRequestCount)
            {
                // create a subset
                var subPayload = payload.Skip(i).Take(maxObjRequestCount).ToArray();

                // get it sync as this is always execed out of the main thread
                //var getTask = Client.ObjectGetBulkAsync(subPayload, "omit=displayValue");
                var getTask = Client.ObjectGetBulkAsync(subPayload,"");
                while (!getTask.IsCompleted) yield return null;

                var res = getTask.Result;

                // put them in our bucket
                newObjects.AddRange(res.Resources);                                            
            }

            // populate the retrieved objects in the original stream's object list
            foreach (var obj in newObjects)
            {
                var locationInStream = Client.Stream.Objects.FindIndex(o => o._id == obj._id);
                try { Client.Stream.Objects[locationInStream] = obj; } catch { }
            }

            Debug.Log("Found " + newObjects.Count + " objects");
            DisplayContents();
            OnUpdateReceived.Invoke(this);
        }      
    }


    /// <summary>
    /// Create native gameobjects and deal with Unity specific things
    /// </summary>
    private void DisplayContents()
    {
        //TODO - update existing objects instead of destroying/recreating all of them

        //Clear existing objects
        foreach (var co in ConvertedObjects)
        { 
            var obj = co as SpeckleUnityGeometry;
            if(obj != null)
            {
                //TODO - write destroy method in class?
                GameObject tempObj = obj.go;
                Destroy(tempObj);                
            }            
        }
        ConvertedObjects.Clear();


        //Convert speckle objects to native
        var localCopy = Client.Stream.Objects.ToList();
        foreach (SpeckleObject myObject in localCopy)
        {
            var gb = Converter.Deserialise(myObject);
            ConvertedObjects.Add(gb);

            SpeckleUnityGeometry geo = gb as SpeckleUnityGeometry;
            if (geo != null)
            {
                geo.go.transform.parent = this.transform;
            }
        }
    }
}
