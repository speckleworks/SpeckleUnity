using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpeckleUnity.Tests

{
	[CreateAssetMenu (menuName = "SpeckleUnity/TestInput")]
	public class TestInput : ScriptableObject
	{
		public string email;
		public string password;
		public string streamID;
	}
}