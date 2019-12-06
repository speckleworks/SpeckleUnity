using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpeckleUnity
{
	public class SpeckleSendTransform : SpeckleSend
	{
		//set to true when moving, if transform has not moved, and hasMoved is true, send update
		protected bool hasMoved = false;

		protected virtual void Start ()
		{
			obj = new SpeckleUnityTransform (this.gameObject);
			Sender?.RegisterObject (obj);
		}

		protected virtual void Update ()
		{
			//don't update every frame - check once it has stopped moving          
			if (transform.hasChanged)
			{
				hasMoved = true;
				transform.hasChanged = false;
			}
			else
			{
				if (hasMoved)
				{
					hasMoved = false;
					if (obj != null)
					{
						obj.OnValueChanged ();
					}
				}
			}


		}
	}
}


