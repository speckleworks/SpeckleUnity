using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpeckleUnity
{
    /// <summary>
    /// 
    /// </summary>
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

