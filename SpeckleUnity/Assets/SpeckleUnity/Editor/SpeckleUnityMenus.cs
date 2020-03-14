using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SpeckleUnity.CustomEditors
{
	/// <summary>
	/// 
	/// </summary>
	public class SpeckleUnityMenus
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="command"></param>
		[MenuItem ("GameObject/SpeckleUnityManager", priority = 10)]
		public static void CreateManager (MenuCommand command)
		{
			GameObject instance = new GameObject ();
			instance.name = "SpeckleUnityManager";
			instance.AddComponent<SpeckleUnityManager> ();

			GameObject currentSelection = (GameObject)command.context;

			// Child the new object to the currently selected object if there is one
			if (currentSelection != null)
				instance.transform.parent = currentSelection.transform;

			// Allow undoing the object creation with ctrl + z
			Undo.RegisterCreatedObjectUndo (instance, "Created " + instance.name);

			// Select the new object for the user
			Selection.activeObject = instance;
		}
	}
}