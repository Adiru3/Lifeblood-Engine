using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Lifeblood.Engine;

namespace Lifeblood.Rendering
{
    public static class ModelLoader
    {
        public static Mesh LoadObj(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("Model not found: " + path);
                return Mesh.CreateCube(1.0f); // Fallback
            }

            List<Vector3> tempVertices = new List<Vector3>();
            // Vector2 unused
            
            List<float[]> tempTexCoords = new List<float[]>();
            List<Vector3> tempNormals = new List<Vector3>();

            List<Mesh.Vertex> resultVertices = new List<Mesh.Vertex>();

            string[] lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                if (line.StartsWith("v "))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                    float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                    tempVertices.Add(new Vector3(x, y, z));
                }
                else if (line.StartsWith("vt "))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    float u = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    float v = float.Parse(parts[2], CultureInfo.InvariantCulture);
                    tempTexCoords.Add(new float[] { u, v });
                }
                else if (line.StartsWith("vn "))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                    float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                    tempNormals.Add(new Vector3(x, y, z));
                }
                else if (line.StartsWith("f "))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    // Support Triangles and Quads
                    int count = parts.Length - 1;
                    for (int i = 0; i < count; i++) // 1, 2, 3 ... if quad 1,2,3 then 1,3,4? 
                    {
                        // Actually standard OBJ face handling for GL_TRIANGLES is:
                        // if Quad (v1, v2, v3, v4) -> Tri (v1, v2, v3), Tri (v1, v3, v4)
                        // But we are using GL_QUADS capability of Mesh? 
                        // Mesh.cs uses GL_QUADS loop. If outputting triangles, Mesh.cs needs update or we store as tris and change draw mode.
                        
                        // For simplicity, let's assume Triangles in OBJ (exported as Triangulate).
                        // Parsing "v/vt/vn"
                        // parts[i+1]
                    }
                    
                    // Simple Triangulation
                    // Fan triangulation for convex polygons (v1, v2, v3), (v1, v3, v4), ...
                    for (int i = 0; i < count - 2; i++)
                    {
                        ParseFaceVertex(parts[1], tempVertices, tempTexCoords, tempNormals, resultVertices);
                        ParseFaceVertex(parts[2 + i], tempVertices, tempTexCoords, tempNormals, resultVertices);
                        ParseFaceVertex(parts[3 + i], tempVertices, tempTexCoords, tempNormals, resultVertices);
                    }
                }
            }
            
            // Note: If we use GL_QUADS in Mesh.Draw(), we must ensure we feed groups of 4.
            // But here we triangulated. So Mesh.Draw() should probably use GL_TRIANGLES.
            // I should update Mesh.Draw() to generic drawing or GL_TRIANGLES.
            
            var mesh = new Mesh(resultVertices);
            mesh.DrawMode = GL.GL_TRIANGLES;
            return mesh;
        }

        private static void ParseFaceVertex(string part, List<Vector3> pos, List<float[]> uvs, List<Vector3> norms, List<Mesh.Vertex> outVerts)
        {
            var idx = part.Split('/');
            
            // OBJ indices are 1-based
            int vIdx = int.Parse(idx[0]) - 1;
            int vtIdx = idx.Length > 1 && idx[1] != "" ? int.Parse(idx[1]) - 1 : 0;
            int vnIdx = idx.Length > 2 ? int.Parse(idx[2]) - 1 : 0;

            Vector3 p = pos[vIdx];
            float u = 0, v = 0;
            if (uvs.Count > vtIdx && vtIdx >= 0) { u = uvs[vtIdx][0]; v = uvs[vtIdx][1]; }
            
            Vector3 n = Vector3.Zero;
            if (norms.Count > vnIdx && vnIdx >= 0) n = norms[vnIdx];

            outVerts.Add(new Mesh.Vertex(p.X, p.Y, p.Z, n.X, n.Y, n.Z, u, v));
        }
    }
}
