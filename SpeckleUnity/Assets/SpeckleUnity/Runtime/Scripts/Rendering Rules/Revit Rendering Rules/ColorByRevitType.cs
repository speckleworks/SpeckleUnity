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
	public abstract class ColorByRevitType : RenderingRule
	{
		/// <summary>
		/// 
		/// </summary>
		public Color fallback = Color.grey;

		/// <summary>
		/// 
		/// </summary>
		public Gradient gradient;

		/// <summary>
		/// 
		/// </summary>
		public string colorName = "_Color";

		/// <summary>
		/// 
		/// </summary>
		public bool receiveShadows = false;

		/// <summary>
		/// 
		/// </summary>
		public ShadowCastingMode shadowCastingMode = ShadowCastingMode.Off;

		/// <summary>
		/// 
		/// </summary>
		public List<ColorKey> colorKey = new List<ColorKey> ();

		/// <summary>
		/// 
		/// </summary>
		protected Dictionary<string, Color> colorLookup = new Dictionary<string, Color> ();

		protected string revitProperty;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="renderer"></param>
		/// <param name="speckleStream"></param>
		/// <param name="objectIndex"></param>
		/// <param name="block"></param>
		public override void ApplyRuleToObject (Renderer renderer, SpeckleStream speckleStream, int objectIndex, MaterialPropertyBlock block)
		{
			Color colorToApply;

			if (colorLookup.Count == 0) colorKey.Clear ();

			if (speckleStream.Objects[objectIndex].Properties.TryGetValue (revitProperty, out object revitType))
			{
				string typeAsString = revitType.ToString ();
				if (!colorLookup.ContainsKey (typeAsString))
				{
					colorToApply = gradient.Evaluate (Random.Range (0f, 1f));

					colorLookup.Add (typeAsString, colorToApply);
					colorKey.Add (new ColorKey (typeAsString, colorToApply));
				}
				else
				{
					colorLookup.TryGetValue (typeAsString, out colorToApply);
				}
			}
			else
			{
				colorToApply = fallback;

				if (!colorLookup.ContainsKey ("No Value"))
				{
					colorLookup.Add ("No Value", colorToApply);
					colorKey.Add (new ColorKey ("No Value", colorToApply));
				}
			}
			

			block.SetColor (colorName, colorToApply);
			renderer.SetPropertyBlock (block);

			renderer.receiveShadows = receiveShadows;
			renderer.shadowCastingMode = shadowCastingMode;
		}
	}
}