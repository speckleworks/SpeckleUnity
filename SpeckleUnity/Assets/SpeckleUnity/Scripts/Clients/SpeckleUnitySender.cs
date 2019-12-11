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
		protected const string StreamNamePrefix = "UnityStream_";

		protected List<SpeckleUnityObject> nativeObjectsToSend = new List<SpeckleUnityObject> ();


		/// <summary>
		/// Initialize Sender
		/// </summary>
		/// <param name="URL"></param>
		public override void InitializeClient (string URL)
		{
			StartCoroutine (InitializeSenderAsync (URL));
		}

		//Initialize sender
		protected virtual IEnumerator InitializeSenderAsync (string URL)
		{
			Client = new SpeckleApiClient (URL, true);
			AssignEvents ();

			//TODO: store guid?
			unity_guid = Guid.NewGuid ().ToString ();
			Client.IntializeSender (authToken, "UnityTestSender", "Unity", unity_guid);

			//wait for receiver to be connected
			while (!Client.IsConnected) yield return null;

			StreamId = Client.Stream.StreamId;
			Client.Stream.Name = StreamNamePrefix + gameObject.name;


			SendStaggeredUpdate ();


		}

		public override void CompleteDeserialization (SpeckleApiClient client)
		{
			Client = client;
			StreamId = Client.StreamId;

		}

		/// <summary>
		/// Send updated geometry to stream
		/// </summary>
		public virtual void SendStaggeredUpdate ()
		{
			//TODO - use a timer to limit the send frequency?
			if (Client != null)
				if (Client.Stream != null)
					StartCoroutine (SendStaggeredUpdateAsync ());
		}

		protected virtual IEnumerator SendStaggeredUpdateAsync ()
		{

			// create a clone
			//Not sure why we are doing this, copied it from the Rhino sender

			var cloneResult = Client.StreamCloneAsync (StreamId);
			while (!cloneResult.IsCompleted) yield return null;
			Client.Stream.Children.Add (cloneResult.Result.Clone.StreamId);
			Client.BroadcastMessage ("stream", StreamId, new { eventType = "update-children" });

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
			Client.Stream.Layers = theLayers.ToList ();


			//UPDATE/CREATE OBJECTS IN CLIENT
			LocalContext.PruneExistingObjects (convertedObjects, Client.BaseUrl);

			List<SpeckleObject> persistedObjects = new List<SpeckleObject> ();

			var createTask = Client.ObjectCreateAsync (payload);
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
						LocalContext.AddSentObject (oL, Client.BaseUrl);
				}
			});


			// create placeholders for stream update payload
			List<SpeckleObject> placeholders = new List<SpeckleObject> ();

			foreach (var obj in persistedObjects)
				placeholders.Add (new SpecklePlaceholder () { _id = obj._id });

			// create stream update payload
			SpeckleStream streamUpdatePayload = new SpeckleStream ();
			streamUpdatePayload.Layers = Client.Stream.Layers;
			streamUpdatePayload.Objects = placeholders;
			streamUpdatePayload.Name = Client.Stream.Name;

			var responseStreamUpdate = Client.StreamUpdateAsync (Client.Stream.StreamId, streamUpdatePayload);
			while (!responseStreamUpdate.IsCompleted) yield return null;

			Client.Stream.Objects = placeholders;

			Client.BroadcastMessage ("stream", StreamId, new { eventType = "update-global" });

		}

		public virtual void OnObjectUpdated (object source)
		{
			SendStaggeredUpdate ();
		}

		public virtual void RegisterObject (SpeckleUnityObject obj)
		{
			//TODO - test if the object is already added
			nativeObjectsToSend.Add (obj);
			obj.ValueChanged += OnObjectUpdated;
			SendStaggeredUpdate ();
		}

		public virtual void UnregisterObject (SpeckleUnityObject obj)
		{
			nativeObjectsToSend.Remove (obj);
			obj.ValueChanged -= OnObjectUpdated;
			SendStaggeredUpdate ();
		}



	}
}

