using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SpeckleCore;
using SpeckleCoreGeometryClasses;

//TODO
//Should this live in a speckle kit along with the speckle unity classes?

namespace SpeckleUnity
{
	/// <summary>
	/// Contains all extension method definitions for converting Speckle stream object types into
	/// native Unity types and vice versa. 
	/// </summary>
	public static partial class Conversions
	{
		#region convenience methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public static Vector3 ToPoint (double x, double y, double z)
		{
			// switch y and z
			return new Vector3 ((float)x, (float)z, (float)y);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ptValues"></param>
		/// <returns></returns>
		public static Vector3 ToPoint (double[] ptValues)
		{
			double x = ptValues[0];
			double y = ptValues[1];
			double z = ptValues[2];
			// switch y and z
			return new Vector3 ((float)x, (float)z, (float)y);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="arr"></param>
		/// <returns></returns>
		public static Vector3[] ToPoints (this IEnumerable<double> arr)
		{
			if (arr.Count () % 3 != 0) throw new Exception ("Array malformed: length%3 != 0.");

			Vector3[] points = new Vector3[arr.Count () / 3];
			var asArray = arr.ToArray ();
			for (int i = 2, k = 0; i < arr.Count (); i += 3)
				points[k++] = ToPoint (asArray[i - 2], asArray[i - 1], asArray[i]);

			return points;
		}
		#endregion

		/// <summary>
		/// The scale factor for each geometry object to be spawned in to.
		/// </summary>
		public static double scaleFactor = 1;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static SpeckleNumber ToSpeckle (this SpeckleUnityNumber obj)
		{
			var result = new SpeckleNumber (obj.value);
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public static SpeckleUnityNumber ToNative (this SpeckleNumber number)
		{
			var result = new SpeckleUnityNumber ((float)number.Value);
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string ToNative (this SpeckleString str)
		{
			var result = str.Value;
			return result;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		//Currently just sending the position of a transform
		//TODO - write speckle Kit with new SpeckleTransform
		public static SpecklePoint ToSpeckle (this SpeckleUnityTransform obj)
		{
			Vector3 p = obj.gameObject.transform.position;

			//switch y and z
			var result = new SpecklePoint (p.x, p.z, p.y);
			return result;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static SpecklePoint ToSpeckle (this SpeckleUnityPoint obj)
		{
			Vector3 p = obj.point;

			//switch y and z
			return new SpecklePoint (p.x, p.z, p.y);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public static SpeckleUnityPoint ToNative (this SpecklePoint point)
		{
			point.Scale (scaleFactor);

			Vector3 newPt = ToPoint (point.Value.ToArray ());
			return new SpeckleUnityPoint (point.Type, newPt);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		public static SpeckleUnityPolyline ToNative (this SpeckleLine line)
		{
			line.Scale (scaleFactor);

			return new SpeckleUnityPolyline (line.Type, line.Value.ToPoints ());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="polyline"></param>
		/// <returns></returns>
		public static SpeckleUnityPolyline ToNative (this SpecklePolyline polyline)
		{
			polyline.Scale (scaleFactor);

			return new SpeckleUnityPolyline (polyline.Type, polyline.Value.ToPoints ());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="curve"></param>
		/// <returns></returns>
		public static SpeckleUnityPolyline ToNative (this SpeckleCurve curve)
		{
			curve.Scale (scaleFactor);

			return new SpeckleUnityPolyline (curve.Type, curve.DisplayValue.Value.ToPoints ());
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="speckleMesh"></param>
		/// <returns></returns>
		public static SpeckleUnityMesh ToNative (this SpeckleMesh speckleMesh)
		{
			speckleMesh.Scale (scaleFactor);

			//convert speckleMesh.Faces into triangle array           
			List<int> tris = new List<int> ();
			int i = 0;
			while (i < speckleMesh.Faces.Count)
			{
				if (speckleMesh.Faces[i] == 0)
				{
					//Triangles
					tris.Add (speckleMesh.Faces[i + 1]);
					tris.Add (speckleMesh.Faces[i + 3]);
					tris.Add (speckleMesh.Faces[i + 2]);
					i += 4;
				}
				else
				{
					//Quads to triangles
					tris.Add (speckleMesh.Faces[i + 1]);
					tris.Add (speckleMesh.Faces[i + 3]);
					tris.Add (speckleMesh.Faces[i + 2]);

					tris.Add (speckleMesh.Faces[i + 3]);
					tris.Add (speckleMesh.Faces[i + 1]);
					tris.Add (speckleMesh.Faces[i + 4]);

					i += 5;
				}
			}

			SpeckleUnityMesh result = new SpeckleUnityMesh (speckleMesh.Type, speckleMesh.Vertices.ToPoints (), tris.ToArray ());

			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="brep"></param>
		/// <returns></returns>
		public static SpeckleUnityMesh ToNative (this SpeckleBrep brep)
		{
			SpeckleMesh speckleMesh = brep.DisplayValue;
			return speckleMesh.ToNative ();
		}
	}
}
