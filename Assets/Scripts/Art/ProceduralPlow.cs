using UnityEngine;

namespace AmishSimulator
{
    /// <summary>Low-poly horse-drawn plow prop.</summary>
    [ExecuteAlways]
    public class ProceduralPlow : MonoBehaviour
    {
        public bool generateOnStart = true;

        private void Start() { if (generateOnStart) Generate(); }

        [ContextMenu("Generate Plow")]
        public void Generate()
        {
            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);

            Color wood  = new(0.55f, 0.38f, 0.20f);
            Color metal = new(0.35f, 0.35f, 0.38f);

            // Beam (main horizontal shaft)
            var beam = new GameObject("Beam");
            beam.transform.SetParent(transform, false);
            beam.transform.localPosition = new Vector3(0, 0.35f, 0);
            ProceduralMeshUtils.AttachMesh(beam,
                ProceduralMeshUtils.CreateBox(0.1f, 0.1f, 2.2f), wood);

            // Plowshare (metal blade angled into ground)
            var share = new GameObject("Plowshare");
            share.transform.SetParent(transform, false);
            share.transform.localPosition = new Vector3(0, 0.08f, -0.8f);
            share.transform.localRotation = Quaternion.Euler(30, 0, 0);
            ProceduralMeshUtils.AttachMesh(share,
                ProceduralMeshUtils.CreateBox(0.25f, 0.06f, 0.5f), metal);

            // Mouldboard (curved soil deflector — approximated as tilted box)
            var mould = new GameObject("Mouldboard");
            mould.transform.SetParent(transform, false);
            mould.transform.localPosition = new Vector3(0.15f, 0.25f, -0.6f);
            mould.transform.localRotation = Quaternion.Euler(20, 0, 30);
            ProceduralMeshUtils.AttachMesh(mould,
                ProceduralMeshUtils.CreateBox(0.08f, 0.35f, 0.55f), metal);

            // Left handle
            var handleL = new GameObject("HandleLeft");
            handleL.transform.SetParent(transform, false);
            handleL.transform.localPosition = new Vector3(-0.22f, 0.55f, 0.75f);
            handleL.transform.localRotation = Quaternion.Euler(-15, 0, 8);
            ProceduralMeshUtils.AttachMesh(handleL,
                ProceduralMeshUtils.CreateCylinder(0.03f, 0.75f, 6), wood);

            // Right handle
            var handleR = new GameObject("HandleRight");
            handleR.transform.SetParent(transform, false);
            handleR.transform.localPosition = new Vector3(0.22f, 0.55f, 0.75f);
            handleR.transform.localRotation = Quaternion.Euler(-15, 0, -8);
            ProceduralMeshUtils.AttachMesh(handleR,
                ProceduralMeshUtils.CreateCylinder(0.03f, 0.75f, 6), wood);

            // Wheel (front)
            var wheel = new GameObject("Wheel");
            wheel.transform.SetParent(transform, false);
            wheel.transform.localPosition = new Vector3(0, 0.18f, 0.9f);
            wheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
            ProceduralMeshUtils.AttachMesh(wheel,
                ProceduralMeshUtils.CreateCylinder(0.18f, 0.06f, 12), wood);

            // Yoke attachment point (front of beam)
            var yoke = new GameObject("YokeBar");
            yoke.transform.SetParent(transform, false);
            yoke.transform.localPosition = new Vector3(0, 0.38f, -1.0f);
            ProceduralMeshUtils.AttachMesh(yoke,
                ProceduralMeshUtils.CreateBox(0.7f, 0.07f, 0.07f), wood);
        }
    }
}
