using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpeckleUnity
{
	/// <summary>
	/// A custom <c>UnityEvent</c> type for assigning methods to in the inspector of a manger instance.
	/// The methods need to be public and accept a single argument of type <c>SpeckleUnityUpdate</c>
	/// in order to be assignable.
	/// </summary>
	[Serializable]
	public class SpeckleUnityUpdateEvent : UnityEvent<SpeckleUnityUpdate> { }
}