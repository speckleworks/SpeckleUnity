using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SpeckleCore;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SpeckleUnity
{
	/// <summary>
	/// Manages the stream interactions for a single user in the scene and exposes parameters
	/// that control the output of the stream conversion.
	/// </summary>
	public class SpeckleUnityManager : MonoBehaviour, ISpeckleInitializer
	{
		/// <summary>
		/// An enum for controlling how quickly geometry objects get isntantiated into the scene.
		/// The Instant value simply means all objects will be spawned in the same frame. (Technically,
		/// it caps out at 2 billion, but who in the world is making streams bigger than 2 billion...)
		/// </summary>
		public enum SpawnSpeed
		{
			Instant = int.MaxValue,
			ThousandPerFrame = 1000,
			HundredPerFrame = 100,
			TenPerFrame = 10
		}

		/// <summary>
		/// If set to false, you need to reference this class and call the <c>InitializeAllClients ()</c> method
		/// yourself.
		/// </summary>
		public bool initializeOnStart = true;

		/// <summary>
		/// Speed value to allow for instantiation to happen gradually over many frames in case of 
		/// performance issues with large streams that get initialized / updated.
		/// </summary>
		public SpawnSpeed spawnSpeed = SpawnSpeed.Instant;

		/// <summary>
		/// A value for easily setting the static <c>Conversions.scaleFactor</c> value via the inspector.
		/// This class assigs the value once on <c>Start ()</c>.
		/// </summary>
		[SerializeField] protected double scaleFactor = 0.001;

		/// <summary>
		/// The server to send / receive streams from and authenticate against. Changing this value during
		/// runtime requires calling <c>InitializeAllClients ()</c> again.
		/// </summary>
		public string serverUrl = "https://hestia.speckle.works/api/v1/";

		/// <summary>
		/// Authentication token used in interacting with the speckle API for streams that require authenticated access.
		/// Designed to be set in the inspector or assigned to after user credentials are authenticated via the login API which
		/// returns the auth token for that user.
		/// </summary>
		public string authToken = "";

		/// <summary>
		/// Assigns to the <c>MeshRenderer</c>s of every brep or mesh object for every stream handled by this manager.
		/// </summary>
		public Material meshMaterial;

		/// <summary>
		/// Assigns to the <c>LineRenderer</c>s of every line, curve or polyline object for every stream handled by this manager.
		/// </summary>
		public Material polylineMaterial;

		/// <summary>
		/// Assigns to the <c>LineRenderer</c>s of every point object for every stream handled by this manager.
		/// </summary>
		public Material pointMaterial;

		/// <summary>
		/// A <c>UnityEvent</c> that is invoked each time a stream is updated, including when it's initialised, for user code to 
		/// respond to that event. Passes some helpful data to inform that custom response.
		/// </summary>
		public SpeckleUnityUpdateEvent onUpdateReceived;

		/// <summary>
		/// An exposed list of all the <c>SpeckleUnityReceivers</c> this manager controls. Intended to only be directly editable via
		/// the inspector. During runtime you should make use of the <c>RemoveReceiver ()</c> or <c>AddReceiver ()</c> methods.
		/// </summary>
		[SerializeField] protected List<SpeckleUnityReceiver> receivers = new List<SpeckleUnityReceiver> ();

		/// <summary>
		/// Assigns the scale factor of all geometry and, if <c>initializeOnStart</c> is set to true, calls <c>InitializeAllClients ()</c>.
		/// </summary>
		protected virtual void Start ()
		{
			Conversions.scaleFactor = scaleFactor;

			if (initializeOnStart) InitializeAllClients ();
		}

		/// <summary>
		/// Initializes Speckle and the <c>LocalContext</c> for Speckle, then loops through each receiver and starts each of their 
		/// initialization coroutines.
		/// </summary>
		public virtual void InitializeAllClients ()
		{
			SpeckleInitializer.Initialize ();
			LocalContext.Init ();

			for (int i = 0; i < receivers.Count; i++)
			{
				StartCoroutine (receivers[i].InitializeClient (this, serverUrl, authToken));
			}
		}

		/// <summary>
		/// Since there is a weird bug in Unity with web socket responses not being able to invoke coroutines, we check a boolean 
		/// on each receiver on each frame to simulate that effect.
		/// </summary>
		protected virtual void Update ()
		{
			for (int i = 0; i < receivers.Count; i++)
			{
				receivers[i].OnWsMessageCheck ();
			}
		}

		/// <summary>
		/// Creates a new receiver to be managed by this manager instance. 
		/// </summary>
		/// <param name="streamID">The ID of the stream to receive. Authentication will be done using the <c>authToken</c>
		/// on the manager instance this method is being called on.</param>
		/// <param name="streamRoot">Optionally, you can provide a <c>Transform</c> for the geometry to be spawned
		/// under.</param>
		/// <param name="initialiseOnCreation">Optionally, the stream can have its <c>InitializeClient</c>
		/// coroutine started after being created.</param>
		public virtual void AddReceiver (string streamID, Transform streamRoot = null, bool initialiseOnCreation = false)
		{
			SpeckleUnityReceiver newReceiver = new SpeckleUnityReceiver (streamID, streamRoot);
			receivers.Add (newReceiver);

			if (initialiseOnCreation)
				StartCoroutine (newReceiver.InitializeClient (this, serverUrl, authToken));
		}

		/// <summary>
		/// Remove the first receiver with a matching stream ID on this manager instance. Cleans up all GameObjects
		/// associated to that stream as well.
		/// </summary>
		/// <param name="streamID">The ID of the stream to be removed.</param>
		public virtual void RemoveReceiver (string streamID)
		{
			for (int i = 0; i < receivers.Count; i++)
			{
				if (receivers[i].streamID == streamID)
				{
					RemoveReceiver (i);
					return;
				}
			}

			RemoveReceiver (-1);
		}

		/// <summary>
		/// Remove the first receiver with a matching root <c>Transform</c> on this manager instance. Cleans
		/// up all GameObjects associated to that stream as well.
		/// </summary>
		/// <param name="streamRoot">The root object of the stream to be removed.</param>
		public virtual void RemoveReceiver (Transform streamRoot)
		{
			for (int i = 0; i < receivers.Count; i++)
			{
				if (receivers[i].streamRoot == streamRoot)
				{
					RemoveReceiver (i);
					return;
				}
			}

			RemoveReceiver (-1);
		}

		/// <summary>
		/// Remove a receiver of a given index in the list on this manager instance. Cleans up all GameObjects
		/// associated to that stream as well.
		/// </summary>
		/// <param name="receiverIndex">The index to to remove from.</param>
		public virtual void RemoveReceiver (int receiverIndex)
		{
			if (receiverIndex < 0 || receiverIndex >= receivers.Count)
				throw new Exception ("Receiver could not be removed because it does not exist");

			SpeckleUnityReceiver receiver = receivers[receiverIndex];
			receivers.RemoveAt (receiverIndex);

			receiver.RemoveContents ();
			receiver.client.Dispose (true);
		}
	}
}