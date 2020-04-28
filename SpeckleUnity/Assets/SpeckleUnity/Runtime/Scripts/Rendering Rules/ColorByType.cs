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
	[CreateAssetMenu (menuName = "SpeckleUnity/Rendering Rule: Color By Type")]
	public class ColorByType : RenderingRule
	{
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="renderer"></param>
		/// <param name="speckleObject"></param>
		/// <param name="block"></param>
		public override void ApplyRuleToObject (Renderer renderer, SpeckleObject speckleObject, MaterialPropertyBlock block)
		{
			Color colorToApply;

			if (colorLookup.Count == 0) colorKey.Clear ();

			if (!colorLookup.ContainsKey (speckleObject.Type))
			{
				colorToApply = gradient.Evaluate (Random.Range (0f, 1f));

				colorLookup.Add (speckleObject.Type, colorToApply);
				colorKey.Add (new ColorKey (speckleObject.Type, colorToApply));
			}
			else
			{
				colorLookup.TryGetValue (speckleObject.Type, out colorToApply);
			}

			block.SetColor (colorName, colorToApply);
			renderer.SetPropertyBlock (block);

			renderer.receiveShadows = receiveShadows;
			renderer.shadowCastingMode = shadowCastingMode;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	[System.Serializable]
	public class ColorKey
	{
		/// <summary>
		/// 
		/// </summary>
		public string name;

		/// <summary>
		/// 
		/// </summary>
		public Color color;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="n"></param>
		/// <param name="c"></param>
		public ColorKey (string n, Color c)
		{
			name = n;
			color = c;
		}
	}
}