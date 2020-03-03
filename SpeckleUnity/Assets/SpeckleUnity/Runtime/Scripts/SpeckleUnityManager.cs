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
		/// <summary>
		/// 
		/// </summary>
		public bool initializeOnStart = true;

		/// <summary>
		/// 
		/// </summary>
		[Tooltip ("URL for the Speckle Server you want to use. eg: https://hestia.speckle.works/api/v1")]
		public string serverUrl = "";

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
		public SpeckleUnityUpdateEvent onUpdateReceived;

		/// <summary>
		/// 
		/// </summary>
		public SpeckleUnityReceiver[] receivers = Array.Empty<SpeckleUnityReceiver> ();

		/// <summary>
		/// 
		/// </summary>
		protected virtual void Start ()
		{
			if (initializeOnStart) Initialize ();
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual void Initialize ()
		{
			SpeckleInitializer.Initialize ();
			LocalContext.Init ();

			for (int i = 0; i < receivers.Length; i++)
			{
				StartCoroutine (receivers[i].InitializeClient (this, serverUrl, authToken));
			}
		}

		// Update is called once per frame
		/*protected virtual void Update ()
		{
			for (int i = 0; i < receivers.Length; i++)
			{
				receivers[i].OnWsMessageCheck ();
			}
		}*/
	}
}