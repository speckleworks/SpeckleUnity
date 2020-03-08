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
	/// 
	/// </summary>
	public class SpeckleUnityManager : MonoBehaviour, ISpeckleInitializer
	{
		public enum SpawnSpeed
		{
			Instant = int.MaxValue,
			ThousandPerFrame = 1000,
			HundredPerFrame = 100,
			TenPerFrame = 10
		}

		/// <summary>
		/// 
		/// </summary>
		public bool initializeOnStart = true;

		/// <summary>
		/// 
		/// </summary>
		public SpawnSpeed spawnSpeed = SpawnSpeed.Instant;

		/// <summary>
		/// 
		/// </summary>
		[SerializeField] protected double scaleFactor = 0.001;

		/// <summary>
		/// 
		/// </summary>
		public string serverUrl = "https://hestia.speckle.works/api/v1";

		/// <summary>
		/// 
		/// </summary>
		public string authToken = "";

		/// <summary>
		/// 
		/// </summary>
		public Material meshMaterial;

		/// <summary>
		/// 
		/// </summary>
		public Material polylineMaterial;

		/// <summary>
		/// 
		/// </summary>
		public Material pointMaterial;

		/// <summary>
		/// 
		/// </summary>
		public SpeckleUnityUpdateEvent onUpdateReceived;

		/// <summary>
		/// 
		/// </summary>
		[SerializeField] protected List<SpeckleUnityReceiver> receivers = new List<SpeckleUnityReceiver> ();

		/// <summary>
		/// 
		/// </summary>
		protected virtual void Start ()
		{
			Conversions.scaleFactor = scaleFactor;

			if (initializeOnStart) Initialize ();
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual void Initialize ()
		{
			SpeckleInitializer.Initialize ();
			LocalContext.Init ();

			for (int i = 0; i < receivers.Count; i++)
			{
				StartCoroutine (receivers[i].InitializeClient (this, serverUrl, authToken));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected virtual void Update ()
		{
			for (int i = 0; i < receivers.Count; i++)
			{
				receivers[i].OnWsMessageCheck ();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="streamID"></param>
		/// <param name="streamRoot"></param>
		/// <param name="initialiseOnCreation"></param>
		public virtual void AddReceiver (string streamID, Transform streamRoot = null, bool initialiseOnCreation = false)
		{
			SpeckleUnityReceiver newReceiver = new SpeckleUnityReceiver (streamID, streamRoot);
			receivers.Add (newReceiver);

			if (initialiseOnCreation)
				StartCoroutine (newReceiver.InitializeClient (this, serverUrl, authToken));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="streamID"></param>
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
		/// 
		/// </summary>
		/// <param name="streamRoot"></param>
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
		/// 
		/// </summary>
		/// <param name="receiverIndex"></param>
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