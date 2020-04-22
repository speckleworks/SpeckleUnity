using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpeckleUnity
{
    /// <summary>
    /// A container of data describing what stream just got updated, what <c>Transform</c> it's under
	/// and what type of update it had so that user code can respond accordingly.
    /// </summary>
    [Serializable]
    public class SpeckleUnityUpdate
    {
        /// <summary>
        /// The ID of the stream that got updated.
        /// </summary>
        public string streamID;

        /// <summary>
        /// The root <c>Transform</c> of the stream that got updated.
        /// </summary>
        public Transform streamRoot;

        /// <summary>
        /// The type of update made to the stream.
        /// </summary>
        public UpdateType updateType;

        /// <summary>
        /// A value from 0-1 describing how far along the update is. Can be used to represent a percentage;
        /// </summary>
        public float updateProgress;

        /// <summary>
        /// Constructs a new instance of this object with all its values assigned.
        /// </summary>
        /// <param name="streamID">The ID of the stream that got updated.</param>
        /// <param name="streamRoot">The root <c>Transform</c> of the stream that got updated.</param>
        /// <param name="updateType">The type of update made to the stream.</param>
        /// <param name="updateProgress">The current progress of the ongoing update.</param>
        public SpeckleUnityUpdate (string streamID, Transform streamRoot, UpdateType updateType, float updateProgress)
        {
            this.streamID = streamID;
            this.streamRoot = streamRoot;
            this.updateType = updateType;
            this.updateProgress = updateProgress;
        }
    }
}