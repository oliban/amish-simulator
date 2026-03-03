using NUnit.Framework;
using AmishSimulator;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace AmishSimulator.Tests
{
    public class CommunitySystemTests
    {
        // ── RelationshipSystem pure logic ────────────────────────────────────

        [Test]
        public void RelationshipLogic_Affinity_CappedAt100()
        {
            var logic = new RelationshipLogic();
            logic.Set("bishop", 95);
            logic.Modify("bishop", 20); // would be 115
            Assert.AreEqual(100, logic.Get("bishop"));
        }

        [Test]
        public void RelationshipLogic_Affinity_CantGoBelowZero()
        {
            var logic = new RelationshipLogic();
            logic.Set("bishop", 5);
            logic.Modify("bishop", -20);
            Assert.AreEqual(0, logic.Get("bishop"));
        }

        [Test]
        public void RelationshipLogic_CanMarry_TrueAt80()
        {
            var logic = new RelationshipLogic();
            logic.Set("spouse", 80);
            Assert.IsTrue(logic.CanMarry("spouse"));
        }

        [Test]
        public void RelationshipLogic_CanMarry_FalseAt79()
        {
            var logic = new RelationshipLogic();
            logic.Set("spouse", 79);
            Assert.IsFalse(logic.CanMarry("spouse"));
        }

        [Test]
        public void RelationshipLogic_AffinityChanged_EventFires()
        {
            bool fired = false;
            var logic = new RelationshipLogic();
            logic.OnChanged += (id, old, nw) => fired = true;
            logic.Set("bishop", 50);
            logic.Modify("bishop", 10);
            Assert.IsTrue(fired);
        }

        [Test]
        public void RelationshipLogic_Level_Stranger_At10()
        {
            var logic = new RelationshipLogic();
            logic.Set("bishop", 10);
            Assert.AreEqual(RelationshipLevel.Stranger, logic.GetLevel("bishop"));
        }

        [Test]
        public void RelationshipLogic_Level_TrustedCompanion_At90()
        {
            var logic = new RelationshipLogic();
            logic.Set("bishop", 90);
            Assert.AreEqual(RelationshipLevel.TrustedCompanion, logic.GetLevel("bishop"));
        }

        // ── OrdnungSystem pure logic ─────────────────────────────────────────

        [Test]
        public void OrdnungLogic_ViolationCount_Increments()
        {
            var logic = new OrdnungLogic(shunThreshold: 5, isGmay: false);
            logic.RecordViolation(OrdnungRule.UsingElectricity);
            Assert.AreEqual(1, logic.GetCount(OrdnungRule.UsingElectricity));
        }

        [Test]
        public void OrdnungLogic_Shunning_TriggeredAtThreshold()
        {
            bool shunned = false;
            var logic = new OrdnungLogic(shunThreshold: 3, isGmay: false);
            logic.OnShunning += () => shunned = true;
            logic.RecordViolation(OrdnungRule.UsingElectricity);
            logic.RecordViolation(OrdnungRule.SkippingGmay);
            logic.RecordViolation(OrdnungRule.MustacheDetected);
            Assert.IsTrue(shunned);
        }

        [Test]
        public void OrdnungLogic_GmayOnly_NotEnforced_OnOrdnung()
        {
            var logic = new OrdnungLogic(shunThreshold: 5, isGmay: false);
            logic.RecordViolation(OrdnungRule.DiagonalWalking);
            Assert.AreEqual(0, logic.GetCount(OrdnungRule.DiagonalWalking));
        }

        [Test]
        public void OrdnungLogic_GmayOnly_Enforced_OnGmay()
        {
            var logic = new OrdnungLogic(shunThreshold: 5, isGmay: true);
            logic.RecordViolation(OrdnungRule.DiagonalWalking);
            Assert.AreEqual(1, logic.GetCount(OrdnungRule.DiagonalWalking));
        }

        // ── EventCalendar pure logic ─────────────────────────────────────────

        [Test]
        public void EventCalendar_GetEventsForDay_BarnRaising_SpringDay7()
        {
            var cal = new EventCalendarLogic();
            var events = cal.GetEventsForDay(Season.Spring, 7);
            Assert.IsTrue(events.Exists(e => e.eventType == EventType.BarnRaising));
        }

        [Test]
        public void EventCalendar_GetEventsForDay_QuiltingBee_FallDay7()
        {
            var cal = new EventCalendarLogic();
            var events = cal.GetEventsForDay(Season.Fall, 7);
            Assert.IsTrue(events.Exists(e => e.eventType == EventType.QuiltingBee));
        }

        [Test]
        public void EventCalendar_GetEventsForDay_EmptyOnNonEventDay()
        {
            var cal = new EventCalendarLogic();
            var events = cal.GetEventsForDay(Season.Spring, 3);
            Assert.AreEqual(0, events.Count);
        }

        [Test]
        public void EventCalendar_GmayService_OccursTwiceInWinter()
        {
            var cal = new EventCalendarLogic();
            int count = 0;
            for (int d = 1; d <= 28; d++)
            {
                var events = cal.GetEventsForDay(Season.Winter, d);
                foreach (var e in events)
                    if (e.eventType == EventType.GmayService) count++;
            }
            Assert.AreEqual(2, count);
        }
    }

    // ── Pure-logic test helpers ──────────────────────────────────────────────

    public class RelationshipLogic
    {
        private readonly Dictionary<string, int> _data = new();
        public event Action<string, int, int> OnChanged;

        public void Set(string id, int val) => _data[id] = Mathf.Clamp(val, 0, 100);

        public void Modify(string id, int delta)
        {
            int old = _data.TryGetValue(id, out int v) ? v : 0;
            int nw  = Mathf.Clamp(old + delta, 0, 100);
            _data[id] = nw;
            OnChanged?.Invoke(id, old, nw);
        }

        public int Get(string id) => _data.TryGetValue(id, out int v) ? v : 0;
        public bool CanMarry(string id) => Get(id) >= 80;

        public RelationshipLevel GetLevel(string id) => Get(id) switch
        {
            < 20 => RelationshipLevel.Stranger,
            < 40 => RelationshipLevel.Acquaintance,
            < 60 => RelationshipLevel.Friend,
            < 80 => RelationshipLevel.GoodFriend,
            _    => RelationshipLevel.TrustedCompanion
        };
    }

    public class OrdnungLogic
    {
        private readonly Dictionary<OrdnungRule, int> _violations = new();
        private readonly int _threshold;
        private readonly bool _isGmay;
        private bool _shunned = false;

        private static readonly HashSet<OrdnungRule> GmayOnly = new()
        {
            OrdnungRule.DiagonalWalking, OrdnungRule.UnapprovedTurnipShape
        };

        public event Action OnShunning;

        public OrdnungLogic(int shunThreshold, bool isGmay)
        {
            _threshold = shunThreshold;
            _isGmay = isGmay;
            foreach (OrdnungRule r in Enum.GetValues(typeof(OrdnungRule)))
                _violations[r] = 0;
        }

        public void RecordViolation(OrdnungRule rule)
        {
            if (GmayOnly.Contains(rule) && !_isGmay) return;
            _violations[rule]++;
            if (!_shunned && GetTotal() >= _threshold)
            {
                _shunned = true;
                OnShunning?.Invoke();
            }
        }

        public int GetCount(OrdnungRule rule) => _violations.TryGetValue(rule, out int v) ? v : 0;
        public int GetTotal() { int t = 0; foreach (var v in _violations.Values) t += v; return t; }
        public bool IsShunned() => _shunned;
    }

    public class EventCalendarLogic
    {
        private readonly List<CommunityEvent> _schedule;

        public EventCalendarLogic()
        {
            _schedule = new List<CommunityEvent>
            {
                new() { season=Season.Spring, dayInSeason=7,  eventType=EventType.BarnRaising },
                new() { season=Season.Spring, dayInSeason=14, eventType=EventType.PlantingDay },
                new() { season=Season.Summer, dayInSeason=7,  eventType=EventType.HarvestPicnic },
                new() { season=Season.Summer, dayInSeason=21, eventType=EventType.CourtshipSingalong },
                new() { season=Season.Fall,   dayInSeason=7,  eventType=EventType.QuiltingBee },
                new() { season=Season.Fall,   dayInSeason=14, eventType=EventType.ApplePressing },
                new() { season=Season.Fall,   dayInSeason=21, eventType=EventType.CanningDay },
                new() { season=Season.Winter, dayInSeason=7,  eventType=EventType.GmayService },
                new() { season=Season.Winter, dayInSeason=21, eventType=EventType.GmayService },
                new() { season=Season.Winter, dayInSeason=14, eventType=EventType.WoodChoppingRace },
                new() { season=Season.Winter, dayInSeason=28, eventType=EventType.StorytellingNight },
            };
        }

        public List<CommunityEvent> GetEventsForDay(Season season, int day)
        {
            return _schedule.FindAll(e => e.season == season && e.dayInSeason == day);
        }
    }
}
