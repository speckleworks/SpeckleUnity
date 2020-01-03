using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpeckleUnity
{

	public class SpeckleSendSlider : SpeckleSend
	{
		protected SpeckleUnityNumber number;
		protected Slider Slider;

		protected virtual void Start ()
		{
			Slider = transform.GetComponent<Slider> ();
			number = new SpeckleUnityNumber (Slider.value);
			Sender?.RegisterObject (number);

			Slider.onValueChanged.AddListener (HandleSliderChange);
		}

		//This sends a new value every frame
		//Either this should be rewritten to only send once it has stopped changing, or the sender should implement a timer to prevent too many updates
		protected virtual void HandleSliderChange (float value)
		{
			number.value = value;
			number.OnValueChanged ();
		}
	}

}
