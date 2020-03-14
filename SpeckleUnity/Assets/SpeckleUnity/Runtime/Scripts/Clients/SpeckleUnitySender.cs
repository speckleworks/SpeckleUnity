using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using SpeckleCore;
using System.Threading.Tasks;

namespace SpeckleUnity
{
    /// <summary>
	/// A <c>SpeckleUnityClient</c> specialised in sending streams and updating the server to reflect
	/// any updates made to the scene. Made serializable so that it would render in the inspector along
	/// with its exposed fields.
	/// </summary>
	[Serializable]
    public class SpeckleUnitySender : SpeckleUnityClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="url"></param>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public override IEnumerator InitializeClient (SpeckleUnityManager controller, string url, string authToken)
        {
            throw new System.NotImplementedException ();
        }
    }
}

