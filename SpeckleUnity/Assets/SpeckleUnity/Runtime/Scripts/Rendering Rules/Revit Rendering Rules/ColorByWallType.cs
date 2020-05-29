using System.Collections;
using System.Collections.Generic;
using SpeckleCore;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpeckleUnity
{
	/// <summary>
	/// 
	/// </summary>
	[CreateAssetMenu (menuName = "SpeckleUnity/Rendering Rules/Color By Wall Type (Revit)")]
	public class ColorByWallType : ColorByRevitType
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="renderer"></param>
		/// <param name="speckleStream"></param>
		/// <param name="objectIndex"></param>
		/// <param name="block"></param>
		public override void ApplyRuleToObject (Renderer renderer, SpeckleStream speckleStream, int objectIndex, MaterialPropertyBlock block)
		{
			revitProperty = "wallType";

			base.ApplyRuleToObject (renderer, speckleStream, objectIndex, block);
		}
	}
}