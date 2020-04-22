using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Wrapper objects to add meta data and functionality to gameobjects to work with speckle

namespace SpeckleUnity
{
	/// <summary>
	/// A delegate which is invoked whenever a stream is updated. Intended for use with the 
	/// <c>SpeckleUnitySender</c> which will eventually be supported.
	/// </summary>
	/// <param name="source"></param>
	public delegate void SpeckleUnityValueChange (object source);


	/// <summary>
	/// Base class for all native SpeckleUnity objects 
	/// </summary>
	public class SpeckleUnityObject
	{
		//onchanged event for senders to implement to signal a sending update
		/// <summary>
		/// 
		/// </summary>
		public event SpeckleUnityValueChange ValueChanged;

		/// <summary>
		/// 
		/// </summary>
		public void OnValueChanged ()
		{
			ValueChanged?.Invoke (this);
		}
	}

	/// <summary>
	/// Base definition for all rendered stream objects. Any Speckle object that needs to be
	/// displayed with a game object should inherit from this class.
	/// </summary>
	public abstract class SpeckleUnityGeometry : SpeckleUnityObject
	{
		/// <summary>
		/// The gameobject that will be displayed. The <c>Transform</c> parent will be assigned
		/// by the <c>SpeckleUnityReceiver</c>.
		/// </summary>
		public GameObject gameObject;

		/// <summary>
		/// 
		/// </summary>
		public Renderer renderer;

		/// <summary>
		/// 
		/// </summary>
		public SpeckleUnityGeometry ()
		{
			gameObject = new GameObject ();

		}
	}

	/// <summary>
	/// A stream object represented as a gameobject with a <c>MeshRenderer</c>. Also adds a 
	/// <c>MeshCollider</c> to the object. The material is assigned by the <c>SpeckleUnityReceiver</c>.
	/// </summary>
	public class SpeckleUnityMesh : SpeckleUnityGeometry
	{
		/// <summary>
		/// 
		/// </summary>
		public MeshRenderer meshRenderer;
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="verts"></param>
		/// <param name="tris"></param>
		public SpeckleUnityMesh (string type, Vector3[] verts, int[] tris) : base ()
		{
			gameObject.name = type;

			renderer = meshRenderer = gameObject.AddComponent<MeshRenderer> ();
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
	/// Used to display lines, curves, or polylines as a game object with a <c>LineRenderer</c>.
	/// The material is assigned by the <c>SpeckleUnityReceiver</c>.
	/// </summary>
	public class SpeckleUnityPolyline : SpeckleUnityGeometry
	{
		/// <summary>
		/// 
		/// </summary>
		public LineRenderer lineRenderer;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="points"></param>
		public SpeckleUnityPolyline (string type, Vector3[] points) : base ()
		{
			gameObject.name = type;

			//create line renderer       
			renderer = lineRenderer = gameObject.AddComponent<LineRenderer> ();
			lineRenderer.positionCount = points.Length;
			lineRenderer.SetPositions (points);
			lineRenderer.numCapVertices = 1;
			lineRenderer.startWidth = 1;
			lineRenderer.endWidth = 1;
		}
	}


	/// <summary>
	/// Display Point. Uses a line renderer for display. The material is assigned by the
	/// <c>SpeckleUnityReceiver</c>.
	/// </summary>
	public class SpeckleUnityPoint : SpeckleUnityGeometry
	{
		/// <summary>
		/// 
		/// </summary>
		public Vector3 point;

		/// <summary>
		/// 
		/// </summary>
		public LineRenderer lineRenderer;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="point"></param>
		public SpeckleUnityPoint (string type, Vector3 point) : base ()
		{
			gameObject.name = type;

			this.point = point;

			//create line renderer       
			renderer = lineRenderer = gameObject.AddComponent<LineRenderer> ();
			lineRenderer.SetPositions (new Vector3[2] { point, point });
			lineRenderer.numCapVertices = 1;
			lineRenderer.startWidth = 1;
			lineRenderer.endWidth = 1;
		}
	}

	/// <summary>
	/// Contains a float, no game object to display
	/// </summary>
	public class SpeckleUnityNumber : SpeckleUnityObject
	{
		/// <summary>
		/// 
		/// </summary>
		public float value;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public SpeckleUnityNumber (float value)
		{
			this.value = value;
		}
	}
}