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
	public class SpeckleController : MonoBehaviour, ISpeckleInitializer
	{
		/// <summary>
		/// 
		/// </summary>
		[Tooltip ("URL for the Speckle Server you want to use. eg: https://hestia.speckle.works/api/v1")]
		public string serverUrl = "";

		/// <summary>
		/// 
		/// </summary>
		public string authToken = "";

		public Material meshMaterial;

		public UnityEvent onUpdateReceived;

		public SpeckleReceiver[] receivers = new SpeckleReceiver[0];

		// Start is called before the first frame update
		protected virtual void Start ()
		{
			Initialize ();
		}

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
		protected virtual void Update ()
		{
			for (int i = 0; i < receivers.Length; i++)
			{
				receivers[i].OnWsMessageCheck ();
			}
		}
	}
}