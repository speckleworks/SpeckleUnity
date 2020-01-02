using System;
using UnityEngine;
using SpeckleCore;


namespace SpeckleUnity
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public abstract class SpeckleUnityClient : MonoBehaviour
	{
		/// <summary>
		/// 
		/// </summary>
		protected string unityGUID;

		/// <summary>
		/// 
		/// </summary>
		public SpeckleApiClient client;

		/// <summary>
		/// 
		/// </summary>
		public string streamID;

		/// <summary>
		/// 
		/// </summary>
		public string authToken = "";

		/// <summary>
		/// 
		/// </summary>
		protected virtual void AssignEvents ()
		{
			client.OnReady += ClientOnReady;
			client.OnLogData += ClientOnLogData;
			client.OnWsMessage += ClientOnWsMessage;
			client.OnError += ClientOnError;
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual void DisposeClient ()
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
		/// <param name="URL"></param>
		public abstract void InitializeClient (string URL);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="client"></param>
		public abstract void CompleteDeserialization (SpeckleApiClient client);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		public virtual void ClientOnReady (object source, SpeckleEventArgs e)
		{
			Debug.Log ("Client ready");
			//Debug.Log(JsonConvert.SerializeObject(e.EventData));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		public virtual void ClientOnLogData (object source, SpeckleEventArgs e)
		{
			Debug.Log ("Client LogData");
			//Debug.Log(JsonConvert.SerializeObject(e.EventData));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		public virtual void ClientOnWsMessage (object source, SpeckleEventArgs e)
		{
			Debug.Log ("Client WsMessage");
			//Debug.Log(JsonConvert.SerializeObject(e.EventData));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		public virtual void ClientOnError (object source, SpeckleEventArgs e)
		{
			Debug.Log ("Client Error");
			//Debug.Log(JsonConvert.SerializeObject(e.EventData));
		}

	}

}
