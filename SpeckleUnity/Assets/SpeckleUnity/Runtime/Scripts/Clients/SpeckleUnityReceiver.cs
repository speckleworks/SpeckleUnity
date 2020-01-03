using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using SpeckleCore;

namespace SpeckleUnity
{
	/// <summary>
	/// 
	/// </summary>
	public class SpeckleUnityReceiver : SpeckleUnityClient
	{
		/// <summary>
		/// 
		/// </summary>
		//public List<SpeckleObject> speckleObjects;

		/// <summary>
		/// 
		/// </summary>
		public List<object> convertedObjects = new List<object> ();

		/// <summary>
		/// 
		/// </summary>
		protected Dictionary<string, SpeckleObject> objectCache = new Dictionary<string, SpeckleObject> ();

		/// <summary>
		/// 
		/// </summary>
		protected bool messageReceived = false;

		/// <summary>
		/// 
		/// </summary>
		protected string messageContent;

		/// <summary>
		/// Provides event to access outside unity speckle
		/// </summary>
		public UnityEvent onUpdateReceived;

		/// <summary>
		/// 
		/// </summary>
		protected virtual void Update ()
		{
			//Update global was not working when triggered from the socket events, due to threading conflicts?
			//Current solution is to set a value with the socket event, and then check every frame to see if an action needs to be taken
			OnWsMessageCheck ();
		}

		/// <summary>
		/// Initialize Unity Receiver
		/// </summary>
		/// <param name="URL"></param>
		public override void InitializeClient (string URL)
		{
			StartCoroutine (IntializeReceiverAsync (streamID, URL));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="inStreamID"></param>
		/// <param name="URL"></param>
		/// <returns></returns>
		protected virtual IEnumerator IntializeReceiverAsync (string inStreamID, string URL)
		{
			client = new SpeckleApiClient (URL, true);
			client.BaseUrl = URL;

			AssignEvents ();

			//Initialize receiver
			client.IntializeReceiver (streamID, "UnityTest", "Unity", Guid.NewGuid ().ToString (), authToken);

			//wait for receiver to be connected
			while (!client.IsConnected) yield return null;

			//speckleObjects = new List<SpeckleObject> ();

			//after connected, call update global to get geometry
			UpdateGlobal ();

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="client"></param>
		public override void CompleteDeserialization (SpeckleApiClient client)
		{
			base.client = client;
			streamID = base.client.StreamId;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		public override void ClientOnWsMessage (object source, SpeckleEventArgs e)
		{
			var wSMessageData = JsonUtility.FromJson<WSMessageData> (e.EventData);

			if (e == null) return;
			if (e.EventObject == null) return;

			//Events aren't firing coroutines directly, so using boolean value to trigger firing in update
			messageReceived = true;
			messageContent = wSMessageData.args.eventType;
		}

		/// <summary>
		/// 
		/// </summary>
		protected virtual void OnWsMessageCheck ()
		{
			//Events aren't firing coroutines directly, so putting this here in update
			if (messageReceived)
			{
				messageReceived = false;
				switch (messageContent)
				{
					case "update-global":
						UpdateGlobal ();
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
						Debug.Log ("Client WsMessage");
						Debug.Log (messageContent);
						break;
				}
			}
		}

		/// <summary>
		/// Update incoming objects
		/// </summary>
		public virtual void UpdateGlobal ()
		{
			StartCoroutine (UpdateGlobalAsync ());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator UpdateGlobalAsync ()
		{
			//TODO - use LocalContext for caching, etc

			var streamGet = client.StreamGetAsync (streamID, null);
			while (!streamGet.IsCompleted) yield return null;

			if (streamGet.Result == null)
			{
				Debug.Log ("error");
			}
			else
			{
				client.Stream = streamGet.Result.Resource;

				Debug.Log ("Getting objects....");
				var payload = client.Stream.Objects.Where (o => o.Type == "Placeholder").Select (obj => obj._id).ToArray ();

				// how many objects to request from the api at a time
				int maxObjRequestCount = 20;

				// list to hold them into
				var newObjects = new List<SpeckleObject> ();

				// jump in `maxObjRequestCount` increments through the payload array
				for (int i = 0; i < payload.Length; i += maxObjRequestCount)
				{
					// create a subset
					var subPayload = payload.Skip (i).Take (maxObjRequestCount).ToArray ();

					// get it sync as this is always execed out of the main thread
					//var getTask = Client.ObjectGetBulkAsync(subPayload, "omit=displayValue");
					var getTask = client.ObjectGetBulkAsync (subPayload, "");
					while (!getTask.IsCompleted) yield return null;

					var res = getTask.Result;

					// put them in our bucket
					newObjects.AddRange (res.Resources);
				}

				// populate the retrieved objects in the original stream's object list
				foreach (var obj in newObjects)
				{
					var locationInStream = client.Stream.Objects.FindIndex (o => o._id == obj._id);
					try { client.Stream.Objects[locationInStream] = obj; } catch { }
				}

				Debug.Log ("Found " + newObjects.Count + " objects");
				DisplayContents ();
				onUpdateReceived.Invoke ();
			}
		}


		/// <summary>
		/// Create native gameobjects and deal with Unity specific things
		/// </summary>
		protected virtual void DisplayContents ()
		{
			//TODO - update existing objects instead of destroying/recreating all of them

			//Clear existing objects
			foreach (var co in convertedObjects)
			{
				var obj = co as SpeckleUnityGeometry;
				if (obj != null)
				{
					//TODO - write destroy method in class?
					GameObject tempObj = obj.go;
					Destroy (tempObj);
				}
			}
			convertedObjects.Clear ();


			//Convert speckle objects to native
			var localCopy = client.Stream.Objects.ToList ();
			foreach (SpeckleObject myObject in localCopy)
			{
				var gb = Converter.Deserialise (myObject);
				convertedObjects.Add (gb);

				SpeckleUnityGeometry geo = gb as SpeckleUnityGeometry;
				if (geo != null)
				{
					geo.go.transform.parent = this.transform;
				}
			}
		}
	}
}

