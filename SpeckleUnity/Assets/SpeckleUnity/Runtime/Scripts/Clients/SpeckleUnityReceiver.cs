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
		/// An optional <c>Transform</c> that can be optionally set in the inspector for the received stream
		/// to be spawned under. If left null, a new one will be created and named after the stream ID.
		/// </summary>
		public Transform streamRoot;

		/// <summary>
		/// A list containing all the Speckle objects from the stream AFTER they had been converted into native
		/// Unity objects.
		/// </summary>
		protected List<object> deserializedStreamObjects = new List<object> ();

		/// <summary>
		/// Key value pairs of SpeckleCore <c>Layer</c>s and Unity <c>Transform</c>s to help with reconstructing
		/// the Stream layer heirarchy within the scene heirarchy.
		/// </summary>
		protected Dictionary<Layer, Transform> layerLookup = new Dictionary<Layer, Transform> ();


		/// <summary>
		/// Key value pairs of Unity <c>GameObject</c>s and SpeckleCore <c>SpeckleObject</c>s to help with 
		/// looking up the corresponding object data to the objects rendered in the scene.
		/// </summary>
		internal Dictionary<GameObject, SpeckleObject> speckleObjectLookup = new Dictionary<GameObject, SpeckleObject> ();

		/// <summary>
		/// 
		/// </summary>
		protected MaterialPropertyBlock propertyBlock = null;

		/// <summary>
		/// Creates an uninitialized instance of a <c>SpeckleUnityReceiver</c>.
		/// </summary>
		/// <param name="streamID">The stream ID to be received.</param>
		/// <param name="streamRoot">An optional root object for the stream to be spawnted under.</param>
		public SpeckleUnityReceiver (string streamID, Transform streamRoot = null)
		{
			this.streamID = streamID;
			this.streamRoot = streamRoot;
		}

		/// <summary>
		/// All clients need to be initialized which creates an instance of an internal speckle client object,
		/// authenticates against the server and provides a manager object to receive inspector arguments from.
		/// </summary>
		/// <param name="manager">The manager instance that provides inspector values for this client.</param>
		/// <param name="url">The url of the speckle server to connect to.</param>
		/// <param name="apiToken">The authentication token of the user to connect as.</param>
		/// <returns>An async <c>Task</c> of the new operation.</returns>
		public override async Task InitializeClient (SpeckleUnityManager manager, string url, string apiToken)
		{
			if (streamRoot == null)
			{
				streamRoot = new GameObject ().transform;
				streamRoot.name = "Default Stream Root: " + streamID;
			}

			this.manager = manager;

			client = new SpeckleApiClient (url.Trim (), true);
			client.BaseUrl = url.Trim ();
			RegisterClient ();

			await client.IntializeReceiver (streamID, "SpeckleUnity", "Unity", Guid.NewGuid ().ToString (), apiToken);

			Debug.Log ("Initialized stream: " + streamID);


			deserializedStreamObjects = new List<object> ();
			layerLookup = new Dictionary<Layer, Transform> ();
			speckleObjectLookup = new Dictionary<GameObject, SpeckleObject> ();

			//after connected, call update global to get geometry
			await UpdateGlobal ();
		}

		/// <summary>
		/// Invoked whenever this client is notified of updates to the stream from the server.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		/// <remarks>NOTE: At the time of writing, coroutines can't be invoked via event 
		/// callbacks for some reason. Until this is resolved, the <c>OnWsMessageCheck ()</c>
		/// method can't be called directly and instead, the manager instance will 
		/// check on each frame whether the method needs to be called in response
		/// to this method being invoked.</remarks>
		protected override void ClientOnWsMessage (object source, SpeckleEventArgs e)
		{
			if (e == null) return;
			//if (e.EventObject == null) return;

			WSMessageData wSMessageData = JsonUtility.FromJson<WSMessageData> (e.EventData);

			//Events aren't firing coroutines directly, so using boolean value to trigger firing in update
			messageReceived = true;
			messageContent = wSMessageData.args.eventType;

			//OnWsMessageCheck ();
		}

		/// <summary>
		/// Checks the content of the web socket message received from the server and invokes the 
		/// appropriate coroutine for updating the scene locally to represent the latest state of the
		/// stream being received.
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
						_ = UpdateGlobal ();
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
		/// Coroutine for the global update message for the stream. Simply put, it redownloads the stream data,
		/// cleans up everything locally and respawns the whole stream.
		/// </summary>
		/// <returns>An async <c>Task</c> of the new operation.</returns>
		protected virtual async Task UpdateGlobal ()
		{
			Debug.Log ("Getting Stream");

			// notify all user code that subsribed to this event in the manager inspector so that their code
			// can respond to the global update of this stream.
			manager.onUpdateProgress.Invoke (new SpeckleUnityUpdate (streamID, streamRoot, UpdateType.Global, 0));

			ResponseStream streamGet = await client.StreamGetAsync (streamID, null);

			Debug.Log ("Got Stream");

			if (streamGet == null)
			{
				Debug.LogError ("Stream '" + streamID + "' Result was null");
			}
			else
			{

				client.Stream = streamGet.Resource;

				string[] payload = client.Stream.Objects.Where (o => o.Type == "Placeholder").Select (obj => obj._id).ToArray ();

				// how many objects to request from the api at a time
				int maxObjRequestCount = 20;

				// list to hold them into
				List<SpeckleObject> newObjects = new List<SpeckleObject> ();

				// jump in `maxObjRequestCount` increments through the payload array
				for (int i = 0; i < payload.Length; i += maxObjRequestCount)
				{
					manager.onUpdateProgress.Invoke (new SpeckleUnityUpdate (streamID, streamRoot, UpdateType.Global, (float)i / (payload.Length * 2)));

					// create a subset
					string[] subPayload = payload.Skip (i).Take (maxObjRequestCount).ToArray ();

					// get it sync as this is always execed out of the main thread
					//Task<ResponseObject> getTask = Client.ObjectGetBulkAsync(subPayload, "omit=displayValue");
					ResponseObject response = await client.ObjectGetBulkAsync (subPayload, "");

					// put them in our bucket
					newObjects.AddRange (response.Resources);
					await Task.Yield ();
				}

				// populate the retrieved objects in the original stream's object list
				foreach (SpeckleObject objects in newObjects)
				{
					int indexInStream = client.Stream.Objects.FindIndex (o => o._id == objects._id);
					try { client.Stream.Objects[indexInStream] = objects; } catch { }
				}

				RemoveContents ();
				await CreateContents ();
				// notify all user code that subsribed to this event in the manager inspector so that their code
				// can respond to the global update of this stream.
				manager.onUpdateProgress.Invoke (new SpeckleUnityUpdate (streamID, streamRoot, UpdateType.Global, 1));
				Debug.Log (streamID + " Download Complete");
			}
		}

		/// <summary>
		/// First constructs the layers for the stream then deserializes the json of all the stream's
		/// objects into Unity gameobjects. The speed of this process is determined by
		/// <c>SpeckleUnityManager.spawnSpeed</c>.
		/// </summary>
		/// <returns>An async <c>Task</c> of the new operation.</returns>
		protected virtual async Task CreateContents ()
		{
			ConstructLayers ();

			propertyBlock = new MaterialPropertyBlock ();

			for (int i = 0; i < client.Stream.Objects.Count; i++)
			{
				object deserializedStreamObject = Converter.Deserialise (client.Stream.Objects[i]);
				deserializedStreamObjects.Add (deserializedStreamObject);

				PostProcessObject (deserializedStreamObject, i);

				if (i % (int)manager.spawnSpeed == 0 && i != 0)
				{
					manager.onUpdateProgress.Invoke (new SpeckleUnityUpdate (streamID, streamRoot, UpdateType.Global, (float)(i + client.Stream.Objects.Count) / (client.Stream.Objects.Count * 2)));
					await Task.Yield ();
				}
			}
		}

		/// <summary>
		/// According to the available layers found in the stream, create a heirarchy of transforms with the same names as those
		/// layers and map them against their original layer objects in a dictionary.
		/// </summary>
		protected virtual void ConstructLayers ()
		{
			List<Layer> layers = client.Stream.Layers;

			for (int i = 0; i < layers.Count; i++)
			{
				Transform newUnityLayer = new GameObject ().transform;

				// Nested layers in rhino are delimited with double colons and revit doesn't have layers so this code may not work
				// for streams originating from other clients where the layering is delimited differently.
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
		/// An algorithm which looks up the layer heirarchy already created in the scene to find the 
		/// parent <c>Transform</c> of the next layer to create.
		/// </summary>
		/// <param name="parents">An array of strings which contains the names of the parent layers leading up to
		/// and including the direct parent of the next layer to create.</param>
		/// <returns>The <c>Transform</c> of the direct parent of the next layer to create a new <c>Transform</c>
		/// for.</returns>
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
		/// Called on each stream object after it's been deserialized. Checks against what <c>Type</c> the
		/// object ended up as and assigns some additional stuff to it including setting geometry objects under
		/// the correct layer.</summary>
		/// <param name="deserializedStreamObject">The stream object after it had been converted from json into
		/// a native object.</param>
		/// <param name="objectIndex">The indext of this stream object to help with placing it in the layer
		/// hierarchy.</param>
		public virtual void PostProcessObject (object deserializedStreamObject, int objectIndex)
		{
			if (deserializedStreamObject is SpeckleUnityGeometry geometry)
			{
				// add object to lookup 
				speckleObjectLookup.Add (geometry.gameObject, client.Stream.Objects[objectIndex]);

				// assign object to layer
				List<Layer> layers = client.Stream.Layers;

				for (int i = 0; i < layers.Count; i++)
				{
					if (objectIndex >= layers[i].StartIndex && objectIndex < (layers[i].StartIndex + layers[i].ObjectCount))
					{
						geometry.gameObject.transform.parent = layerLookup[layers[i]];
						break;
					}
				}

				geometry.renderer.material = manager.meshMaterial;

				manager.renderingRule?.ApplyRuleToObject (geometry.renderer, client.Stream.Objects[objectIndex], propertyBlock);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual void ReApplyRenderingRule ()
		{
			for (int i = 0; i < deserializedStreamObjects.Count; i++)
			{
				if (deserializedStreamObjects[i] is SpeckleUnityGeometry geometry)
				{
					manager.renderingRule?.ApplyRuleToObject (geometry.renderer, client.Stream.Objects[i], propertyBlock);
				}
			}
		}

		/// <summary>
		/// Clean up all gameobjects for the geometry spawned in from the stream including layer objects.
		/// </summary>
		public virtual void RemoveContents ()
		{
			if (streamRoot != null)
			{
				//Clear existing objects including layer objects
				for (int i = 0; i < streamRoot.childCount; i++)
				{
					GameObject.Destroy (streamRoot.GetChild (i));
				}
			}

			deserializedStreamObjects?.Clear ();
		}
	}
}