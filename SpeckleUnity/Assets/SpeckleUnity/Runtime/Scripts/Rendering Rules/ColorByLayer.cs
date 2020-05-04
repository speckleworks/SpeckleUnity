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
	[CreateAssetMenu (menuName = "SpeckleUnity/Rendering Rule: Color By Layer")]
	public class ColorByLayer : RenderingRule
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
		/// <param name="speckleStream"></param>
		/// <param name="objectIndex"></param>
		/// <param name="block"></param>
		public override void ApplyRuleToObject (Renderer renderer, SpeckleStream speckleStream, int objectIndex, MaterialPropertyBlock block)
		{
			Color colorToApply;

			if (colorLookup.Count == 0) colorKey.Clear ();

			List<Layer> layers = speckleStream.Layers;

			for (int i = 0; i < layers.Count; i++)
			{
				if (!colorLookup.ContainsKey (layers[i].Name))
				{
					colorToApply = gradient.Evaluate (Random.Range (0f, 1f));

					colorLookup.Add (layers[i].Name, colorToApply);
					colorKey.Add (new ColorKey (layers[i].Name, colorToApply));
				}

				if (objectIndex >= layers[i].StartIndex && objectIndex < (layers[i].StartIndex + layers[i].ObjectCount))
				{
					colorLookup.TryGetValue (layers[i].Name, out colorToApply);

					block.SetColor (colorName, colorToApply);
					renderer.SetPropertyBlock (block);

					renderer.receiveShadows = receiveShadows;
					renderer.shadowCastingMode = shadowCastingMode;
				}
			}
		}
	}
}