using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpeckleCore;
using UnityNurbs;

public static class SpeckleToNurbs {

        public static Vector3 ToVector3(this SpecklePoint p)
        {
            return new Vector3((float)p.Value[0], (float)p.Value[1], (float)p.Value[2]);
        }

        public static Vector3 ToVector3(this SpeckleVector p)
        {
            return new Vector3((float)p.Value[0], (float)p.Value[1], (float)p.Value[2]);
        }

        public static Arc ToVerb(this SpeckleArc sArc)
        {
            var plane = sArc.Plane;

            return new Arc(plane.Origin.ToVector3(), plane.Xdir.ToVector3(), plane.Ydir.ToVector3(), (float)sArc.Radius, (float)sArc.StartAngle, (float)sArc.EndAngle);
        }
    



}
