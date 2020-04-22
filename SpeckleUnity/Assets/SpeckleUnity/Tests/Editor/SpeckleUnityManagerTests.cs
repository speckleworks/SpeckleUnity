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
		public IEnumerator OnStartDoNothingSuccess ()
		{
			yield return AsyncTest.Execute (manager.RunStartBehaviourAsync ());

			Assert.IsNull (manager.loggedInUser);
		}

		[UnityTest]
		public IEnumerator OnStartJustLoginSuccess ()
		{
			manager.onStartBehaviour = StartMode.JustLogin;
			manager.startLoginEmail = workingCredentials.email;
			manager.startLoginPassword = workingCredentials.password;

			yield return AsyncTest.Execute (manager.RunStartBehaviourAsync ());

			Assert.NotNull (manager.loggedInUser);
			Assert.NotNull (manager.loggedInUser.Apitoken);
		}

		[UnityTest]
		public IEnumerator OnStartLoginAndReceiveStreamsSuccess ()
		{
			Transform root = new GameObject ().transform;

			manager.onStartBehaviour = StartMode.LoginAndReceiveStreams;
			manager.startLoginEmail = workingCredentials.email;
			manager.startLoginPassword = workingCredentials.password;

			yield return AsyncTest.Execute (manager.AddReceiverAsync (workingCredentials.streamID, root));
			yield return AsyncTest.Execute (manager.RunStartBehaviourAsync ());

			Assert.True (root.childCount > 0);
			Assert.NotNull (manager.loggedInUser);
			Assert.NotNull (manager.loggedInUser.Apitoken);
		}

		[UnityTest]
		public IEnumerator AddReceiver ()
		{
			yield return AsyncTest.Execute (manager.AddReceiverAsync (workingCredentials.streamID));

			Assert.True (manager.ReceiverCount == 1);
		}

		[UnityTest]
		public IEnumerator AddReceiverAndInitializeSuccess ()
		{
			Transform root = new GameObject ().transform;

			yield return AsyncTest.Execute (manager.LoginAsync (workingCredentials.email, workingCredentials.password, LoginCallback));
			yield return AsyncTest.Execute (manager.AddReceiverAsync (workingCredentials.streamID, root, true));

			Assert.True (manager.ReceiverCount == 1);
		}

		[UnityTest]
		public IEnumerator AddReceiverAndInitializeWithInvalidID ()
		{
			Transform root = new GameObject ().transform;

			yield return AsyncTest.Execute (manager.LoginAsync (workingCredentials.email, workingCredentials.password, LoginCallback));
			yield return AsyncTest.Execute (AsyncTest.ThrowsAsync<SpeckleException> (manager.AddReceiverAsync ("not valid stream ID", root, true)));

			LogAssert.Expect (LogType.Error, "INTERNAL ERROR: The HTTP status code of the response was not expected (404).");
		}

		[UnityTest]
		public IEnumerator LoginSuccess ()
		{
			yield return AsyncTest.Execute (manager.LoginAsync (workingCredentials.email, workingCredentials.password, LoginCallback));

			Assert.NotNull (manager.loggedInUser);
			Assert.NotNull (manager.loggedInUser.Apitoken);
			Assert.AreSame (loggedInUser, manager.loggedInUser);
			Assert.True (gotCallback);
		}

		[UnityTest]
		public IEnumerator LoginThenLogoutSuccess ()
		{
			yield return AsyncTest.Execute (manager.LoginAsync (workingCredentials.email, workingCredentials.password, LoginCallback));

			manager.Logout ();

			Assert.IsNull (manager.loggedInUser);
			Assert.True (manager.ReceiverCount == 0);
		}


		[UnityTest]
		public IEnumerator LoginWithWrongEmailOrPassword ()
		{
			yield return AsyncTest.Execute (AsyncTest.ThrowsAsync<SpeckleException> (manager.LoginAsync ("email", "password", LoginCallback)));
		}

		[UnityTest]
		public IEnumerator ConnectToInvalidServer ()
		{
			manager.SetServerUrl ("I am a wrong server URL");

			yield return AsyncTest.Execute (AsyncTest.ThrowsAsync<InvalidOperationException> (manager.LoginAsync ("email", "password", LoginCallback)));
		}

		[UnityTest]
		public IEnumerator GetProjectMetaDataWithoutLogin ()
		{
			yield return AsyncTest.Execute (AsyncTest.ThrowsAsync<UnauthorizedAccessException> (manager.GetAllProjectMetaDataForUserAsync (ProjectCallback)));
		}

		[UnityTest]
		public IEnumerator GetProjectMetaDataSuccess ()
		{
			yield return AsyncTest.Execute (manager.LoginAsync (workingCredentials.email, workingCredentials.password, null));
			yield return AsyncTest.Execute (manager.GetAllProjectMetaDataForUserAsync (ProjectCallback));

			Assert.NotNull (projectData);
			Assert.True (gotCallback);
		}

		[UnityTest]
		public IEnumerator GetStreamMetaDataWithoutLogin ()
		{
			yield return AsyncTest.Execute (AsyncTest.ThrowsAsync<UnauthorizedAccessException> (manager.GetAllStreamMetaDataForUserAsync (StreamCallback)));
		}

		[UnityTest]
		public IEnumerator GetStreamtMetaDataSuccess ()
		{
			yield return AsyncTest.Execute (manager.LoginAsync (workingCredentials.email, workingCredentials.password, null));
			yield return AsyncTest.Execute (manager.GetAllStreamMetaDataForUserAsync (StreamCallback));

			Assert.NotNull (streamData);
			Assert.True (gotCallback);
		}
	}

	public static class AsyncTest
	{
		public static IEnumerator Execute (Task task)
		{
			while (!task.IsCompleted) { yield return null; }
			if (task.IsFaulted) { throw task.Exception; }
		}

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