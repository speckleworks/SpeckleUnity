using System;
using UnityEngine;
using SpeckleCore;
using UnityEngine.Events;

namespace SpeckleUnity
{
	[Serializable]
	public class WSMessageData
	{
		public string eventName;
		public string senderId;
		public string resourceType;
		public string resourceId;
		public WSMessageDataArgs args;
	}
	[Serializable]
	public class WSMessageDataArgs
	{
		public string eventType;
	}

	/// <summary>
	/// Creating class of event to be called from clients to allow developers to add listeners to new geometry received and other events 
	/// </summary>
	[Serializable]
	public class ReceiverEvent : UnityEvent<SpeckleUnityClient>
	{
	}


	[Serializable]
	public abstract class SpeckleUnityClient : MonoBehaviour
	{
		protected string unity_guid;
		public SpeckleApiClient Client { get; set; }

		[SerializeField]
		[Header ("Speckle Client")]
		[Tooltip ("For a reciever, enter the stream ID here. For a sender, leave this blank.")]
		public string StreamId;

		[Header ("Persistence")]
		[Tooltip ("Set to true to save this client. Only used for senders.")]
		public bool Persistent = false;

		//USed to find the appropriate client next time the application is loaded
		//Needs to be defined in editor, so can't use client speckle id which won't exist
		//Only used if persistent is set to true
		[Tooltip ("Enter a unique value for saving this sending client")]
		public string KeyForSaving;

		//auth token
		public string authToken = "";

		protected virtual void AssignEvents ()
		{
			Client.OnReady += Client_OnReady;
			Client.OnLogData += Client_OnLogData;
			Client.OnWsMessage += Client_OnWsMessage;
			Client.OnError += Client_OnError;
		}

		public virtual void DisposeClient ()
		{
			Client.OnReady -= Client_OnReady;
			Client.OnLogData -= Client_OnLogData;
			Client.OnWsMessage -= Client_OnWsMessage;
			Client.OnError -= Client_OnError;

			Client.Dispose (true);
		}

		public abstract void InitializeClient (string URL);

		public abstract void CompleteDeserialization (SpeckleApiClient client);

		public virtual void Client_OnReady (object source, SpeckleEventArgs e)
		{
			Debug.Log ("Client ready");
			//Debug.Log(JsonConvert.SerializeObject(e.EventData));
		}

		public virtual void Client_OnLogData (object source, SpeckleEventArgs e)
		{
			Debug.Log ("Client LogData");
			//Debug.Log(JsonConvert.SerializeObject(e.EventData));
		}

		public virtual void Client_OnWsMessage (object source, SpeckleEventArgs e)
		{
			Debug.Log ("Client WsMessage");
			//Debug.Log(JsonConvert.SerializeObject(e.EventData));
		}

		public virtual void Client_OnError (object source, SpeckleEventArgs e)
		{
			Debug.Log ("Client Error");
			//Debug.Log(JsonConvert.SerializeObject(e.EventData));
		}

	}

}
