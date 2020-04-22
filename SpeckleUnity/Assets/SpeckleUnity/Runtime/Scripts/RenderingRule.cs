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
		/// <param name="speckleObject"></param>
		/// <param name="block"></param>
		public abstract void ApplyRuleToObject (Renderer renderer, SpeckleObject speckleObject, MaterialPropertyBlock block);
	}
}