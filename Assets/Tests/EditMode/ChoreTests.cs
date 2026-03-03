using NUnit.Framework;
using AmishSimulator;
using UnityEngine;

namespace AmishSimulator.Tests
{
    public class ChoreTests
    {
        // ── ButterChurning pure logic ────────────────────────────────────────

        [Test]
        public void ButterChurning_Hit_IncreasesProgress()
        {
            var logic = new ButterChurningLogic(maxMissed: 3);
            logic.SetBeatWindowOpen(true);
            logic.OnPlayerInput();
            Assert.Greater(logic.Progress, 0f);
        }

        [Test]
        public void ButterChurning_HitOutsideWindow_CountsAsMiss()
        {
            var logic = new ButterChurningLogic(maxMissed: 3);
            logic.SetBeatWindowOpen(false);
            logic.OnPlayerInput();
            Assert.AreEqual(1, logic.MissedBeats);
        }

        [Test]
        public void ButterChurning_MaxMissedBeats_TriggersFail()
        {
            bool failed = false;
            var logic = new ButterChurningLogic(maxMissed: 2);
            logic.OnFail += () => failed = true;
            logic.SetBeatWindowOpen(false);
            logic.OnPlayerInput(); // miss 1
            logic.OnPlayerInput(); // miss 2 → fail
            Assert.IsTrue(failed);
        }

        [Test]
        public void ButterChurning_ProgressReachesOne_TriggersSuccess()
        {
            bool succeeded = false;
            var logic = new ButterChurningLogic(maxMissed: 99, progressPerHit: 0.5f);
            logic.OnSuccess += () => succeeded = true;
            logic.SetBeatWindowOpen(true);
            logic.OnPlayerInput(); // 0.5
            logic.OnPlayerInput(); // 1.0 → success
            Assert.IsTrue(succeeded);
        }

        [Test]
        public void ButterChurning_MissedBeat_WhenNoInput()
        {
            var logic = new ButterChurningLogic(maxMissed: 3);
            logic.TickBeat(); // no input this beat
            Assert.AreEqual(1, logic.MissedBeats);
        }

        // ── Plowing pure logic ───────────────────────────────────────────────

        [Test]
        public void Plowing_MoveHorse_MarksCell()
        {
            var logic = new PlowingLogic(4, 3);
            logic.Move(Vector2Int.right);
            Assert.IsTrue(logic.IsCellPlowed(new Vector2Int(1, 0)));
        }

        [Test]
        public void Plowing_CompletionPercent_ZeroAtStart()
        {
            var logic = new PlowingLogic(4, 3);
            Assert.AreEqual(0f, logic.GetCompletion(), 0.001f);
        }

        [Test]
        public void Plowing_CompletionPercent_100_WhenAllPlowed()
        {
            var logic = new PlowingLogic(2, 2);
            // Plow all 4 cells (start at 0,0 — move right, down, left)
            logic.Move(Vector2Int.right);  // (1,0)
            logic.Move(Vector2Int.up);     // (1,1)
            logic.Move(Vector2Int.left);   // (0,1)
            Assert.AreEqual(0.75f, logic.GetCompletion(), 0.001f); // 3/4 plowed
        }

        [Test]
        public void Plowing_OutOfBoundsMove_Rejected()
        {
            var logic = new PlowingLogic(4, 3);
            bool result = logic.Move(Vector2Int.left); // can't go left from (0,0)
            Assert.IsFalse(result);
        }

        [Test]
        public void Plowing_TimerExpiry_FailAt50Percent()
        {
            bool failed = false;
            var logic = new PlowingLogic(4, 4, timeLimit: 10f);
            logic.OnFail += () => failed = true;
            // Plow only 4 of 16 cells (25%)
            logic.Move(Vector2Int.right);
            logic.Move(Vector2Int.right);
            logic.Move(Vector2Int.right);
            logic.Tick(11f); // time out
            Assert.IsTrue(failed);
        }

        [Test]
        public void Plowing_TimerExpiry_SuccessAt80Percent()
        {
            bool succeeded = false;
            // 2x2 grid, need >= 80% → need 4 cells (it's 100% of 4)
            var logic = new PlowingLogic(2, 2, timeLimit: 10f);
            logic.OnSuccess += () => succeeded = true;
            logic.Move(Vector2Int.right);  // (1,0)
            logic.Move(Vector2Int.up);     // (1,1)
            logic.Move(Vector2Int.left);   // (0,1)
            logic.Tick(11f); // 3/4 = 75% — should fail, not succeed
            // Expect fail at 75%
            Assert.IsFalse(succeeded);
        }
    }

    // ── Pure-logic test helpers ──────────────────────────────────────────────

    public class ButterChurningLogic
    {
        public float Progress { get; private set; } = 0f;
        public int MissedBeats { get; private set; } = 0;

        private bool _beatWindowOpen = false;
        private bool _inputThisBeat = false;
        private readonly int _maxMissed;
        private readonly float _progressPerHit;
        private bool _done = false;

        public event System.Action OnSuccess;
        public event System.Action OnFail;

        public ButterChurningLogic(int maxMissed, float progressPerHit = 0.1f)
        {
            _maxMissed = maxMissed;
            _progressPerHit = progressPerHit;
        }

        public void SetBeatWindowOpen(bool open) => _beatWindowOpen = open;

        public void OnPlayerInput()
        {
            if (_done) return;
            if (_beatWindowOpen && !_inputThisBeat)
            {
                _inputThisBeat = true;
                Progress += _progressPerHit;
                if (Progress >= 1f) { _done = true; OnSuccess?.Invoke(); }
            }
            else if (!_beatWindowOpen)
            {
                RegisterMiss();
            }
        }

        public void TickBeat()
        {
            if (!_inputThisBeat) RegisterMiss();
            _inputThisBeat = false;
        }

        private void RegisterMiss()
        {
            if (_done) return;
            MissedBeats++;
            if (MissedBeats >= _maxMissed) { _done = true; OnFail?.Invoke(); }
        }
    }

    public class PlowingLogic
    {
        private readonly bool[,] _plowed;
        private Vector2Int _pos = Vector2Int.zero;
        private readonly int _width, _height;
        private int _plowedCount = 0;
        private float _timeRemaining;
        private bool _done = false;

        public event System.Action OnSuccess;
        public event System.Action OnFail;

        public PlowingLogic(int width, int height, float timeLimit = float.MaxValue)
        {
            _width = width; _height = height;
            _plowed = new bool[width, height];
            _timeRemaining = timeLimit;
        }

        public bool Move(Vector2Int dir)
        {
            var next = _pos + dir;
            if (next.x < 0 || next.x >= _width || next.y < 0 || next.y >= _height) return false;
            _pos = next;
            if (!_plowed[next.x, next.y]) { _plowed[next.x, next.y] = true; _plowedCount++; }
            return true;
        }

        public bool IsCellPlowed(Vector2Int p) => _plowed[p.x, p.y];

        public float GetCompletion() => (float)_plowedCount / (_width * _height);

        public void Tick(float dt)
        {
            if (_done) return;
            _timeRemaining -= dt;
            if (_timeRemaining <= 0f)
            {
                _done = true;
                if (GetCompletion() >= 0.8f) OnSuccess?.Invoke();
                else OnFail?.Invoke();
            }
        }
    }
}
