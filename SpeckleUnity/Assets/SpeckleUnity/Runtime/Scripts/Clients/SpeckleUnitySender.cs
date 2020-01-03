using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using SpeckleCore;
using System.Runtime.CompilerServices;

using System;
using System.Threading.Tasks;

namespace SpeckleUnity
{
	//TODO - Improve saving system

	[Serializable]
	public class SpeckleUnitySender : SpeckleUnityClient
	{
		/// <summary>
		/// 
		/// </summary>
		[Header ("Persistence")]
		[Tooltip ("Set to true to save this client. Only used for senders.")]
		public bool persistent = false;

		/// <summary>
		/// Used to find the appropriate client next time the application is loaded
		/// Needs to be defined in editor, so can't use client speckle id which won't exist
		/// Only used if persistent is set to true
		/// </summary>
		[Tooltip ("Enter a unique value for saving this sending client")]
		public string keyForSaving;

		/// <summary>
		/// 
		/// </summary>
		protected const string streamNamePrefix = "UnityStream_";

		/// <summary>
		/// 
		/// </summary>
		protected List<SpeckleUnityObject> nativeObjectsToSend = new List<SpeckleUnityObject> ();


		/// <summary>
		/// Initialize Sender
		/// </summary>
		/// <param name="URL"></param>
		public override void InitializeClient (string URL)
		{
			StartCoroutine (InitializeSenderAsync (URL));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="URL"></param>
		/// <returns></returns>
		protected virtual IEnumerator InitializeSenderAsync (string URL)
		{
			client = new SpeckleApiClient (URL, true);
			AssignEvents ();

			//TODO: store guid?
			unityGUID = Guid.NewGuid ().ToString ();
			client.IntializeSender (authToken, "UnityTestSender", "Unity", unityGUID);

			//wait for receiver to be connected
			while (!client.IsConnected) yield return null;

			streamID = client.Stream.StreamId;
			client.Stream.Name = streamNamePrefix + gameObject.name;


			SendStaggeredUpdate ();


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
		/// Send updated geometry to stream
		/// </summary>
		public virtual void SendStaggeredUpdate ()
		{
			//TODO - use a timer to limit the send frequency?
			if (client != null)
				if (client.Stream != null)
					StartCoroutine (SendStaggeredUpdateAsync ());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator SendStaggeredUpdateAsync ()
		{

			// create a clone
			//Not sure why we are doing this, copied it from the Rhino sender

			var cloneResult = client.StreamCloneAsync (streamID);
			while (!cloneResult.IsCompleted) yield return null;
			client.Stream.Children.Add (cloneResult.Result.Clone.StreamId);
			client.BroadcastMessage ("stream", streamID, new { eventType = "update-children" });

			//CREATE SPECKLE OBJECTS FROM NATIVE OBJECTS
			var convertedObjects = new List<SpeckleObject> ();
			foreach (SpeckleUnityObject obj in nativeObjectsToSend)
			{
				var myObj = Converter.Serialise (obj) as SpeckleObject;
				//myObj.ApplicationId = obj.Id.ToString();
				convertedObjects.Add (myObj);
			}
			var payload = convertedObjects;

			//TODO - implement multiple layers
			Layer newLayer = new SpeckleCore.Layer ()
			{
				Name = "UnityLayer",
				Guid = "TestGUID",
				ObjectCount = convertedObjects.Count,
				StartIndex = 0,
				OrderIndex = 0,
				Properties = new LayerProperties () { Color = new SpeckleCore.SpeckleBaseColor () { A = 1, Hex = "Black" }, },
				Topology = "0-" + convertedObjects.Count.ToString () + " "
			};
			List<Layer> theLayers = new List<Layer> ();
			theLayers.Add (newLayer);
			client.Stream.Layers = theLayers.ToList ();


			//UPDATE/CREATE OBJECTS IN CLIENT
			LocalContext.PruneExistingObjects (convertedObjects, client.BaseUrl);

			List<SpeckleObject> persistedObjects = new List<SpeckleObject> ();

			var createTask = client.ObjectCreateAsync (payload);
			while (!createTask.IsCompleted) yield return null;
			persistedObjects.AddRange (createTask.Result.Resources);

			int m = 0;
			foreach (var oL in payload)
			{
				oL._id = createTask.Result.Resources[m++]._id;
			}

			// push sent objects in the cache non-blocking
			Task.Run (() =>
			{
				foreach (var oL in payload)
				{
					if (oL.Type != "Placeholder")
						LocalContext.AddSentObject (oL, client.BaseUrl);
				}
			});


			// create placeholders for stream update payload
			List<SpeckleObject> placeholders = new List<SpeckleObject> ();

			foreach (var obj in persistedObjects)
				placeholders.Add (new SpecklePlaceholder () { _id = obj._id });

			// create stream update payload
			SpeckleStream streamUpdatePayload = new SpeckleStream ();
			streamUpdatePayload.Layers = client.Stream.Layers;
			streamUpdatePayload.Objects = placeholders;
			streamUpdatePayload.Name = client.Stream.Name;

			var responseStreamUpdate = client.StreamUpdateAsync (client.Stream.StreamId, streamUpdatePayload);
			while (!responseStreamUpdate.IsCompleted) yield return null;

			client.Stream.Objects = placeholders;

			client.BroadcastMessage ("stream", streamID, new { eventType = "update-global" });

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		public virtual void OnObjectUpdated (object source)
		{
			SendStaggeredUpdate ();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		public virtual void RegisterObject (SpeckleUnityObject obj)
		{
			//TODO - test if the object is already added
			nativeObjectsToSend.Add (obj);
			obj.ValueChanged += OnObjectUpdated;
			SendStaggeredUpdate ();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		public virtual void UnregisterObject (SpeckleUnityObject obj)
		{
			nativeObjectsToSend.Remove (obj);
			obj.ValueChanged -= OnObjectUpdated;
			SendStaggeredUpdate ();
		}
	}
}

