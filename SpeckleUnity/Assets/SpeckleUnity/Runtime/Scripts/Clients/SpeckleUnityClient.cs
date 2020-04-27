using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpeckleCore;

namespace SpeckleUnity
{
	/// <summary>
	/// Base class for all SpeckleUnity client types. Inherit from this class to define your own client
	/// types.
	/// </summary>
	[Serializable]
	public abstract class SpeckleUnityClient
	{
		/// <summary>
		/// A reference to the manager instance which oversees the functions of this client. Allows for
		/// access to the fields made available in its inspector as well as the <c>StartCoroutine ()
		/// method.</c>
		/// </summary>
		protected SpeckleUnityManager manager;

		/// <summary>
		/// A reference to the internal client object that this object registers with.
		/// </summary>
		[HideInInspector] public SpeckleApiClient client;

		/// <summary>
		/// The ID of the stream for this object to be a client of.
		/// </summary>
		public string streamID;

		/// <summary>
		/// Assigns reponse methods to the <c>OnReady</c>, <c>OnLogData</c>, <c>OnWsMessage</c> and
		/// <c>OnError</c> events of the internal client object. Requires initialization to happen
		/// first.
		/// </summary>
		public virtual void RegisterClient ()
		{
			client.OnReady += ClientOnReady;
			client.OnLogData += ClientOnLogData;
			client.OnWsMessage += ClientOnWsMessage;
			client.OnError += ClientOnError;
		}

		/// <summary>
		/// Removes reponse methods from the <c>OnReady</c>, <c>OnLogData</c>, <c>OnWsMessage</c> and
		/// <c>OnError</c> events of the internal client object. Since the internal client also gets disposed,
		/// initialization will need to happen again. 
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
		/// All clients need to be initialized which creates an instance of an internal speckle client object,
		/// authenticates against the server and provides a manager object to receive inspector arguments from.
		/// </summary>
		/// <param name="manager">The manager instance that provides inspector values for this client.</param>
		/// <param name="url">The url of the speckle server to connect to.</param>
		/// <param name="authToken">The authentication token of the user to connect as.</param>
		/// <returns>An async <c>Task</c> of the new operation.</returns>
		public abstract Task InitializeClient (SpeckleUnityManager manager, string url, string authToken);

		/// <summary>
		/// Base implementation of the "OnReady" event response. It's empty at this level and intended to be 
		/// overriden.
		/// </summary>
		/// <param name="source">Source object passed from the event.</param>
		/// <param name="e">Arguments passed from the event.</param>
		protected virtual void ClientOnReady (object source, SpeckleEventArgs e)
		{
			//Debug.Log ("INTERNAL READY EVENT: " + e.EventData);

		}

		/// <summary>
		/// Base implementation of the "OnLogData" event response. It's empty at this level and intended to be 
		/// overriden.
		/// </summary>
		/// <param name="source">Source object passed from the event.</param>
		/// <param name="e">Arguments passed from the event.</param>
		protected virtual void ClientOnLogData (object source, SpeckleEventArgs e)
		{
			//Debug.Log ("INTERNAL LOG: " + e.EventData);
		}

		/// <summary>
		/// Base implementation of the "OnWsMessage" event response. It's empty at this level and intended to be 
		/// overriden.
		/// </summary>
		/// <param name="source">Source object passed from the event.</param>
		/// <param name="e">Arguments passed from the event.</param>
		protected virtual void ClientOnWsMessage (object source, SpeckleEventArgs e)
		{

		}

		/// <summary>
		/// Base implementation of the "OnError" event response.  It just logs an error and is intended to be 
		/// overriden.
		/// </summary>
		/// <param name="source">Source object passed from the event.</param>
		/// <param name="e">Arguments passed from the event.</param>
		protected virtual void ClientOnError (object source, SpeckleEventArgs e)
		{
			Debug.LogError (string.Format ("INTERNAL ERROR ({0}): {1}", e.EventName, e.EventData));
		}
	}
}