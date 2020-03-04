using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpeckleUnity
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class SpeckleUnityUpdate
    {
        /// <summary>
        /// 
        /// </summary>
        public string streamID;

        /// <summary>
        /// 
        /// </summary>
        public Transform streamRoot;

        /// <summary>
        /// 
        /// </summary>
        public UpdateType updateType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="streamID"></param>
        /// <param name="streamRoot"></param>
        /// <param name="updateType"></param>
        public SpeckleUnityUpdate (string streamID, Transform streamRoot, UpdateType updateType)
        {
            this.streamID = streamID;
            this.streamRoot = streamRoot;
            this.updateType = updateType;
        }
    }
}