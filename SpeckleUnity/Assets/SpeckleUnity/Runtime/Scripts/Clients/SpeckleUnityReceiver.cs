using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using SpeckleCore;
using System.Threading.Tasks;

namespace SpeckleUnity
{
	/// <summary>
	/// A <c>SpeckleUnityClient</c> specialised in receiving streams and updating the scene to reflect
	/// any updates made to the stream. Made serializable so that it would render in the inspector along
	/// with its exposed fields.
	/// </summary>
	[Serializable]
	public class SpeckleUnityReceiver : SpeckleUnityClient
	{
		/// <summary>
		/// Boolean used to help with the coroutine workaround. Is set to true when the web socket event
		/// is fired for the manager to respond against and then immediately set back to false.
		/// </summary>
		protected bool messageReceived = false;

		/// <summary>
		/// String used to help with the coroutine workaround. Is set to the conent of the web socket event
		/// when it is fired for the manager to respond against.
		/// </summary>
		protected string messageContent;

		/// <summary>
		/// A <c>Transform</c> that can be optionally set in the inspector
		/// </summary>
		public Transform streamRoot;

		/// <summary>
		/// 
		/// </summary>
		protected List<object> convertedObjects = new List<object> ();

		/// <summary>
		/// 
		/// </summary>
		protected Dictionary<Layer, Transform> layerLookup = new Dictionary<Layer, Transform> ();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="streamID"></param>
		/// <param name="streamRoot"></param>
		public SpeckleUnityReceiver (string streamID, Transform streamRoot = null)
		{
			this.streamID = streamID;
			this.streamRoot = streamRoot;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="manager"></param>
		/// <param name="url"></param>
		/// <param name="authToken"></param>
		/// <returns></returns>
		public override IEnumerator InitializeClient (SpeckleUnityManager manager, string url, string authToken)
		{
			if (streamRoot == null)
			{
				streamRoot = new GameObject ().transform;
				streamRoot.name = "Default Stream Root: " + streamID;
			}

			this.manager = manager;

			client = new SpeckleApiClient (url, true);
			client.BaseUrl = url;

			RegisterClient ();

			//Initialize receiver
			client.IntializeReceiver (streamID, "SpeckleUnity", "Unity", Guid.NewGuid ().ToString (), authToken);

			Debug.Log ("Initializing stream: " + streamID);

			//wait for receiver to be connected
			while (!client.IsConnected) yield return null;

			convertedObjects = new List<object> ();
			layerLookup = new Dictionary<Layer, Transform> ();

			//after connected, call update global to get geometry
			yield return manager.StartCoroutine (UpdateGlobal ());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected override void ClientOnWsMessage (object source, SpeckleEventArgs e)
		{
			if (e == null) return;
			if (e.EventObject == null) return;

			WSMessageData wSMessageData = JsonUtility.FromJson<WSMessageData> (e.EventData);

			//Events aren't firing coroutines directly, so using boolean value to trigger firing in update
			messageReceived = true;
			messageContent = wSMessageData.args.eventType;

			//OnWsMessageCheck ();
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
				Debug.Log (streamID + ": " + messageContent);

				switch (messageContent)
				{
					case "update-global":
						manager.StartCoroutine (UpdateGlobal ());
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
			Task<ResponseStream> streamGet = client.StreamGetAsync (streamID, null);
			while (!streamGet.IsCompleted) yield return null;

			if (streamGet.Result == null)
			{
				Debug.LogError ("Stream '" + streamID + "' Result was null");
			}
			else
			{
				client.Stream = streamGet.Result.Resource;

				string[] payload = client.Stream.Objects.Where (o => o.Type == "Placeholder").Select (obj => obj._id).ToArray ();

				// how many objects to request from the api at a time
				int maxObjRequestCount = 20;

				// list to hold them into
				List<SpeckleObject> newObjects = new List<SpeckleObject> ();

				// jump in `maxObjRequestCount` increments through the payload array
				for (int i = 0; i < payload.Length; i += maxObjRequestCount)
				{
					// create a subset
					string[] subPayload = payload.Skip (i).Take (maxObjRequestCount).ToArray ();

					// get it sync as this is always execed out of the main thread
					//var getTask = Client.ObjectGetBulkAsync(subPayload, "omit=displayValue");
					Task<ResponseObject> getTask = client.ObjectGetBulkAsync (subPayload, "");
					while (!getTask.IsCompleted) yield return null;

					ResponseObject response = getTask.Result;

					// put them in our bucket
					newObjects.AddRange (response.Resources);
				}

				// populate the retrieved objects in the original stream's object list
				foreach (SpeckleObject objects in newObjects)
				{
					int indexInStream = client.Stream.Objects.FindIndex (o => o._id == objects._id);
					try { client.Stream.Objects[indexInStream] = objects; } catch { }
				}

				yield return manager.StartCoroutine (DisplayContents ());

				manager.onUpdateReceived.Invoke (new SpeckleUnityUpdate (streamID, streamRoot, UpdateType.Global));
			}
		}


		/// <summary>
		/// Create native gameobjects and deal with Unity specific things
		/// </summary>
		protected virtual IEnumerator DisplayContents ()
		{
			//TODO - update existing objects instead of destroying/recreating all of them

			RemoveContents ();

			yield return manager.StartCoroutine (CreateContents ());
		}

		/// <summary>
		/// 
		/// </summary>
		protected virtual IEnumerator CreateContents ()
		{
			ConstructLayers ();

			for (int i = 0; i < client.Stream.Objects.Count; i++)
			{
				object convertedObject = Converter.Deserialise (client.Stream.Objects[i]);
				convertedObjects.Add (convertedObject);

				PostProcessObject (convertedObject, i);

				if (i % (int)manager.spawnSpeed == 0) yield return null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected virtual void ConstructLayers ()
		{
			List<Layer> layers = client.Stream.Layers;

			for (int i = 0; i < layers.Count; i++)
			{
				Transform newUnityLayer = new GameObject ().transform;

				if (!layers[i].Name.Contains ("::"))
				{
					newUnityLayer.name = layers[i].Name;
					newUnityLayer.parent = streamRoot;
				}
				else
				{
					string[] layerNames = layers[i].Name.Split (new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

					newUnityLayer.name = layerNames[layerNames.Length - 1];

					newUnityLayer.parent = FindParentInHierarchy (layerNames.Take (layerNames.Length - 1).ToArray ());
				}

				layerLookup.Add (layers[i], newUnityLayer);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parents"></param>
		/// <returns></returns>
		protected Transform FindParentInHierarchy (string[] parents)
		{
			int layerDepth = 0;
			Transform layerToSearchIn = streamRoot;
			while (layerDepth < parents.Length)
			{
				for (int i = 0; i < layerToSearchIn.childCount; i++)
				{
					if (layerToSearchIn.GetChild (i).name == parents[layerDepth])
					{
						layerToSearchIn = layerToSearchIn.GetChild (i);
						layerDepth++;
						break;
					}
				}
			}

			return layerToSearchIn;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="convertedObject"></param>
		public virtual void PostProcessObject (object convertedObject, int objectIndex)
		{
			if (convertedObject is SpeckleUnityGeometry geometry)
			{
				List<Layer> layers = client.Stream.Layers;

				for (int i = 0; i < layers.Count; i++)
				{
					if (objectIndex >= layers[i].StartIndex && objectIndex < (layers[i].StartIndex + layers[i].ObjectCount))
					{
						geometry.gameObject.transform.parent = layerLookup[layers[i]];
						break;
					}
				}
				
			}

			if (convertedObject is SpeckleUnityMesh mesh)
			{
				mesh.meshRenderer.material = manager.meshMaterial;
			}

			if (convertedObject is SpeckleUnityPolyline line)
			{
				line.lineRenderer.material = manager.polylineMaterial;
			}

			if (convertedObject is SpeckleUnityPoint point)
			{
				point.lineRenderer.material = manager.pointMaterial;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual void RemoveContents ()
		{
			//Clear existing objects
			foreach (object convertedObject in convertedObjects)
			{
				if (convertedObject is SpeckleUnityGeometry geometry)
				{
					//TODO - write destroy method in class?
					GameObject.Destroy (geometry.gameObject);
				}
			}
			convertedObjects.Clear ();
		}
	}
}