using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{

    public class TimeManager : Singleton<TimeManager>
    {
        public delegate void OnTimeUpHandlerWithParms(int timesquence, params object[] parms);

        public delegate void OnTimeUpHandler(int timesquence);
        public enum enTimeType
        {
            TimeScale,
            NoTimeScale,
        }

        private List<Timer>[] m_timers;
        private int m_timerSquence;
        public override void Init()
        {
            this.m_timers = new List<Timer>[Enum.GetValues(typeof(enTimeType)).Length];
            for (int i = 0; i < m_timers.Length; i++)
            {
                m_timers[i] = new List<Timer>();
            }
            this.m_timerSquence = 0;
        }

        public override void UnInit()
        {
        }

        public int AddTimer(int time, int loop, enTimeType enTimeType, OnTimeUpHandler onTimeUpHandler)
        {
            m_timerSquence++;
            m_timers[enTimeType == enTimeType.NoTimeScale ? 0 : 1].Add(new Timer(m_timerSquence, time, loop, onTimeUpHandler));
            return m_timerSquence;
        }

        public int AddTimer(int time, int loop, enTimeType enTimeType, OnTimeUpHandlerWithParms onTimeUpHandlerWithParms,
            params object[] parms)
        {
            m_timerSquence++;
            m_timers[enTimeType == enTimeType.NoTimeScale ? 0 : 1].Add(new Timer(m_timerSquence, time, loop, onTimeUpHandlerWithParms, parms));
            return m_timerSquence;
        }

        public void RemoveTimer(int squene)
        {
            for (int i = 0; i < m_timers.Length; i++)
            {
                List<Timer> list = m_timers[i];
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].IsSquenceMatch(squene))
                    {
                        list[j].Finish();
                        return;
                    }
                }
            }
        }

        public void RemoveTimer(OnTimeUpHandler ontime, enTimeType enTimeType)
        {
            List<Timer> list = m_timers[enTimeType == enTimeType.NoTimeScale ? 0 : 1];
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].IsDelegateMatch(ontime))
                {
                    list[i].Finish();
                    return;
                }
            }
        }

        public void RemoveTimer(OnTimeUpHandlerWithParms ontimeparm, enTimeType emEnTimeType)
        {
            List<Timer> list = m_timers[emEnTimeType == enTimeType.NoTimeScale ? 0 : 1];
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].IsDelegateWithParmMatch(ontimeparm))
                {
                    list[i].Finish();
                    return;
                }
            }
        }

        public Timer GetTimer(int squen)
        {
            for (int i = 0; i < m_timers.Length; i++)
            {
                List<Timer> list = m_timers[i];
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].IsSquenceMatch(squen))
                    {
                        return list[j];
                    }
                }
            }
            return null;
        }

        public void PauseTimer(int squence)
        {
            for (int i = 0; i < m_timers.Length; i++)
            {
                List<Timer> lise = m_timers[i];
                for (int j = 0; j < lise.Count; j++)
                {
                    if (lise[j].IsSquenceMatch(squence))
                    {
                        lise[j].Pause();
                        return;
                    }
                }
            }
        }

        public void ResumeTimer(int squence)
        {
            Timer timer = GetTimer(squence);
            if (timer != null)
            {
                timer.Resume();
            }
        }

        public void ResetTimer(int squq)
        {
            Timer timer = GetTimer(squq);
            if (timer != null)
            {
                timer.Reset();
            }
        }

        public void ResetTimerTotalTime(int squence, int total)
        {
            Timer timer = GetTimer(squence);
            if (timer != null)
            {
                timer.ResetTotalTime(total);
            }
        }

        public int GetTimerCurrent(int dquen)
        {
            Timer timer = GetTimer(dquen);
            if (timer != null)
            {
                return timer.CurrentTime;
            }
            return -1;
        }

        public int GetLeftTime(int squence)
        {
            Timer timer = GetTimer(squence);
            if (timer != null)
            {
                return timer.GetLeftTime();
            }
            return -1;
        }

        public void RemoveAllTimer()
        {
            for (int i = 0; i < m_timers.Length; i++)
            {
                m_timers[i].Clear();
            }
        }

        public void RemoveAllTimer(enTimeType enTimeType)
        {
            List<Timer> list = m_timers[enTimeType == enTimeType.NoTimeScale ? 0 : 1];
            list.Clear();
        }

        public void UpdateNoTimeScale(int detal, enTimeType enTimeType = enTimeType.NoTimeScale)
        {
            List<Timer> list = m_timers[(int)enTimeType];
            int i = 0;
            while (i < list.Count)
            {
                if (list[i].IsFinish)
                {
                    list.RemoveAt(i);
                }
                else
                {
                    list[i].Update(detal);
                    i++;
                }
            }
        }

        public void Update(int detal, enTimeType enTimeType = enTimeType.TimeScale)
        {
            List<Timer> list = m_timers[(int)enTimeType];
            int i = 0;
            while (i < list.Count)
            {
                if (list[i].IsFinish)
                {
                    list.RemoveAt(i);
                }
                else
                {
                    list[i].Update(detal);
                    i++;
                }
            }
        }

        public void Update()
        {
            Update((int)(Time.deltaTime * 1000));
            UpdateNoTimeScale((int)(Time.unscaledDeltaTime * 1000));
        }
    }

}