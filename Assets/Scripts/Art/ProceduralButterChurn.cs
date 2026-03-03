using UnityEngine;

namespace AmishSimulator
{
    /// <summary>Low-poly butter churn prop.</summary>
    [ExecuteAlways]
    public class ProceduralButterChurn : MonoBehaviour
    {
        public bool generateOnStart = true;

        private void Start() { if (generateOnStart) Generate(); }

        [ContextMenu("Generate Churn")]
        public void Generate()
        {
            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);

            // Main barrel body — slightly tapered (approximate with two cylinders)
            var body = new GameObject("Body");
            body.transform.SetParent(transform, false);
            body.transform.localPosition = new Vector3(0, 0.05f, 0);
            ProceduralMeshUtils.AttachMesh(body,
                ProceduralMeshUtils.CreateCylinder(0.18f, 0.55f, 10),
                new Color(0.6f, 0.42f, 0.22f)); // wood brown

            // Lid
            var lid = new GameObject("Lid");
            lid.transform.SetParent(transform, false);
            lid.transform.localPosition = new Vector3(0, 0.60f, 0);
            ProceduralMeshUtils.AttachMesh(lid,
                ProceduralMeshUtils.CreateCylinder(0.19f, 0.04f, 10),
                new Color(0.45f, 0.30f, 0.15f));

            // Dasher rod (sticking up through lid)
            var rod = new GameObject("DasherRod");
            rod.transform.SetParent(transform, false);
            rod.transform.localPosition = new Vector3(0, 0.62f, 0);
            ProceduralMeshUtils.AttachMesh(rod,
                ProceduralMeshUtils.CreateCylinder(0.025f, 0.60f, 6),
                new Color(0.35f, 0.22f, 0.10f));

            // Handle crosspiece at top of rod
            var handle = new GameObject("Handle");
            handle.transform.SetParent(transform, false);
            handle.transform.localPosition = new Vector3(0, 1.20f, 0);
            handle.transform.localRotation = Quaternion.Euler(0, 0, 90);
            ProceduralMeshUtils.AttachMesh(handle,
                ProceduralMeshUtils.CreateCylinder(0.02f, 0.28f, 6),
                new Color(0.35f, 0.22f, 0.10f));

            // Hoops (decorative bands around barrel)
            for (int i = 0; i < 3; i++)
            {
                var hoop = new GameObject($"Hoop{i}");
                hoop.transform.SetParent(transform, false);
                hoop.transform.localPosition = new Vector3(0, 0.15f + i * 0.18f, 0);
                ProceduralMeshUtils.AttachMesh(hoop,
                    ProceduralMeshUtils.CreateCylinder(0.185f, 0.025f, 10),
                    new Color(0.25f, 0.20f, 0.15f)); // dark iron
            }
        }
    }
}
