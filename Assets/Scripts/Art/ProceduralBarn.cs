using UnityEngine;

namespace AmishSimulator
{
    /// <summary>
    /// Procedurally generated Amish barn. Attach to an empty GameObject.
    /// Call Generate() or enable auto-generate on Start.
    /// </summary>
    [ExecuteAlways]
    public class ProceduralBarn : MonoBehaviour
    {
        [Header("Dimensions")]
        public float width  = 8f;
        public float wallHeight = 4f;
        public float depth  = 12f;
        public float roofHeight = 3f;

        [Header("Colors")]
        public Color wallColor  = new(0.55f, 0.15f, 0.10f); // Amish red
        public Color roofColor  = new(0.20f, 0.20f, 0.20f); // dark grey
        public Color doorColor  = new(0.35f, 0.22f, 0.10f); // brown

        [Header("Auto")]
        public bool generateOnStart = true;

        private void Start()
        {
            if (generateOnStart) Generate();
        }

        [ContextMenu("Generate Barn")]
        public void Generate()
        {
            // Clear previous children
            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);

            BuildWalls();
            BuildRoof();
            BuildDoors();
            BuildFoundation();
        }

        private void BuildWalls()
        {
            var go = new GameObject("Walls");
            go.transform.SetParent(transform, false);
            // Shift down 0.1 so the bottom face is slightly underground — eliminates Z-fighting with the ground plane
            go.transform.localPosition = new Vector3(0, wallHeight * 0.5f - 0.1f, 0);
            ProceduralMeshUtils.AttachMesh(go,
                ProceduralMeshUtils.CreateBox(width, wallHeight + 0.1f, depth),
                wallColor);
        }

        private void BuildRoof()
        {
            var go = new GameObject("Roof");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0, wallHeight, 0);
            ProceduralMeshUtils.AttachMesh(go,
                ProceduralMeshUtils.CreateGabledRoof(width + 0.4f, roofHeight, depth + 0.4f),
                roofColor);
        }

        private void BuildDoors()
        {
            // Large barn doors on the front face (Z+ side)
            var go = new GameObject("BarnDoors");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0, wallHeight * 0.4f, depth * 0.5f + 0.01f);
            ProceduralMeshUtils.AttachMesh(go,
                ProceduralMeshUtils.CreateBox(width * 0.55f, wallHeight * 0.8f, 0.05f),
                doorColor);
        }

        private void BuildFoundation()
        {
            var go = new GameObject("Foundation");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0, -0.15f, 0);
            ProceduralMeshUtils.AttachMesh(go,
                ProceduralMeshUtils.CreateBox(width + 0.3f, 0.3f, depth + 0.3f),
                new Color(0.5f, 0.45f, 0.35f)); // stone grey
        }
    }
}
