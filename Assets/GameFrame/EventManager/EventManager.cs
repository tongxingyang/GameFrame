using System;
using System.Collections;
using System.Collections.Generic;
using GameFrame;
using UnityEngine;

namespace GameFrame
{
    public class EventManager : Singleton<EventManager>
    {
        public delegate void OnEventHandler(Event @event);
        private List<object> m_events = null;
        private OnEventHandler[] m_eventMap = null;
        public override void Init()
        {
            m_events = new List<object>();
            m_eventMap = new OnEventHandler[Enum.GetValues(typeof(enEventID)).Length];
        }

        public bool HasEventListener(enEventID enEventId)
        {
            return m_eventMap[(int)enEventId] == null;
        }

        public void AddEventListener(enEventID enEventId, OnEventHandler onEventHandler)
        {
            if (m_eventMap[(int)enEventId] == null)
            {
                m_eventMap[(int)enEventId] = delegate { };
                m_eventMap[(int)enEventId] =
                    (OnEventHandler)Delegate.Combine(m_eventMap[(int)enEventId], onEventHandler);
            }
            else
            {
                //加之前先移除 防止一个方法执行多次
                m_eventMap[(int)enEventId] =
                    (OnEventHandler)Delegate.Remove(m_eventMap[(int)enEventId], onEventHandler);
                m_eventMap[(int)enEventId] =
                    (OnEventHandler)Delegate.Combine(m_eventMap[(int)enEventId], onEventHandler);

            }
        }

        public void RemoveEventListener(enEventID enEventId, OnEventHandler onEventHandler)
        {
            if (m_eventMap[(int)enEventId] != null)
            {
                m_eventMap[(int)enEventId] = (OnEventHandler)Delegate.Remove(m_eventMap[(int)enEventId], onEventHandler);
            }
        }

        public void DispathEvent(Event @event)
        {
            @event.m_isused = true;
            OnEventHandler callback = m_eventMap[(int)@event.EnEventId];
            if (callback != null)
            {
                callback(@event);
            }
            @event.Clear();
        }

        public void DispathEvent(enEventID eventId)
        {
            Event @event = GetEvent();
            @event.EnEventId = eventId;
            DispathEvent(@event);
        }

        public void DispathEvent(enEventID enEventId, stEventParms stEventParms)
        {
            Event @event = GetEvent();
            @event.EnEventId = enEventId;
            @event.StEventParms = stEventParms;
            DispathEvent(@event);
        }

        public Event GetEvent()
        {
            for (int i = 0; i < m_events.Count; i++)
            {
                if (((Event)m_events[i]).m_isused == false)
                {
                    return (Event)m_events[i];
                }
            }
            Event @event = new Event();
            m_events.Add(@event);
            return @event;
        }

        public void ClearAllEvents()
        {
            m_events = null;
            this.m_eventMap = null;
        }
    }

}