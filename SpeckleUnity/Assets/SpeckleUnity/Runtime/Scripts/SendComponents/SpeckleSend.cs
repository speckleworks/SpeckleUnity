using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This is a parent class for components to add to gameobjects that we want to send to speckle
//

namespace SpeckleUnity
{
	public class SpeckleSend : MonoBehaviour
	{
		//Specify which sender to add this object to
		public SpeckleUnitySender Sender;

		//object to send
		protected SpeckleUnityObject obj;

		//TODO - implement layers
		//public string LayerName = "Default Unity";

		//Base classes should call obj.OnValueChanged() to prompt the sender to send an update to the server


	}
}

