using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpeckleCore;

namespace SpeckleUnity
{
	[Serializable]
	public abstract class SpeckleClient
	{
		protected SpeckleController controller;

		/// <summary>
		/// 
		/// </summary>
		[HideInInspector] public SpeckleApiClient client;

		/// <summary>
		/// 
		/// </summary>
		public string streamID;

		/// <summary>
		/// 
		/// </summary>
		public virtual void RegisterClient ()
		{
			client.OnReady += ClientOnReady;
			client.OnLogData += ClientOnLogData;
			client.OnWsMessage += ClientOnWsMessage;
			client.OnError += ClientOnError;
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual void UnregisterClient ()
		{
			client.OnReady -= ClientOnReady;
			client.OnLogData -= ClientOnLogData;
			client.OnWsMessage -= ClientOnWsMessage;
			client.OnError -= ClientOnError;

			client.Dispose (true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		public abstract IEnumerator InitializeClient (SpeckleController controller, string url, string authToken);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected virtual void ClientOnReady (object source, SpeckleEventArgs e)
		{
			Debug.Log ("Client ready");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected virtual void ClientOnLogData (object source, SpeckleEventArgs e)
		{
			Debug.Log ("Client LogData");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected virtual void ClientOnWsMessage (object source, SpeckleEventArgs e)
		{
			Debug.Log ("Client WsMessage");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected virtual void ClientOnError (object source, SpeckleEventArgs e)
		{
			Debug.Log ("Client Error");
		}
	}
}