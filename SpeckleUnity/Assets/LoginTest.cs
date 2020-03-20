using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpeckleCore;
using SpeckleUnity;

public class LoginTest : MonoBehaviour
{
	public SpeckleUnityManager manager;

	void Start ()
	{
		manager.Login ("email", "password", HandleLogin);
	}

	public void HandleLogin (User resultUser)
	{
		if (resultUser != null) manager.GetAllStreamsForUser (HandleStreamsForUser);
	}

	public void HandleStreamsForUser (SpeckleStream[] streamIDs)
	{
		if (streamIDs != null) manager.AddReceiver (streamIDs[0].StreamId, null, true);
	}
}
