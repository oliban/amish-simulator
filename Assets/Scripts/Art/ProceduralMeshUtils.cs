using UnityEngine;
using System.Collections.Generic;

namespace AmishSimulator
{
    public static class ProceduralMeshUtils
    {
        /// <summary>Create a cylinder mesh along Y axis.</summary>
        public static Mesh CreateCylinder(float radius, float height, int segments = 12)
        {
            var mesh = new Mesh();
            var verts  = new List<Vector3>();
            var norms  = new List<Vector3>();
            var uvs    = new List<Vector2>();
            var tris   = new List<int>();

            // Top cap center
            int topCenter = 0;
            verts.Add(new Vector3(0, height, 0));
            norms.Add(Vector3.up);
            uvs.Add(new Vector2(0.5f, 0.5f));

            // Bottom cap center
            int botCenter = 1;
            verts.Add(new Vector3(0, 0, 0));
            norms.Add(Vector3.down);
            uvs.Add(new Vector2(0.5f, 0.5f));

            // Ring verts (top + bottom, shared for side normals)
            int ringStart = 2;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                Vector3 n = new Vector3(x, 0, z).normalized;

                verts.Add(new Vector3(x, height, z)); // top ring
                norms.Add(n);
                uvs.Add(new Vector2((float)i / segments, 1));

                verts.Add(new Vector3(x, 0, z)); // bottom ring
                norms.Add(n);
                uvs.Add(new Vector2((float)i / segments, 0));
            }

            // Side triangles
            for (int i = 0; i < segments; i++)
            {
                int a = ringStart + i * 2;
                int b = ringStart + i * 2 + 1;
                int c = ringStart + (i + 1) * 2;
                int d = ringStart + (i + 1) * 2 + 1;
                tris.AddRange(new[] { a, c, b, b, c, d });
            }

            // Cap verts (separate so normals are flat)
            int capStart = verts.Count;
            for (int i = 0; i < segments; i++)
            {
                float a0 = i       * Mathf.PI * 2f / segments;
                float a1 = (i + 1) * Mathf.PI * 2f / segments;
                // Top cap
                tris.Add(topCenter);
                tris.Add(capStart + i * 2);
                tris.Add(capStart + i * 2 + 1);
                verts.Add(new Vector3(Mathf.Cos(a0) * radius, height, Mathf.Sin(a0) * radius));
                norms.Add(Vector3.up);
                uvs.Add(Vector2.zero);
                verts.Add(new Vector3(Mathf.Cos(a1) * radius, height, Mathf.Sin(a1) * radius));
                norms.Add(Vector3.up);
                uvs.Add(Vector2.zero);
                // Bottom cap
                tris.Add(botCenter);
                tris.Add(capStart + segments * 2 + i * 2 + 1);
                tris.Add(capStart + segments * 2 + i * 2);
                verts.Add(new Vector3(Mathf.Cos(a0) * radius, 0, Mathf.Sin(a0) * radius));
                norms.Add(Vector3.down);
                uvs.Add(Vector2.zero);
                verts.Add(new Vector3(Mathf.Cos(a1) * radius, 0, Mathf.Sin(a1) * radius));
                norms.Add(Vector3.down);
                uvs.Add(Vector2.zero);
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Create a box mesh centered at origin.</summary>
        public static Mesh CreateBox(float w, float h, float d)
        {
            var mesh = new Mesh();
            float hw = w * 0.5f, hh = h * 0.5f, hd = d * 0.5f;

            mesh.vertices = new Vector3[]
            {
                // Front
                new(-hw,-hh, hd), new( hw,-hh, hd), new( hw, hh, hd), new(-hw, hh, hd),
                // Back
                new( hw,-hh,-hd), new(-hw,-hh,-hd), new(-hw, hh,-hd), new( hw, hh,-hd),
                // Left
                new(-hw,-hh,-hd), new(-hw,-hh, hd), new(-hw, hh, hd), new(-hw, hh,-hd),
                // Right
                new( hw,-hh, hd), new( hw,-hh,-hd), new( hw, hh,-hd), new( hw, hh, hd),
                // Top
                new(-hw, hh, hd), new( hw, hh, hd), new( hw, hh,-hd), new(-hw, hh,-hd),
                // Bottom
                new(-hw,-hh,-hd), new( hw,-hh,-hd), new( hw,-hh, hd), new(-hw,-hh, hd),
            };

            mesh.normals = new Vector3[]
            {
                Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward,
                Vector3.back,    Vector3.back,    Vector3.back,    Vector3.back,
                Vector3.left,    Vector3.left,    Vector3.left,    Vector3.left,
                Vector3.right,   Vector3.right,   Vector3.right,   Vector3.right,
                Vector3.up,      Vector3.up,      Vector3.up,      Vector3.up,
                Vector3.down,    Vector3.down,    Vector3.down,    Vector3.down,
            };

            mesh.uv = new Vector2[]
            {
                new(0,0),new(1,0),new(1,1),new(0,1),
                new(0,0),new(1,0),new(1,1),new(0,1),
                new(0,0),new(1,0),new(1,1),new(0,1),
                new(0,0),new(1,0),new(1,1),new(0,1),
                new(0,0),new(1,0),new(1,1),new(0,1),
                new(0,0),new(1,0),new(1,1),new(0,1),
            };

            mesh.triangles = new int[]
            {
                0,2,1, 0,3,2,       // front
                4,6,5, 4,7,6,       // back
                8,10,9, 8,11,10,    // left
                12,14,13, 12,15,14, // right
                16,18,17, 16,19,18, // top
                20,22,21, 20,23,22, // bottom
            };

            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Create a gabled roof (triangular prism).</summary>
        public static Mesh CreateGabledRoof(float w, float height, float depth)
        {
            var mesh = new Mesh();
            float hw = w * 0.5f, hd = depth * 0.5f;

            mesh.vertices = new Vector3[]
            {
                // Ridge (top center line)
                new(0, height, -hd), new(0, height, hd),
                // Eaves
                new(-hw, 0, -hd), new( hw, 0, -hd),
                new(-hw, 0,  hd), new( hw, 0,  hd),
                // Gable faces (duplicated for normals)
                new(0, height, -hd), new(-hw, 0, -hd), new(hw, 0, -hd), // back gable
                new(0, height,  hd), new(-hw, 0,  hd), new(hw, 0,  hd), // front gable
            };

            mesh.triangles = new int[]
            {
                // Left slope
                0, 2, 4,  0, 4, 1,
                // Right slope
                1, 5, 3,  1, 3, 0,
                // Back gable
                6, 8, 7,
                // Front gable
                9, 10, 11,
            };

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        static Shader FindLitShader() =>
            Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Universal Render Pipeline/Simple Lit")
            ?? Shader.Find("Standard")
            ?? Shader.Find("Diffuse");

        /// <summary>Attach a MeshFilter + MeshRenderer with a simple flat-shaded material.</summary>
        public static MeshRenderer AttachMesh(GameObject go, Mesh mesh, Color color)
        {
            var mf = go.GetComponent<MeshFilter>();
            if (mf == null) mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            var mr = go.GetComponent<MeshRenderer>();
            if (mr == null) mr = go.AddComponent<MeshRenderer>();
            var mat = new Material(FindLitShader());
            mat.color = color;
            // Double-sided: prevents faces vanishing from camera angles or Z-fight flicker
            mat.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
            mr.sharedMaterial = mat;
            return mr;
        }
    }
}
