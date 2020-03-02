using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using SpeckleCore;

namespace SpeckleUnity
{
	[Serializable]
	public class SpeckleReceiver : SpeckleClient
	{
		/// <summary>
		/// 
		/// </summary>
		protected bool messageReceived = false;

		/// <summary>
		/// 
		/// </summary>
		protected string messageContent;

		/// <summary>
		/// 
		/// </summary>
		public Transform streamRoot;

		/// <summary>
		/// 
		/// </summary>
		protected List<object> convertedObjects = new List<object> ();

		public override IEnumerator InitializeClient (SpeckleController controller, string url, string authToken)
		{
			if (streamRoot == null)
			{
				streamRoot = new GameObject ().transform;
				streamRoot.name = "Root: " + streamID;
			}

			this.controller = controller;

			client = new SpeckleApiClient (url, true);
			client.BaseUrl = url;

			RegisterClient ();

			//Initialize receiver
			client.IntializeReceiver (streamID, "SpeckleUnity", "Unity", Guid.NewGuid ().ToString (), authToken);

			//wait for receiver to be connected
			while (!client.IsConnected) yield return null;

			//speckleObjects = new List<SpeckleObject> ();

			//after connected, call update global to get geometry
			yield return controller.StartCoroutine (UpdateGlobal ());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected override void ClientOnWsMessage (object source, SpeckleEventArgs e)
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
		public virtual void OnWsMessageCheck ()
		{
			//Events aren't firing coroutines directly, so putting this here in update
			if (messageReceived)
			{
				messageReceived = false;
				switch (messageContent)
				{
					case "update-global":
						controller.StartCoroutine (UpdateGlobal ());
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
		/// 
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator UpdateGlobal ()
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
				controller.onUpdateReceived.Invoke ();
			}
		}


		/// <summary>
		/// Create native gameobjects and deal with Unity specific things
		/// </summary>
		protected virtual void DisplayContents ()
		{
			//TODO - update existing objects instead of destroying/recreating all of them

			//Clear existing objects
			foreach (object convertedObject in convertedObjects)
			{
				if (convertedObject is SpeckleUnityGeometry geometry)
				{
					//TODO - write destroy method in class?
					GameObject tempObj = geometry.gameObject;
					GameObject.Destroy (tempObj);
				}
			}
			convertedObjects.Clear ();


			foreach (SpeckleObject streamObject in client.Stream.Objects)
			{
				object convertedObject = Converter.Deserialise (streamObject);
				convertedObjects.Add (convertedObject);

				if (convertedObject is SpeckleUnityGeometry geometry)
				{
					geometry.gameObject.transform.parent = streamRoot;
				}

				if (convertedObject is SpeckleUnityMesh mesh)
				{
					mesh.meshRenderer.material = controller.meshMaterial;
				}
			}
		}
	}
}