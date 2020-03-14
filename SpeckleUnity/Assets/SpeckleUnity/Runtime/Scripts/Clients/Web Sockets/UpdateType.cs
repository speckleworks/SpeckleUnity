using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpeckleUnity
{
    /// <summary>
    /// The type of update that a receiver was notified of for a given stream.
    /// </summary>
    public enum UpdateType
    {
        Global,
        Meta,
        Name,
        Object,
        Children
    }
}