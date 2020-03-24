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
		[SerializeField] protected StartMode onStartBehaviour = StartMode.LoginAndReceiveStreams;

		/// <summary>
		/// The server to send / receive streams from and authenticate against. Changing this value during
		/// runtime requires calling <c>InitializeAllClients ()</c> again.
		/// </summary>
		[Header ("Server Settings")]
		[SerializeField] protected string serverUrl = "https://hestia.speckle.works/api/";

		/// <summary>
		/// The email to login with on start if <c>onStartBehaviour</c> is set to at least <c>JustLogin</c>.
		/// </summary>
		[SerializeField] protected string loginEmail = "";

		/// <summary>
		/// The password to login with on start if <c>onStartBehaviour</c> is set to at least <c>JustLogin</c>.
		/// </summary>
		[SerializeField] protected string loginPassword = "";

		/// <summary>
		/// A cached reference to the current logged in user.
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
		[SerializeField] protected internal SpawnSpeed spawnSpeed = SpawnSpeed.TenPerFrame;

		/// <summary>
		/// A <c>UnityEvent</c> that is invoked each time a stream update is started, including when it's initialised, for user code to 
		/// respond to that event. Passes some helpful data to inform that custom response.
		/// </summary>
		[SerializeField] protected internal SpeckleUnityUpdateEvent onUpdateStarted;

		/// <summary>
		/// A <c>UnityEvent</c> that is invoked each time a stream update is finished, including when it's initialised, for user code to 
		/// respond to that event. Passes some helpful data to inform that custom response.
		/// </summary>
		[SerializeField] protected internal SpeckleUnityUpdateEvent onUpdateReceived;

		/// <summary>
		/// A list of all the <c>SpeckleUnityReceivers</c> this manager controls. Intended to only be directly editable via
		/// the inspector. During runtime you should make use of the <c>RemoveReceiver ()</c> or <c>AddReceiver ()</c> methods.
		/// </summary>
		[SerializeField] protected List<SpeckleUnityReceiver> receivers = new List<SpeckleUnityReceiver> ();

		/// <summary>
		/// Initializes Speckle and assigns the scale factor of all geometry. Invokes the <c>RunStartBehaviour ()</c>
		/// coroutine.
		/// </summary>
		protected virtual void Start ()
		{
			SpeckleInitializer.Initialize (false);
			Conversions.scaleFactor = scaleFactor;

			StartCoroutine (RunStartBehaviour ());
		}

		/// <summary>
		/// Intended for running additional actions on start depending on the value of the <c>onStartBehaviour</c> enum.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator RunStartBehaviour ()
		{
			if (onStartBehaviour > StartMode.DoNothing)
			{
				if (!string.IsNullOrWhiteSpace (loginEmail) && !string.IsNullOrWhiteSpace (loginPassword))
				{
					yield return StartCoroutine (AttemptLogin (loginEmail, loginPassword, null));
				}
				else
				{
					Debug.LogError ("The Email and Password fields need to be filled in if you wish to login on start.");
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
		/// Exposed method for users to call when trying to login to a Speckle server via code.
		/// </summary>
		/// <param name="email">The email of the account you wish to login with.</param>
		/// <param name="password">The corresponding password for the account.</param>
		/// <param name="callBack">A method callback which takes a <c>User</c>.</param>
		/// <remarks>If login was successful, the resulting user object is passed back. If failed, null
		/// is passed. Need to be using the <c>SpeckleCore</c> namespace to access this type.</remarks>
		public virtual void Login (string email, string password, Action<User> callBack)
		{
			StartCoroutine (AttemptLogin (email, password, callBack));
		}

		/// <summary>
		/// Coroutine for running the asyncronous login process.
		/// </summary>
		/// <param name="email">The email of the account you wish to login with.</param>
		/// <param name="password">The corresponding password for the account.</param>
		/// <param name="callBack">A method callback which takes a <c>User</c>.</param>
		/// <returns>An IEnumerator to yield or start as a new coroutine.</returns>
		/// <remarks>If login was successful, the resulting user object is passed back. If failed, null
		/// is passed. Need to be using the <c>SpeckleCore</c> namespace to access this type.</remarks>
		protected virtual IEnumerator AttemptLogin (string email, string password, Action<User> callBack)
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
		/// Sets the <c>loggedInUser</c> to null and clears all the current receivers.
		/// </summary>
		public virtual void Logout ()
		{
			loggedInUser = null;
			ClearReceivers ();
		}

		/// <summary>
		/// Exposed method for users to call when trying to download the meta data of the Projects the current
		/// logged in user is able to access.
		/// </summary>
		/// <param name="callBack">A method callback which takes a <c>Project</c> array.</param>
		public virtual void GetAllProjectMetaDataForUser (Action<Project[]> callBack)
		{
			StartCoroutine (AttemptGetAllProjectMetaDataForUser (callBack));
		}

		/// <summary>
		/// Coroutine for running the asyncronous Project download process.
		/// </summary>
		/// <param name="callBack">A method callback which takes a <c>Project</c> array.</param>
		/// <returns>An IEnumerator to yield or start as a new coroutine.</returns>
		/// <remarks>If download was successful, the resulting array is passed back. If failed, null
		/// is passed. Need to be using the <c>SpeckleCore</c> namespace to access this type.</remarks>
		protected virtual IEnumerator AttemptGetAllProjectMetaDataForUser (Action<Project[]> callBack)
		{
			SpeckleApiClient userProjectsClient = new SpeckleApiClient (serverUrl);
			userProjectsClient.AuthToken = loggedInUser.Apitoken;

			Task<ResponseProject> projectsGet = userProjectsClient.ProjectGetAllAsync ();
			while (!projectsGet.IsCompleted) yield return null;

			if (projectsGet.Result == null)
			{
				Debug.LogError ("Could not get projects for user");

				callBack?.Invoke (null);
			}
			else
			{
				List<Project> projects = projectsGet.Result.Resources;

				Debug.Log ("Got " + projects.Count + " projects for user");

				callBack?.Invoke (projects.ToArray ());
			}
		}

		/// <summary>
		/// Exposed method for users to call when trying to download the meta data of the Streams the current
		/// logged in user is able to access. Use this to get the IDs of the streams you would later want to start
		/// receiving or use the rest of the data to populate your UI with data describing the Streams that are
		/// available.
		/// </summary>
		/// <param name="callBack">A method callback which takes a <c>SpeckleStream</c> array.</param>
		/// <remarks>If download was successful, the resulting array is passed back. If failed, null
		/// is passed. Need to be using the <c>SpeckleCore</c> namespace to access this type.</remarks>
		public virtual void GetAllStreamMetaDataForUser (Action<SpeckleStream[]> callBack)
		{
			StartCoroutine (AttemptGetAllStreamMetaDataForUser (callBack));
		}

		/// <summary>
		/// Coroutine for running the asyncronous Stream download process.
		/// </summary>
		/// <param name="callBack">A method callback which takes a <c>SpeckleStream</c> array.</param>
		/// <returns>An IEnumerator to yield or start as a new coroutine.</returns>
		/// <remarks>If download was successful, the resulting array is passed back. If failed, null
		/// is passed. Need to be using the <c>SpeckleCore</c> namespace to access this type.</remarks>
		protected virtual IEnumerator AttemptGetAllStreamMetaDataForUser (Action<SpeckleStream[]> callBack)
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
		/// <param name="streamID">The ID of the stream to be removed. If no matching ID is found, nothing will
		/// happen.</param>
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
		/// <param name="streamRoot">The root object of the stream to be removed. If no matching root is 
		/// found, nothing will happen.</param>
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
		/// Calls <c>RemoveReceiver (int)</c> on all receivers on this manager instance.
		/// </summary>
		public virtual void ClearReceivers ()
		{
			for (int i = 0; i < receivers.Count; i++)
			{
				RemoveReceiver (i);
			}
		}


		/// <summary>
		/// Checks through all receivers and looks up their <c>GameObject</c> to <c>SpeckleObject</c> dictionaries and outputs 
		/// the <c>SpeckleObject</c>. 
		/// </summary>
		/// <param name="gameObjectKey">The <c>GameObject</c> in the scene that represents the object to get data for.</param>
		/// <param name="speckleObjectData">The <c>SpeckleObject</c> that corresponds to the <c>GameObject</c>. Outputs as 
		/// null if the lookup failed.</param>
		/// <returns>Whether or not the lookup was successful.</returns>
		public virtual bool TryGetSpeckleObject (GameObject gameObjectKey, out SpeckleObject speckleObjectData)
		{
			speckleObjectData = null;

			if (gameObjectKey == null) return false;

			for (int i = 0; i < receivers.Count; i++)
			{
				bool gotValue = receivers[i].speckleObjectLookup.TryGetValue (gameObjectKey, out SpeckleObject speckleObject);

				if (gotValue)
				{
					speckleObjectData = speckleObject;
					return true;
				}
			}

			return false;
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
	/// An enum for controlling what the default start behaviour of a <c>SpeckleUnityManager</c>
	/// would be. 
	/// </summary>
	public enum StartMode : int
	{
		DoNothing = 0,
		JustLogin = 1,
		LoginAndReceiveStreams = 2
	}
}