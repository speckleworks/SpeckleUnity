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
		manager.LoginAsync ("email", "password", HandleLogin);
	}

	public void HandleLogin (User resultUser)
	{
		if (resultUser != null) manager.GetAllStreamMetaDataForUserAsync (HandleStreamsForUser);
	}

	public void HandleStreamsForUser (SpeckleStream[] streamIDs)
	{
		if (streamIDs != null) manager.AddReceiverAsync (streamIDs[0].StreamId, null, true);
	}

	public void PrintObjectData (GameObject gameObjectKey)
	{
		if (manager.TryGetSpeckleObject (gameObjectKey, out SpeckleObject speckleObjectData))
		{
			Debug.Log (speckleObjectData._id);
			Debug.Log (speckleObjectData.Type);

			if (speckleObjectData.Properties.TryGetValue ("the key of some property you're looking for", out object property))
			{
				Debug.Log (property.ToString ());
			}
		}
	}
}
