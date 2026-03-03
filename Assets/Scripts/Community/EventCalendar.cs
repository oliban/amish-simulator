using UnityEngine;
using System;
using System.Collections.Generic;

namespace AmishSimulator
{
    public enum EventType
    {
        BarnRaising, PlantingDay, HarvestPicnic, CourtshipSingalong,
        QuiltingBee, ApplePressing, CanningDay, GmayService,
        WoodChoppingRace, StorytellingNight
    }

    [Serializable]
    public struct CommunityEvent
    {
        public string eventName;
        public Season season;
        public int dayInSeason;
        public EventType eventType;
        public string description;
    }

    public class EventCalendar : MonoBehaviour
    {
        public static EventCalendar Instance { get; private set; }

        private List<CommunityEvent> _schedule;

        public event Action<CommunityEvent> OnEventStarted;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildSchedule();
        }

        private void Start()
        {
            if (TimeSystem.Instance != null)
                TimeSystem.Instance.OnDayChanged += CheckEventsForDay;
        }

        private void OnDestroy()
        {
            if (TimeSystem.Instance != null)
                TimeSystem.Instance.OnDayChanged -= CheckEventsForDay;
        }

        private void BuildSchedule()
        {
            _schedule = new List<CommunityEvent>
            {
                new() { eventName="Barn Raising",        season=Season.Spring, dayInSeason=7,  eventType=EventType.BarnRaising,       description="Raise the community barn together." },
                new() { eventName="Planting Day",        season=Season.Spring, dayInSeason=14, eventType=EventType.PlantingDay,       description="Sow the seeds for summer harvest." },
                new() { eventName="Harvest Picnic",      season=Season.Summer, dayInSeason=7,  eventType=EventType.HarvestPicnic,    description="Celebrate the early harvest with neighbors." },
                new() { eventName="Courtship Singalong", season=Season.Summer, dayInSeason=21, eventType=EventType.CourtshipSingalong, description="Young folk gather to sing and court." },
                new() { eventName="Quilting Bee",        season=Season.Fall,   dayInSeason=7,  eventType=EventType.QuiltingBee,      description="Pattern quilts at the Miller homestead." },
                new() { eventName="Apple Pressing",      season=Season.Fall,   dayInSeason=14, eventType=EventType.ApplePressing,    description="Press apples for cider and preserves." },
                new() { eventName="Canning Day",         season=Season.Fall,   dayInSeason=21, eventType=EventType.CanningDay,       description="Preserve food for the long winter." },
                new() { eventName="Gmay Service",        season=Season.Winter, dayInSeason=7,  eventType=EventType.GmayService,      description="Bi-weekly church service." },
                new() { eventName="Gmay Service",        season=Season.Winter, dayInSeason=21, eventType=EventType.GmayService,      description="Bi-weekly church service." },
                new() { eventName="Wood Chopping Race",  season=Season.Winter, dayInSeason=14, eventType=EventType.WoodChoppingRace, description="Who can chop the most wood?" },
                new() { eventName="Storytelling Night",  season=Season.Winter, dayInSeason=28, eventType=EventType.StorytellingNight, description="Elders share tales by the fire." },
            };
        }

        private void CheckEventsForDay(int day)
        {
            if (SeasonSystem.Instance == null) return;
            Season season = SeasonSystem.Instance.GetCurrentSeason();
            var events = GetEventsForDay(season, day);
            foreach (var evt in events)
                OnEventStarted?.Invoke(evt);
        }

        public List<CommunityEvent> GetEventsForDay(Season season, int day)
        {
            var result = new List<CommunityEvent>();
            foreach (var evt in _schedule)
                if (evt.season == season && evt.dayInSeason == day)
                    result.Add(evt);
            return result;
        }

        public bool IsEventToday()
        {
            if (TimeSystem.Instance == null || SeasonSystem.Instance == null) return false;
            return GetEventsForDay(SeasonSystem.Instance.GetCurrentSeason(),
                                   TimeSystem.Instance.CurrentDay).Count > 0;
        }

        public CommunityEvent? GetUpcomingEvent()
        {
            if (TimeSystem.Instance == null || SeasonSystem.Instance == null) return null;

            Season currentSeason = SeasonSystem.Instance.GetCurrentSeason();
            int currentDay = TimeSystem.Instance.CurrentDay;

            // Search remainder of current season, then next seasons
            foreach (var evt in _schedule)
            {
                if (evt.season == currentSeason && evt.dayInSeason > currentDay)
                    return evt;
            }

            // Next season(s)
            for (int offset = 1; offset <= 4; offset++)
            {
                Season nextSeason = (Season)(((int)currentSeason + offset) % 4);
                foreach (var evt in _schedule)
                    if (evt.season == nextSeason)
                        return evt;
            }

            return null;
        }
    }
}
