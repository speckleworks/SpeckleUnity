using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpeckleCore;
using UnityNurbs;

internal static class SpeckleToNurbs {

    //unity functions

    public static Vector3 ToVector3(this SpecklePoint p)
    {
        return new Vector3((float)p.Value[0], (float)p.Value[2], (float)p.Value[1]);
    }

    public static Vector3 ToVector3(this SpeckleVector p)
    {
        return new Vector3((float)p.Value[0], (float)p.Value[2], (float)p.Value[1]);
    }

    public static Vector3[] ToVector3Array(this List<double> array)
    {
        var vecs = new Vector3[array.Count / 3];

        for (int i = 0; i < vecs.Length; i++)
        {
            var u = i * 3;
            vecs[i] = new Vector3((float)array[u], (float)array[u + 2], (float)array[u + 1]);
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

    public static Line ToVerb(this SpeckleLine sLine)
    {
        var points = sLine.Value.ToVector3Array();
        return new Line(points[0], points[1]);
    }

    public static NurbsCurve ToVerb(this SpeckleCurve curve)
    {
        var degree = curve.Degree;
        var knotsList = curve.Knots;//.ToArray();  
        var points = curve.Points.ToVector3Array();

        if (knotsList.Count != degree + points.Length + 1)
        {
            knotsList.Insert(0, knotsList[0]);
            knotsList.Add(knotsList[knotsList.Count-1]);
        }

        var knots = knotsList.ToArray();
        var weights = curve.Weights.ToArray();

        var nurbs = new NurbsCurve(degree, knots, points, weights);

        return nurbs;        
    }
}
