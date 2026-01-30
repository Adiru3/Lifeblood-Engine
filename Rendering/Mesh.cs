using System;
using Lifeblood.Engine;
using System.Collections.Generic;

namespace Lifeblood.Rendering
{
    public class Mesh
    {
        public struct Vertex
        {
            public float X, Y, Z;
            public float NX, NY, NZ;
            public float U, V;

            public Vertex(float x, float y, float z, float u, float v)
            {
                X = x; Y = y; Z = z;
                NX = 0; NY = 1; NZ = 0;
                U = u; V = v;
            }
             public Vertex(float x, float y, float z, float nx, float ny, float nz, float u, float v)
            {
                X = x; Y = y; Z = z;
                NX = nx; NY = ny; NZ = nz;
                U = u; V = v;
            }
        }

        private Vertex[] vertices;
        public uint DrawMode = GL.GL_QUADS;

        public Mesh(List<Vertex> verts)
        {
            vertices = verts.ToArray();
        }

        public void Draw()
        {
            // Simple Immediate Mode
            GL.glBegin(DrawMode);
            
            foreach (var v in vertices)
            {
                // GL.glNormal3f(v.NX, v.NY, v.NZ); // Need to import glNormal3f
                GL.glTexCoord2f(v.U, v.V);
                GL.glVertex3f(v.X, v.Y, v.Z);
            }
            
            GL.glEnd();
        }

        public static Mesh CreateCube(float size)
        {
            float s = size / 2.0f;
            var verts = new List<Vertex>();
            
            // Front
            verts.Add(new Vertex(-s, -s, s, 0, 0, 1, 0, 1));
            verts.Add(new Vertex(s, -s, s, 0, 0, 1, 1, 1));
            verts.Add(new Vertex(s, s, s, 0, 0, 1, 1, 0));
            verts.Add(new Vertex(-s, s, s, 0, 0, 1, 0, 0));
            
            // Back
             verts.Add(new Vertex(s, -s, -s, 0, 0, -1, 0, 1));
            verts.Add(new Vertex(-s, -s, -s, 0, 0, -1, 1, 1));
            verts.Add(new Vertex(-s, s, -s, 0, 0, -1, 1, 0));
            verts.Add(new Vertex(s, s, -s, 0, 0, -1, 0, 0));
            
            // Left
            verts.Add(new Vertex(-s, -s, -s, -1, 0, 0, 0, 1));
            verts.Add(new Vertex(-s, -s, s, -1, 0, 0, 1, 1));
            verts.Add(new Vertex(-s, s, s, -1, 0, 0, 1, 0));
            verts.Add(new Vertex(-s, s, -s, -1, 0, 0, 0, 0));

            // Right
            verts.Add(new Vertex(s, -s, s, 1, 0, 0, 0, 1));
            verts.Add(new Vertex(s, -s, -s, 1, 0, 0, 1, 1));
            verts.Add(new Vertex(s, s, -s, 1, 0, 0, 1, 0));
            verts.Add(new Vertex(s, s, s, 1, 0, 0, 0, 0));
            
            // Top
            verts.Add(new Vertex(-s, s, s, 0, 1, 0, 0, 1));
            verts.Add(new Vertex(s, s, s, 0, 1, 0, 1, 1));
            verts.Add(new Vertex(s, s, -s, 0, 1, 0, 1, 0));
            verts.Add(new Vertex(-s, s, -s, 0, 1, 0, 0, 0));
            
            // Bottom
             verts.Add(new Vertex(-s, -s, -s, 0, -1, 0, 0, 1));
            verts.Add(new Vertex(s, -s, -s, 0, -1, 0, 1, 1));
            verts.Add(new Vertex(s, -s, s, 0, -1, 0, 1, 0));
            verts.Add(new Vertex(-s, -s, s, 0, -1, 0, 0, 0));

            return new Mesh(verts);
        }
        public static Mesh CreateSphere(float radius, int tessellation)
        {
            var verts = new List<Vertex>();
            int slices = tessellation;
            int stacks = tessellation;

            for (int i = 0; i < slices; i++)
            {
                for (int j = 0; j < stacks; j++)
                {
                    // Quads
                    float u1 = (float)i / slices;
                    float u2 = (float)(i + 1) / slices;
                    float v1 = (float)j / stacks;
                    float v2 = (float)(j + 1) / stacks;

                    float theta1 = u1 * 2.0f * (float)Math.PI;
                    float theta2 = u2 * 2.0f * (float)Math.PI;
                    float phi1 = v1 * (float)Math.PI;
                    float phi2 = v2 * (float)Math.PI;

                    verts.Add(GetSphereVertex(radius, theta1, phi1, u1, v1));
                    verts.Add(GetSphereVertex(radius, theta2, phi1, u2, v1));
                    verts.Add(GetSphereVertex(radius, theta2, phi2, u2, v2));
                    verts.Add(GetSphereVertex(radius, theta1, phi2, u1, v2));
                }
            }

            var mesh = new Mesh(verts);
            mesh.DrawMode = GL.GL_QUADS;
            return mesh;
        }

        private static Vertex GetSphereVertex(float r, float theta, float phi, float u, float v)
        {
            float x = r * (float)(Math.Sin(phi) * Math.Cos(theta));
            float y = r * (float)Math.Cos(phi);
            float z = r * (float)(Math.Sin(phi) * Math.Sin(theta));
            // Normal is just normalized position for sphere at origin
            float nx = x / r;
            float ny = y / r;
            float nz = z / r;
            return new Vertex(x, y, z, nx, ny, nz, u, v);
        }
    }
}
