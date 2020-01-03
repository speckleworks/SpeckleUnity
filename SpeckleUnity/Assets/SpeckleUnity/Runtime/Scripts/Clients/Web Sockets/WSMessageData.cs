using System;

namespace SpeckleUnity
{
	/// <summary>
	/// 
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