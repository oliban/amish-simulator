using UnityEngine;

namespace AmishSimulator
{
    /// <summary>
    /// Procedurally generated low-poly blocky character.
    /// Generates body, head, limbs, hat/bonnet, and a separate beard child object.
    /// BeardSystem scales the beard child's Y to 0-1.
    /// </summary>
    [ExecuteAlways]
    public class ProceduralCharacter : MonoBehaviour
    {
        [Header("Character")]
        public Gender gender = Gender.Male;

        [Header("Colors")]
        public Color skinColor    = new(0.92f, 0.78f, 0.65f);
        public Color shirtColor   = new(0.20f, 0.25f, 0.35f); // dark blue Amish shirt
        public Color trouserColor = new(0.18f, 0.18f, 0.20f);
        public Color hatColor     = new(0.12f, 0.10f, 0.08f); // black felt hat
        public Color beardColor   = new(0.40f, 0.28f, 0.16f); // brown

        public bool generateOnStart = true;

        // Reference to the beard object so BeardSystem can scale it
        public Transform BeardRoot { get; private set; }

        private void Start() { if (generateOnStart) Generate(); }

        [ContextMenu("Generate Character")]
        public void Generate()
        {
            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);

            BuildLegs();
            BuildTorso();
            BuildArms();
            BuildHead();
            if (gender == Gender.Male) BuildHat();
            else BuildBonnet();
            if (gender == Gender.Male) BuildBeard();
        }

        private void BuildLegs()
        {
            for (int side = -1; side <= 1; side += 2)
            {
                var leg = new GameObject(side < 0 ? "LegLeft" : "LegRight");
                leg.transform.SetParent(transform, false);
                leg.transform.localPosition = new Vector3(side * 0.13f, 0.45f, 0);
                ProceduralMeshUtils.AttachMesh(leg,
                    ProceduralMeshUtils.CreateBox(0.22f, 0.9f, 0.22f), trouserColor);
            }
        }

        private void BuildTorso()
        {
            var torso = new GameObject("Torso");
            torso.transform.SetParent(transform, false);
            torso.transform.localPosition = new Vector3(0, 1.25f, 0);
            ProceduralMeshUtils.AttachMesh(torso,
                ProceduralMeshUtils.CreateBox(0.55f, 0.70f, 0.30f), shirtColor);

            // Suspenders (thin vertical strips)
            for (int side = -1; side <= 1; side += 2)
            {
                var sus = new GameObject($"Suspender{side}");
                sus.transform.SetParent(transform, false);
                sus.transform.localPosition = new Vector3(side * 0.15f, 1.25f, 0.16f);
                ProceduralMeshUtils.AttachMesh(sus,
                    ProceduralMeshUtils.CreateBox(0.04f, 0.65f, 0.01f),
                    new Color(0.35f, 0.22f, 0.10f));
            }
        }

        private void BuildArms()
        {
            for (int side = -1; side <= 1; side += 2)
            {
                var arm = new GameObject(side < 0 ? "ArmLeft" : "ArmRight");
                arm.transform.SetParent(transform, false);
                arm.transform.localPosition = new Vector3(side * 0.42f, 1.15f, 0);
                arm.transform.localRotation = Quaternion.Euler(0, 0, side * 10f);
                ProceduralMeshUtils.AttachMesh(arm,
                    ProceduralMeshUtils.CreateBox(0.18f, 0.65f, 0.18f), shirtColor);

                // Hand
                var hand = new GameObject(side < 0 ? "HandLeft" : "HandRight");
                hand.transform.SetParent(transform, false);
                hand.transform.localPosition = new Vector3(side * 0.44f, 0.78f, 0);
                ProceduralMeshUtils.AttachMesh(hand,
                    ProceduralMeshUtils.CreateBox(0.16f, 0.18f, 0.14f), skinColor);
            }
        }

        private void BuildHead()
        {
            var head = new GameObject("Head");
            head.transform.SetParent(transform, false);
            head.transform.localPosition = new Vector3(0, 1.85f, 0);
            ProceduralMeshUtils.AttachMesh(head,
                ProceduralMeshUtils.CreateBox(0.40f, 0.42f, 0.38f), skinColor);
        }

        private void BuildHat()
        {
            // Amish wide-brim black felt hat
            var brim = new GameObject("HatBrim");
            brim.transform.SetParent(transform, false);
            brim.transform.localPosition = new Vector3(0, 2.07f, 0);
            ProceduralMeshUtils.AttachMesh(brim,
                ProceduralMeshUtils.CreateCylinder(0.42f, 0.04f, 12), hatColor);

            var crown = new GameObject("HatCrown");
            crown.transform.SetParent(transform, false);
            crown.transform.localPosition = new Vector3(0, 2.09f, 0);
            ProceduralMeshUtils.AttachMesh(crown,
                ProceduralMeshUtils.CreateCylinder(0.22f, 0.30f, 10), hatColor);
        }

        private void BuildBonnet()
        {
            // Simple bonnet: a half-dome + brim
            var bonnet = new GameObject("Bonnet");
            bonnet.transform.SetParent(transform, false);
            bonnet.transform.localPosition = new Vector3(0, 1.88f, -0.05f);
            ProceduralMeshUtils.AttachMesh(bonnet,
                ProceduralMeshUtils.CreateCylinder(0.28f, 0.22f, 10),
                new Color(0.85f, 0.82f, 0.75f)); // off-white

            var brim = new GameObject("BonnetBrim");
            brim.transform.SetParent(transform, false);
            brim.transform.localPosition = new Vector3(0, 1.87f, 0.10f);
            ProceduralMeshUtils.AttachMesh(brim,
                ProceduralMeshUtils.CreateBox(0.38f, 0.05f, 0.22f),
                new Color(0.85f, 0.82f, 0.75f));
        }

        private void BuildBeard()
        {
            // Beard as a separate child — BeardSystem scales this object's localScale.y
            var beardRoot = new GameObject("BeardRoot");
            beardRoot.transform.SetParent(transform, false);
            beardRoot.transform.localPosition = new Vector3(0, 1.62f, 0.18f);
            beardRoot.transform.localScale = new Vector3(1, 0, 1); // starts at 0 (shaven)
            BeardRoot = beardRoot.transform;

            // Beard mesh (tapered box, grows downward from chin)
            var beardMesh = new GameObject("BeardMesh");
            beardMesh.transform.SetParent(beardRoot.transform, false);
            beardMesh.transform.localPosition = new Vector3(0, -0.25f, 0);
            ProceduralMeshUtils.AttachMesh(beardMesh,
                ProceduralMeshUtils.CreateBox(0.32f, 0.50f, 0.14f), beardColor);
        }
    }
}
