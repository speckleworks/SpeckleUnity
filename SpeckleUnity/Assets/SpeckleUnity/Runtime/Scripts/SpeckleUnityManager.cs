using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SpeckleCore;
using System.IO;
using System.Threading.Tasks;
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
		/// 
		/// </summary>
		public StartMode onStartBehaviour = StartMode.LoginAndReceiveStreams;

		/// <summary>
		/// The server to send / receive streams from and authenticate against. Changing this value during
		/// runtime requires calling <c>InitializeAllClients ()</c> again.
		/// </summary>
		[Header ("Server Settings")]
		public string serverUrl = "https://hestia.speckle.works/api/";

		/// <summary>
		/// 
		/// </summary>
		[SerializeField] protected string loginEmail = "";

		/// <summary>
		/// 
		/// </summary>
		[SerializeField] protected string loginPassword = "";

		/// <summary>
		/// 
		/// </summary>
		protected User loggedInUser;

		/// <summary>
		/// Assigns to the <c>MeshRenderer</c>s of every brep or mesh object for every stream handled by this manager.
		/// </summary>
		[Header ("Rendering Settings")]
		public Material meshMaterial;

		/// <summary>
		/// Assigns to the <c>LineRenderer</c>s of every line, curve or polyline object for every stream handled by this manager.
		/// </summary>
		public Material lineMaterial;

		/// <summary>
		/// Assigns to the <c>LineRenderer</c>s of every point object for every stream handled by this manager.
		/// </summary>
		public Material pointMaterial;

		/// <summary>
		/// A value for easily setting the static <c>Conversions.scaleFactor</c> value via the inspector.
		/// This class assigs the value once on <c>Start ()</c>. Default value is 0.001 because it's assuming
		/// that the stream was modelled in milimeters and needs to be scaled to 
		/// </summary>
		[SerializeField] protected double scaleFactor = 0.001;

		/// <summary>
		/// Speed value to allow for instantiation to happen gradually over many frames in case of 
		/// performance issues with large streams that get initialized / updated.
		/// </summary>
		[Header ("Receiver Settings")]
		public SpawnSpeed spawnSpeed = SpawnSpeed.TenPerFrame;

		/// <summary>
		/// A <c>UnityEvent</c> that is invoked each time a stream update is started, including when it's initialised, for user code to 
		/// respond to that event. Passes some helpful data to inform that custom response.
		/// </summary>
		public SpeckleUnityUpdateEvent onUpdateStarted;

		/// <summary>
		/// A <c>UnityEvent</c> that is invoked each time a stream update is finished, including when it's initialised, for user code to 
		/// respond to that event. Passes some helpful data to inform that custom response.
		/// </summary>
		public SpeckleUnityUpdateEvent onUpdateReceived;

		/// <summary>
		/// A list of all the <c>SpeckleUnityReceivers</c> this manager controls. Intended to only be directly editable via
		/// the inspector. During runtime you should make use of the <c>RemoveReceiver ()</c> or <c>AddReceiver ()</c> methods.
		/// </summary>
		[SerializeField] protected List<SpeckleUnityReceiver> receivers = new List<SpeckleUnityReceiver> ();

		/// <summary>
		/// Initializes Speckle and assigns the scale factor of all geometry. If <c>receiveStreamsOnStart</c> is set
		/// to true, calls <c>InitializeAllClients ()</c>.
		/// </summary>
		protected virtual void Start ()
		{
			SpeckleInitializer.Initialize (false);
			Conversions.scaleFactor = scaleFactor;

			StartCoroutine (RunStartBehaviour ());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator RunStartBehaviour ()
		{
			if (onStartBehaviour > StartMode.DoNothing)
			{
				if (!string.IsNullOrWhiteSpace (loginEmail) && !string.IsNullOrWhiteSpace (loginPassword))
				{
					yield return StartCoroutine (AttemptLogin (loginEmail, loginPassword));
				}
			}

			if (onStartBehaviour > StartMode.JustLogin)
			{
				if (!string.IsNullOrWhiteSpace (loggedInUser?.Apitoken))
				{
					InitializeAllClients ();
				}
				else
				{
					Debug.LogError ("User has no API token.");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <param name="callBack"></param>
		public virtual void Login (string email, string password, Action<User> callBack = null)
		{
			StartCoroutine (AttemptLogin (email, password, callBack));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <param name="callBack"></param>
		/// <returns></returns>
		protected virtual IEnumerator AttemptLogin (string email, string password, Action<User> callBack = null)
		{
			SpeckleApiClient loginClient = new SpeckleApiClient (serverUrl);

			User user = new User { Email = email, Password = password };

			Task<ResponseUser> userGet = loginClient.UserLoginAsync (user);
			Debug.Log ("Atempting login");
			while (!userGet.IsCompleted) yield return null;

			if (userGet.Result == null)
			{
				Debug.LogError ("Could not login");

				callBack?.Invoke (null);
			}
			else
			{
				loggedInUser = userGet.Result.Resource;
				Debug.Log ("Logged in as " + loggedInUser.Name + " " + loggedInUser.Surname);

				loginClient?.Dispose (true);

				callBack?.Invoke (loggedInUser);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callBack"></param>
		public virtual void GetAllStreamsForUser (Action<SpeckleStream[]> callBack)
		{
			StartCoroutine (AttemptGetAllStreamsForUser (callBack));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callBack"></param>
		/// <returns></returns>
		protected virtual IEnumerator AttemptGetAllStreamsForUser (Action<SpeckleStream[]> callBack)
		{
			SpeckleApiClient userStreamsClient = new SpeckleApiClient (serverUrl);
			userStreamsClient.AuthToken = loggedInUser.Apitoken;

			Task<ResponseStream> streamsGet = userStreamsClient.StreamsGetAllAsync (null);
			while (!streamsGet.IsCompleted) yield return null;

			if (streamsGet.Result == null)
			{
				Debug.LogError ("Could not get streams for user");
				
				callBack?.Invoke (null);
			}
			else
			{
				List<SpeckleStream> streams = streamsGet.Result.Resources;
				
				Debug.Log ("Got " + streams.Count + " streams for user");

				callBack?.Invoke (streams.ToArray ());
			}
		}

		/// <summary>
		/// Loops through each receiver and starts each of their initialization coroutines.
		/// </summary>
		public virtual void InitializeAllClients ()
		{
			for (int i = 0; i < receivers.Count; i++)
			{
				StartCoroutine (receivers[i].InitializeClient (this, serverUrl, loggedInUser.Apitoken));
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
				StartCoroutine (newReceiver.InitializeClient (this, serverUrl, loggedInUser.Apitoken));
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

		/// <summary>
		/// 
		/// </summary>
		public virtual void ClearReceivers ()
		{
			for (int i = 0; i < receivers.Count; i++)
			{
				RemoveReceiver (i);
			}
		}
	}

	/// <summary>
	/// An enum for controlling how quickly geometry objects get isntantiated into the scene.
	/// The Instant value simply means all objects will be spawned in the same frame. (Technically,
	/// it caps out at 2 billion, but who in the world is making streams bigger than 2 billion...)
	/// </summary>
	public enum SpawnSpeed : int
	{
		Instant = int.MaxValue,
		ThousandPerFrame = 1000,
		HundredPerFrame = 100,
		TenPerFrame = 10
	}

	/// <summary>
	/// 
	/// </summary>
	public enum StartMode : int
	{ 
		DoNothing = 0,
		JustLogin = 1,
		LoginAndReceiveStreams = 2
	}
}