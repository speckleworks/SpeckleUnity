using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpeckleCore;
using UnityNurbs;

public static class SpeckleToNurbs {

    //unity functions

        public static Vector3 ToVector3(this SpecklePoint p)
        {
            return new Vector3((float)p.Value[0], (float)p.Value[1], (float)p.Value[2]);
        }

        public static Vector3 ToVector3(this SpeckleVector p)
        {
            return new Vector3((float)p.Value[0], (float)p.Value[1], (float)p.Value[2]);
        }

    public static Vector3[] ToVector3Array(this List<double> array)
    {
        var vecs = new Vector3[array.Count / 3];

        for (int i = 0; i < vecs.Length; i++)
        {
            var u = i * 3;
            vecs[i] = new Vector3(u, u + 2, u + 1);
        }

        return vecs;
    }

    //verb functions

        public static Arc ToVerb(this SpeckleArc sArc)
        {
            var plane = sArc.Plane;

            return new Arc(plane.Origin.ToVector3(), plane.Xdir.ToVector3(), plane.Ydir.ToVector3(), (float)sArc.Radius, (float)sArc.StartAngle, (float)sArc.EndAngle);
        } 

    //needs plane definition
    
    //public static Circle ToNative(this SpeckleCircle sCirc)
    //{
    //    var normal = sCirc.Normal.ToVector3();
    //    var center = sCirc.Center.ToVector3();
    //
    //    //var yAxis = new Vec
    //
    //    var plane = new Plane(normal, center); 
    //
    //   return new Circle(sCirc.Center.ToVector3,plane.)
    //
    //}

    public static Ellipse ToVerb(this SpeckleEllipse sEllipse)
    {
        var plane = sEllipse.Plane;

        return new Ellipse(plane.Origin.ToVector3(), plane.Xdir.ToVector3(), plane.Ydir.ToVector3());
    }

    public static NurbsCurve ToVerb(this SpeckleCurve curve)
    {
        var degree = curve.Degree;
        var knots = curve.Knots.ToArray();

        for (int i = 0; i < knots.Length; i++)
        {
            knots[i] = Mathf.InverseLerp((float)curve.Domain.Start, (float)curve.Domain.End, (float)knots[i]); //yes this should probably be all doubles or all floats, shut up
        }

        var points = curve.Points.ToVector3Array();
        //var weights = curve.Weights.ToArray();

        var nurbs = new NurbsCurve(degree, knots, points);

        return nurbs;
    }






}
