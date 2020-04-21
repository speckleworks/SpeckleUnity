using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SpeckleCore;
using System.Threading.Tasks;

namespace SpeckleUnity.Tests
{
    public class SpeckleUnityManagerTests
    {
		SpeckleUnityManager manager;

		User loggedInUser;
		Project[] projectData;
		SpeckleStream[] streamData;

		bool gotCallback;

		TestInput workingCredentials;

		[OneTimeSetUp]
		public void BeforeAllTests ()
		{
			workingCredentials = Resources.Load<TestInput> ("User Credentials");
		}

		[SetUp]
		public void BeforeTest ()
		{
			manager = new GameObject ().AddComponent<SpeckleUnityManager> ();
			manager.onStartBehaviour = StartMode.DoNothing;

			loggedInUser = null;

			gotCallback = false;
		}

		[TearDown]
		public void AfterTest ()
		{
			manager.ClearReceivers ();
			GameObject.Destroy (manager.gameObject);
		}

		public void LoginCallback (User userResult)
		{
			gotCallback = true;
			loggedInUser = userResult;
		}

		public void ProjectCallback (Project[] projectDataResult)
		{
			gotCallback = true;
			projectData = projectDataResult;
		}

		public void StreamCallback (SpeckleStream[] streamDataResult)
		{
			gotCallback = true;
			streamData = streamDataResult;
		}

		[UnityTest]
		public async Task OnStartDoNothingSuccess ()
		{
			await manager.RunStartBehaviourAsync ();

			Assert.IsNull (manager.loggedInUser);
		}

		[UnityTest]
		public async Task OnStartJustLoginSuccess ()
		{
			manager.onStartBehaviour = StartMode.JustLogin;
			manager.startLoginEmail = workingCredentials.email;
			manager.startLoginPassword = workingCredentials.password;

			await manager.RunStartBehaviourAsync ();

			Assert.NotNull (manager.loggedInUser);
			Assert.NotNull (manager.loggedInUser.Apitoken);
		}

		// SHOULD NOT PASS
		[UnityTest]
		public async Task OnStartLoginAndReceiveStreamsSuccess ()
		{
			Transform root = new GameObject ().transform;

			manager.onStartBehaviour = StartMode.LoginAndReceiveStreams;
			manager.startLoginEmail = workingCredentials.email;
			manager.startLoginPassword = workingCredentials.password;
			await manager.AddReceiverAsync (workingCredentials.streamID, root);

			await manager.RunStartBehaviourAsync ();

			Debug.Log (root.childCount);
			Assert.True (root.childCount < 0);
			Assert.NotNull (manager.loggedInUser);
			Assert.NotNull (manager.loggedInUser.Apitoken);
		}

		[UnityTest]
		public async Task AddReceiver ()
		{
			await manager.AddReceiverAsync (workingCredentials.streamID);

			Assert.True (manager.ReceiverCount == 1);
		}

		[UnityTest]
		public async Task AddReceiverAndInitializeWithWorkingID ()
		{
			await manager.AddReceiverAsync (workingCredentials.streamID, null, true);

			Assert.True (manager.ReceiverCount == 1);
		}

		// THIS TEST SHOULDNT PASS
		[UnityTest]
		public async Task AddReceiverAndInitializeWithBrokenID ()
		{
			await manager.AddReceiverAsync ("", null, true);

			Assert.True (manager.ReceiverCount == 1);
		}

		[UnityTest]
		public async Task LoginSuccess ()
		{
			await manager.LoginAsync (workingCredentials.email, workingCredentials.password, LoginCallback);

			Assert.NotNull (manager.loggedInUser);
			Assert.NotNull (manager.loggedInUser.Apitoken);
			Assert.AreSame (loggedInUser, manager.loggedInUser);
			Assert.True (gotCallback);
		}

		[UnityTest]
		public async Task LoginThenLogoutSuccess ()
		{
			await manager.LoginAsync (workingCredentials.email, workingCredentials.password, LoginCallback);

			manager.Logout ();

			Assert.IsNull (manager.loggedInUser);
			Assert.True (manager.ReceiverCount == 0);
		}


		[UnityTest]
		public async Task LoginWithWrongEmailOrPassword ()
		{
			await AssertAsync.ThrowsAsync<SpeckleException> (manager.LoginAsync ("email", "password", LoginCallback));
		}

		[UnityTest]
		public async Task ConnectToInvalidServer ()
		{
			manager.SetServerUrl ("I am a wrong server URL");

			await AssertAsync.ThrowsAsync<SpeckleException> (manager.LoginAsync ("email", "password", LoginCallback));
		}

		[UnityTest]
		public async Task GetProjectMetaDataWithoutLogin ()
		{
			await AssertAsync.ThrowsAsync<SpeckleException> (manager.GetAllProjectMetaDataForUserAsync (ProjectCallback));
		}

		[UnityTest]
		public async Task GetProjectMetaDataSuccess ()
		{
			await manager.LoginAsync (workingCredentials.email, workingCredentials.password, null);
			await manager.GetAllProjectMetaDataForUserAsync (ProjectCallback);

			Assert.NotNull (projectData);
			Assert.True (gotCallback);
		}

		[UnityTest]
		public async Task GetStreamMetaDataWithoutLogin ()
		{
			await AssertAsync.ThrowsAsync<SpeckleException> (manager.GetAllStreamMetaDataForUserAsync (StreamCallback));
		}

		[UnityTest]
		public async Task GetStreamtMetaDataSuccess ()
		{
			await manager.LoginAsync (workingCredentials.email, workingCredentials.password, null);
			await manager.GetAllStreamMetaDataForUserAsync (StreamCallback);

			Assert.NotNull (streamData);
			Assert.True (gotCallback);
		}
	}

	public static class AssertAsync
	{
		public static async Task ThrowsAsync<T> (Task asyncMethod) where T : Exception
		{
			await ThrowsAsync<T> (asyncMethod, "");
		}

		public static async Task ThrowsAsync<T> (Task asyncMethod, string message) where T : Exception
		{
			try
			{
				await asyncMethod; //Should throw..
			}
			catch (T)
			{
				//Ok! Swallow the exception.
				return;
			}
			catch (Exception e)
			{
				if (message != "")
				{
					Assert.That (e, Is.TypeOf<T> (), message + " " + e.ToString ()); //of course this fail because it goes through the first catch..
				}
				else
				{
					Assert.That (e, Is.TypeOf<T> (), e.ToString ());
				}
				throw; //probably unreachable
			}
			Assert.Fail ("Expected an exception of type " + typeof (T).FullName + " but no exception was thrown.");
		}
	}
}