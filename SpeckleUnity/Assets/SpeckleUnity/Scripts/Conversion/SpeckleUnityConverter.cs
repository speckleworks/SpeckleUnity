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
	public class Initialiser : ISpeckleInitializer
	{
		public Initialiser () { }
	}


	public static partial class Conversions
	{
		#region convenience methods
		public static Vector3 ToPoint (double x, double y, double z)
		{
			// switch y and z
			return new Vector3 ((float)x, (float)z, (float)y);
		}
		public static Vector3 ToPoint (double[] ptValues)
		{
			double x = ptValues[0];
			double y = ptValues[1];
			double z = ptValues[2];
			// switch y and z
			return new Vector3 ((float)x, (float)z, (float)y);
		}

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


		public static SpeckleNumber ToSpeckle (this SpeckleUnityNumber obj)
		{
			var result = new SpeckleNumber (obj.value);
			return result;
		}
		public static SpeckleUnityNumber ToNative (this SpeckleNumber number)
		{
			var result = new SpeckleUnityNumber ((float)number.Value);
			return result;
		}

		public static string ToNative (this SpeckleString str)
		{
			var result = str.Value;
			return result;
		}


		//Currently just sending the position of a transform
		//TODO - write speckle Kit with new SpeckleTransform
		public static SpecklePoint ToSpeckle (this SpeckleUnityTransform obj)
		{
			Vector3 p = obj.go.transform.position;

			//switch y and z
			var result = new SpecklePoint (p.x, p.z, p.y);
			return result;
		}




		public static SpecklePoint ToSpeckle (this SpeckleUnityPoint obj)
		{
			Vector3 p = obj.point;

			//switch y and z
			var result = new SpecklePoint (p.x, p.z, p.y);
			return result;
		}
		public static SpeckleUnityPoint ToNative (this SpecklePoint point)
		{
			Vector3 newPt = ToPoint (point.Value.ToArray ());
			SpeckleUnityPoint result = new SpeckleUnityPoint (newPt);
			return result;
		}


		public static SpeckleUnityPolyline ToNative (this SpeckleLine line)
		{
			var result = new SpeckleUnityPolyline (line.Value.ToPoints ());
			return result;
		}
		public static SpeckleUnityPolyline ToNative (this SpecklePolyline polyline)
		{
			var result = new SpeckleUnityPolyline (polyline.Value.ToPoints ());
			return result;
		}
		public static SpeckleUnityPolyline ToNative (this SpeckleCurve curve)
		{
			var result = new SpeckleUnityPolyline (curve.DisplayValue.Value.ToPoints ());
			return result;
		}



		public static SpeckleUnityMesh ToNative (this SpeckleMesh speckleMesh)
		{
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
			SpeckleUnityMesh result = new SpeckleUnityMesh (speckleMesh.Vertices.ToPoints (), tris.ToArray ());
			return result;
		}

		public static SpeckleUnityMesh ToNative (this SpeckleBrep brep)
		{
			var speckleMesh = brep.DisplayValue;
			return speckleMesh.ToNative ();
		}
	}
}
