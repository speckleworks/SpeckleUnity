using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpeckleCore;

namespace SpeckleUnity
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class RenderingRule : ScriptableObject
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="renderer"></param>
		/// <param name="speckleStream"></param>
		/// <param name="objectIndex"></param>
		/// <param name="block"></param>
		public abstract void ApplyRuleToObject (Renderer renderer, SpeckleStream speckleStream, int objectIndex, MaterialPropertyBlock block);
	}
}