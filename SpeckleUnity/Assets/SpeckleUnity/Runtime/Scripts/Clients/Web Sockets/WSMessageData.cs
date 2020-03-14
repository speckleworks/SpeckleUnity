using System;

namespace SpeckleUnity
{
	/// <summary>
	/// Concrete type for the json structure of the web socket message data to deserialize into.
	/// </summary>
	[Serializable]
	public class WSMessageData
	{
		/// <summary>
		/// 
		/// </summary>
		public string eventName;

		/// <summary>
		/// 
		/// </summary>
		public string senderId;

		/// <summary>
		/// 
		/// </summary>
		public string resourceType;

		/// <summary>
		/// 
		/// </summary>
		public string resourceId;

		/// <summary>
		/// 
		/// </summary>
		public WSMessageDataArgs args;
	}
}