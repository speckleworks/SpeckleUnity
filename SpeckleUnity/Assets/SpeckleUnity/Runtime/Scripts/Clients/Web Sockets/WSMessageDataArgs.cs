using System;

namespace SpeckleUnity
{
	/// <summary>
	/// Concrete type for the json structure of the nested web socket message argument data to
	/// deserialize into.
	/// </summary>
	[Serializable]
	public class WSMessageDataArgs
	{
		/// <summary>
		/// 
		/// </summary>
		public string eventType;
	}
}