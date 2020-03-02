using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Wrapper objects to add meta data and functionality to gameobjects to work with speckle


//TODO
//Should these live in a speckle kit along with the speckle unity converter?

namespace SpeckleUnity
{
	public delegate void SpeckleUnityValueChange (object source);


	/// <summary>
	/// Base class for all native SpeckleUnity objects 
	/// </summary>
	public class SpeckleUnityObject
	{
		//onchanged event for senders to implement to signal a sending update
		public event SpeckleUnityValueChange ValueChanged;
		public void OnValueChanged ()
		{
			ValueChanged?.Invoke (this);
		}
	}


	/// <summary>
	/// Any SpeckleObject that needs to be displayed with a game object should inherit from this
	/// </summary>
	public class SpeckleUnityGeometry : SpeckleUnityObject
	{
		//Display object
		public GameObject gameObject;

		public SpeckleUnityGeometry ()
		{
			gameObject = new GameObject ();

		}

		public SpeckleUnityGeometry (GameObject go)
		{
			this.gameObject = go;
		}
	}

	/// <summary>
	/// Transform
	/// </summary>
	//Currently converting to a point to send to speckle
	//TODO - write Unity kit to implement SpeckleTransform along with converters for Rhino/GH/Dynamo/Etc?
	public class SpeckleUnityTransform : SpeckleUnityGeometry
	{
		public SpeckleUnityTransform (GameObject go) : base (go) { }
	}


	/// <summary>
	/// General mesh
	/// </summary>
	public class SpeckleUnityMesh : SpeckleUnityGeometry
	{
		public MeshRenderer meshRenderer;
		
		public SpeckleUnityMesh (Vector3[] verts, int[] tris) : base ()
		{
			meshRenderer = gameObject.AddComponent<MeshRenderer> ();
			Mesh mesh = gameObject.AddComponent<MeshFilter> ().mesh;

			mesh.vertices = verts;
			mesh.triangles = tris;
			mesh.RecalculateNormals ();
			mesh.RecalculateTangents ();

			//Add mesh collider
			MeshCollider mc = gameObject.AddComponent<MeshCollider> ();
			mc.sharedMesh = mesh;
		}
	}

	/// <summary>
	/// Polyline
	/// Can be used to display lines, curves, or polylines
	/// </summary>
	public class SpeckleUnityPolyline : SpeckleUnityGeometry
	{
		public SpeckleUnityPolyline (Vector3[] points) : base ()
		{
			//create line renderer       
			LineRenderer lr = gameObject.AddComponent<LineRenderer> ();
			lr.positionCount = points.Length;
			lr.SetPositions (points);
			lr.numCapVertices = 1;
			lr.startWidth = 0.01f;
			lr.endWidth = 0.01f;
		}
	}


	/// <summary>
	/// Display Point
	/// Uses a line renderer for display
	/// </summary>
	public class SpeckleUnityPoint : SpeckleUnityGeometry
	{
		public Vector3 point;

		public SpeckleUnityPoint (Vector3 point) : base ()
		{
			this.point = point;

			//create line renderer       
			LineRenderer lr = gameObject.AddComponent<LineRenderer> ();
			lr.SetPositions (new Vector3[2] { point, point });
			lr.numCapVertices = 1;
			lr.startWidth = 0.01f;
			lr.endWidth = 0.01f;
		}

	}

	/// <summary>
	/// Contains a float, no game object to display
	/// </summary>
	public class SpeckleUnityNumber : SpeckleUnityObject
	{

		public float value;

		public SpeckleUnityNumber (float value)
		{
			this.value = value;
		}
	}

}
