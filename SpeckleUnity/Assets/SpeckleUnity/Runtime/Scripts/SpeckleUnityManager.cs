using System;
using System.Collections.Generic;
using UnityEngine;
using SpeckleCore;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SpeckleUnity
{
	/// <summary>
	/// 
	/// </summary>
	public class SpeckleUnityManager : MonoBehaviour, ISpeckleInitializer
	{
		/// <summary>
		/// 
		/// </summary>
		[Tooltip ("URL for the Speckle Server you want to use. eg: https://hestia.speckle.works/api/v1")]
		[SerializeField] protected string ServerURL; 

		/// <summary>
		/// 
		/// </summary>
		protected BinaryFormatter formatter;

		/// <summary>
		/// 
		/// </summary>
		protected const string DATA_FILENAME = "TestSaveClient.dat";

		/// <summary>
		/// 
		/// </summary>
		protected Dictionary<string, SpeckleApiClient> ClientSaveDictionary = new Dictionary<string, SpeckleApiClient> ();

		/// <summary>
		/// 
		/// </summary>
		protected virtual void Start ()
		{
			SpeckleInitializer.Initialize ();
			LocalContext.Init ();
			formatter = new BinaryFormatter ();

			LoadClients ();
			SpeckleUnitySender[] SenderClients = FindObjectsOfType<SpeckleUnitySender> ();
			foreach (var c in SenderClients)
			{
				if (ClientSaveDictionary.ContainsKey (c.keyForSaving) && c.persistent)
					c.CompleteDeserialization (ClientSaveDictionary[c.keyForSaving]);
				else
					c.InitializeClient (ServerURL);
			}

            SpeckleUnityReceiver[] ReceiverClients = FindObjectsOfType<SpeckleUnityReceiver> ();
            foreach (var c in ReceiverClients)
            {
                c.InitializeClient (ServerURL);
            }
        }

		/// <summary>
		/// 
		/// </summary>
		protected virtual void OnApplicationQuit ()
		{
			Debug.Log ("Application ending after " + Time.time + " seconds");
			SaveClients ();
		}


		/// <summary>
		/// 
		/// </summary>
		protected virtual void SaveClients ()
		{
			//check for persistent clients
			SpeckleUnitySender[] Clients = FindObjectsOfType<SpeckleUnitySender> ();
			foreach (var c in Clients)
			{
				if (c.persistent)
					ClientSaveDictionary[c.keyForSaving] = c.client;
				else
				{
					ClientSaveDictionary.Remove (c.keyForSaving);
					c.DisposeClient ();
				}
			}
			//TODO - dispose of clients in save file that no longer exist

			//dictionaries aren't serializable?
			List<ClientSaveObject> ClientSaveList = new List<ClientSaveObject> ();

			foreach (var kvp in ClientSaveDictionary)
			{
				ClientSaveObject c = new ClientSaveObject
				{
					client = kvp.Value,
					key = kvp.Key
				};
				ClientSaveList.Add (c);
			}

			try
			{
				// Create a FileStream that will write data to file.
				FileStream writerFileStream =
					new FileStream (DATA_FILENAME, FileMode.Create, FileAccess.Write);
				// Save information
				this.formatter.Serialize (writerFileStream, ClientSaveList);

				// Close the writerFileStream when we are done.
				writerFileStream.Close ();
			}
			catch (Exception)
			{
				Debug.Log ("Unable to save");
			} // end try-catch
		}

		/// <summary>
		/// 
		/// </summary>
		protected virtual void LoadClients ()
		{
			// Check if we had previously Save information 
			if (File.Exists (DATA_FILENAME))
			{
				try
				{
					// Create a FileStream will gain read access to the data file
					FileStream readerFileStream = new FileStream (DATA_FILENAME, FileMode.Open, FileAccess.Read);
					// Reconstruct information
					List<ClientSaveObject> ClientSaveList = (List<ClientSaveObject>)this.formatter.Deserialize (readerFileStream);

					// Close the readerFileStream when we are done
					readerFileStream.Close ();

					//rebuild dictionary for searching later
					foreach (var c in ClientSaveList)
					{
						ClientSaveDictionary[c.key] = c.client;
					}

				}
				catch (Exception e)
				{
					Debug.Log ("Could not read file: " + e.ToString ());
				} // end try-catch

			} // end if

		}

	}
}
