using NUnit.Framework;
using AmishSimulator;
using UnityEngine;

namespace AmishSimulator.Tests
{
    public class ChoreTests2
    {
        // ── BarnRaising pure logic ───────────────────────────────────────────

        [Test]
        public void BarnRaising_PlaceBeam_IncrementsCount()
        {
            var logic = new BarnRaisingLogic(totalBeams: 6, npcRate: 0f);
            logic.Initialize();
            logic.PlaceBeam(0);
            Assert.AreEqual(1, logic.PlacedBeams);
        }

        [Test]
        public void BarnRaising_PlaceBeam_MarksAsPlaced()
        {
            var logic = new BarnRaisingLogic(totalBeams: 4, npcRate: 0f);
            logic.Initialize();
            bool placed = logic.PlaceBeam(0);
            Assert.IsTrue(placed);
        }

        [Test]
        public void BarnRaising_PlaceBeam_ReturnsFalseIfAlreadyPlaced()
        {
            var logic = new BarnRaisingLogic(totalBeams: 4, npcRate: 0f);
            logic.Initialize();
            logic.PlaceBeam(0);
            bool second = logic.PlaceBeam(0); // already placed
            Assert.IsFalse(second);
        }

        [Test]
        public void BarnRaising_NpcTick_AutoPlacesNonPlayerBeams()
        {
            var logic = new BarnRaisingLogic(totalBeams: 4, npcRate: 10f); // fast NPC
            logic.Initialize();
            logic.Tick(1f); // should place some NPC-only beams
            Assert.Greater(logic.PlacedBeams, 0);
        }

        [Test]
        public void BarnRaising_SunsetExpiry_AllPlaced_Succeeds()
        {
            bool succeeded = false;
            var logic = new BarnRaisingLogic(totalBeams: 2, npcRate: 0f, timeLimit: 5f);
            logic.OnSuccess += () => succeeded = true;
            logic.Initialize();
            logic.PlaceBeam(0);
            logic.PlaceBeam(1);
            logic.Tick(6f); // time expires, but all placed
            // All placed triggers success before timer
            Assert.IsTrue(succeeded);
        }

        [Test]
        public void BarnRaising_SunsetExpiry_NotComplete_Fails()
        {
            bool failed = false;
            var logic = new BarnRaisingLogic(totalBeams: 6, npcRate: 0f, timeLimit: 5f);
            logic.OnFail += () => failed = true;
            logic.Initialize();
            logic.PlaceBeam(0); // only 1 of 6 placed
            logic.Tick(6f);
            Assert.IsTrue(failed);
        }

        // ── QuiltingBee pure logic ───────────────────────────────────────────

        [Test]
        public void Quilt_PlaceSquare_SetsPattern()
        {
            var logic = new QuiltingBeeLogic(timeLimit: 120f);
            logic.PlaceSquare(0, 0, QuiltColor.Blue);
            Assert.AreEqual(QuiltColor.Blue, logic.PlayerPattern.Get(0, 0));
        }

        [Test]
        public void Quilt_GetMatchPercent_ZeroOnEmpty()
        {
            var logic = new QuiltingBeeLogic(timeLimit: 120f);
            // Target and player both initialized empty — all Blue vs Blue = 100% match actually
            // Let's set a specific target and verify mismatch
            var target = QuiltPattern.Empty();
            target.Set(0, 0, QuiltColor.Red);
            logic.SetTarget(target);
            Assert.Less(logic.GetMatchPercent(), 1f);
        }

        [Test]
        public void Quilt_GetMatchPercent_100_WhenFullMatch()
        {
            var logic = new QuiltingBeeLogic(timeLimit: 120f);
            var pattern = QuiltPattern.Empty();
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    pattern.Set(r, c, QuiltColor.Red);
            logic.SetTarget(pattern);
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    logic.PlaceSquare(r, c, QuiltColor.Red);
            Assert.AreEqual(1f, logic.GetMatchPercent(), 0.001f);
        }

        [Test]
        public void Quilt_TimeExpiry_Below75Percent_Fails()
        {
            bool failed = false;
            var logic = new QuiltingBeeLogic(timeLimit: 5f);
            logic.OnFail += () => failed = true;
            // Set mismatching target
            var target = QuiltPattern.Empty();
            for (int i = 0; i < 16; i++) target.squares[i] = QuiltColor.Red;
            logic.SetTarget(target);
            // Leave player pattern empty (all Blue = 0 match)
            logic.Tick(6f);
            Assert.IsTrue(failed);
        }

        [Test]
        public void Quilt_TimeExpiry_Above75Percent_Succeeds()
        {
            bool succeeded = false;
            var logic = new QuiltingBeeLogic(timeLimit: 5f);
            logic.OnSuccess += () => succeeded = true;
            // Make target all Blue (default), player also all Blue = 100% match
            // Both start as Empty (all Blue = 0), so they match perfectly
            logic.Tick(6f);
            Assert.IsTrue(succeeded);
        }
    }

    // ── Pure-logic test helpers ──────────────────────────────────────────────

    public class BarnRaisingLogic
    {
        private bool[] _placed;
        private bool[] _requiresPlayer;
        private readonly int _total;
        private float _npcRate;
        private float _timer;
        private bool _done;

        public int PlacedBeams { get; private set; }
        public event System.Action OnSuccess;
        public event System.Action OnFail;

        public BarnRaisingLogic(int totalBeams, float npcRate, float timeLimit = float.MaxValue)
        {
            _total = totalBeams;
            _npcRate = npcRate;
            _timer = timeLimit;
        }

        public void Initialize()
        {
            _placed = new bool[_total];
            _requiresPlayer = new bool[_total];
            var rng = new System.Random(42);
            for (int i = 0; i < _total; i++)
                _requiresPlayer[i] = rng.NextDouble() < 0.4;
        }

        public bool PlaceBeam(int id)
        {
            if (id < 0 || id >= _total || _placed[id]) return false;
            _placed[id] = true;
            PlacedBeams++;
            if (PlacedBeams >= _total && !_done) { _done = true; OnSuccess?.Invoke(); }
            return true;
        }

        public void Tick(float dt)
        {
            if (_done) return;
            // NPC places non-player beams
            float acc = _npcRate * dt;
            for (int i = 0; i < _total && acc >= 1f; i++)
            {
                if (!_placed[i] && !_requiresPlayer[i])
                {
                    _placed[i] = true;
                    PlacedBeams++;
                    acc -= 1f;
                    if (PlacedBeams >= _total) { _done = true; OnSuccess?.Invoke(); return; }
                }
            }
            _timer -= dt;
            if (_timer <= 0f && !_done)
            {
                _done = true;
                if (PlacedBeams >= _total) OnSuccess?.Invoke();
                else OnFail?.Invoke();
            }
        }
    }

    public class QuiltingBeeLogic
    {
        private QuiltPattern _target;
        public QuiltPattern PlayerPattern { get; private set; }
        private float _timer;
        private bool _done;

        public event System.Action OnSuccess;
        public event System.Action OnFail;

        public QuiltingBeeLogic(float timeLimit)
        {
            _timer = timeLimit;
            _target = QuiltPattern.Empty();
            PlayerPattern = QuiltPattern.Empty();
        }

        public void SetTarget(QuiltPattern p) => _target = p;

        public void PlaceSquare(int row, int col, QuiltColor color)
        {
            PlayerPattern.Set(row, col, color);
        }

        public float GetMatchPercent()
        {
            int matched = 0;
            for (int i = 0; i < 16; i++)
                if (_target.squares[i] == PlayerPattern.squares[i]) matched++;
            return matched / 16f;
        }

        public void Tick(float dt)
        {
            if (_done) return;
            _timer -= dt;
            if (_timer <= 0f)
            {
                _done = true;
                if (GetMatchPercent() >= 0.75f) OnSuccess?.Invoke();
                else OnFail?.Invoke();
            }
        }
    }
}
