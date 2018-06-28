using System; 
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;

using SpeckleCore;


public class ConverterHack { /*makes sure the assembly is loaded*/  public ConverterHack() { } }

public static class SpeckleUnityConverter
{
      
    #region convenience methods

        public static Vector3 ToPoint(double x, double y, double z)
        {
            // switch y and z
            return new Vector3((float)x, (float)z, (float)y);
        }

        public static Vector3[] ToPoints(this IEnumerable<double> arr)
        {
            if (arr.Count() % 3 != 0) throw new Exception("Array malformed: length%3 != 0.");

            Vector3[] points = new Vector3[arr.Count() / 3];
            var asArray = arr.ToArray();
            for (int i = 2, k = 0; i < arr.Count(); i += 3)
                points[k++] = ToPoint(asArray[i - 2], asArray[i - 1], asArray[i]);
            
            return points;
        }
        
        private static Mesh CreatePrimitiveMesh(PrimitiveType type)
        {
            GameObject gameObject = GameObject.CreatePrimitive(type);
            Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            GameObject.Destroy(gameObject);

            //PrimitiveHelper.primitiveMeshes[type] = mesh;
            return mesh;
        }
    
    private static GameObject BaseObejct()
    {
        GameObject prefab = GameObject.Find("SpeckleManager").GetComponent<UnitySpeckle>().prefab;
        GameObject go = GameObject.Instantiate(prefab);
        return go;        
    }

    private static GameObject BaseLineObejct()
    {
        GameObject prefab = GameObject.Find("SpeckleManager").GetComponent<UnitySpeckle>().LinePrefab;
        GameObject go = GameObject.Instantiate(prefab);
        return go;
    }

    #endregion

    #region points


    public static GameObject ToNative(this SpecklePoint pt)
    {        
        GameObject go = BaseObejct();
        
        go.GetComponent<UnitySpeckleObjectData>().Id = pt._id;
        //go.GetComponent<UnitySpeckleObjectData>().LayerName = (string)pt.Properties["LayerName"];

        go.GetComponent<MeshFilter>().mesh = CreatePrimitiveMesh(PrimitiveType.Sphere);
        go.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        
        go.transform.position = ToPoint(pt.Value[0], pt.Value[1], pt.Value[2]);
        return go;

    }

    #endregion

    #region polyline

    public static GameObject ToNative(this SpecklePolyline pl)
    {
        GameObject go = BaseLineObejct();
        go.GetComponent<UnitySpeckleObjectData>().Id = pl._id;

        Vector3[] pts = pl.Value.ToPoints();
        go.GetComponent<LineRenderer>().positionCount = pts.Count();
        go.GetComponent<LineRenderer>().SetPositions(pts);     
       
        return go;
    }


    #endregion


    #region mesh

    public static GameObject ToNative(this SpeckleMesh mesh)
        {
            GameObject go = BaseObejct();
            
            go.GetComponent<UnitySpeckleObjectData>().Id = mesh._id;
            //go.GetComponent<UnitySpeckleObjectData>().LayerName = (string)mesh.Properties["LayerName"];

        var newMesh = go.GetComponent<MeshFilter>().mesh;

            var vertices = mesh.Vertices.ToPoints();
            newMesh.vertices = vertices;

            List<int> tris = new List<int>();

            int i = 0;
            while (i < mesh.Faces.Count)
            {
                if (mesh.Faces[i] == 0)
                {
                    //Triangle
                    tris.Add(mesh.Faces[i+1]);
                    tris.Add(mesh.Faces[i+3]);
                    tris.Add(mesh.Faces[i+2]);
                    i += 4;
                } else {
                    //Quads
                    tris.Add(mesh.Faces[i + 1]);
                    tris.Add(mesh.Faces[i + 3]);
                    tris.Add(mesh.Faces[i + 2]);

                    tris.Add(mesh.Faces[i + 3]);
                    tris.Add(mesh.Faces[i + 1]);
                    tris.Add(mesh.Faces[i + 4]);

                    i += 5;
            }
        }
        newMesh.triangles = tris.ToArray();
        newMesh.RecalculateNormals();
        newMesh.RecalculateTangents();

        return go;
        
    }

    #endregion
}
