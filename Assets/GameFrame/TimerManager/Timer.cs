using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace GameFrame
{
    public class Timer
    {
        private int m_squence;
        private object[] parms;
        private TimeManager.OnTimeUpHandler OnTimeUpHandler = null;
        private TimeManager.OnTimeUpHandlerWithParms OnTimeUpHandlerWithParms = null;
        /// <summary>
        /// 循环次数
        /// </summary>
        private int m_loop = 1;
        /// <summary>
        /// 总时间
        /// </summary>
        private int m_totalTime;
        /// <summary>
        /// 当前时间
        /// </summary>
        private int m_currentTime;
        /// <summary>
        /// 结束标记
        /// </summary>
        private bool m_finish;
        /// <summary>
        /// 运行标志
        /// </summary>
        private bool m_isrunning;

        public int CurrentTime
        {
            get { return m_currentTime; }
        }

        public int Squence
        {
            get { return m_squence; }
        }

        public bool IsFinish
        {
            get { return m_finish; }
        }

        public bool IsRunning
        {
            get { return m_isrunning; }
        }

        public int TotalTime
        {
            get { return m_totalTime; }
        }
        /// <summary>
        /// 获取剩余时间
        /// </summary>
        /// <returns></returns>
        public int GetLeftTime()
        {
            return m_totalTime - m_currentTime;
        }

        public void Finish()
        {
            m_finish = true;
        }

        public void Pause()
        {
            m_isrunning = false;
        }

        public void Reset()
        {
            m_currentTime = 0;
        }

        public void Resume()
        {
            m_isrunning = true;
        }

        public void ResetTotalTime(int totalname)
        {
            if (m_totalTime == totalname)
            {
                return;
            }
            m_totalTime = totalname;
            m_currentTime = 0;
        }

        public bool IsSquenceMatch(int sque)
        {
            if (m_squence == sque)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsDelegateMatch(TimeManager.OnTimeUpHandler ontime)
        {
            if (OnTimeUpHandler == ontime)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsDelegateWithParmMatch(TimeManager.OnTimeUpHandlerWithParms ontimeparm)
        {
            if (OnTimeUpHandlerWithParms == ontimeparm)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Update(int deteltime)
        {
            if (this.m_finish || !this.m_isrunning)
            {
                return;
            }
            if (m_loop == 0)
            {
                m_finish = true;
            }
            else
            {
                m_currentTime += deteltime;
                if (m_currentTime >= m_totalTime)
                {
                    if (OnTimeUpHandler != null)
                    {
                        OnTimeUpHandler(m_squence);
                    }
                    if (OnTimeUpHandlerWithParms != null)
                    {
                        OnTimeUpHandlerWithParms(m_squence, parms);
                    }
                    m_currentTime = 0;
                    m_loop--;
                }
            }

        }

        public Timer(int sqeune, int time, int loop, TimeManager.OnTimeUpHandler onTimeUpHandler)
        {
            if (loop == 0)
            {
                loop = -1;
            }
            m_squence = sqeune;
            m_totalTime = time;
            m_loop = loop;
            OnTimeUpHandler = onTimeUpHandler;
            m_finish = false;
            m_isrunning = true;
        }

        public Timer(int squence, int time, int loop, TimeManager.OnTimeUpHandlerWithParms onTimeUpHandlerWithParms,
            params object[] parms)
        {
            if (loop == 0)
            {
                loop = -1;
            }
            m_squence = squence;
            m_totalTime = time;
            m_loop = loop;
            this.OnTimeUpHandlerWithParms = onTimeUpHandlerWithParms;
            this.parms = parms;
            m_finish = false;
            m_isrunning = true;
        }

        public void Clear()
        {
            m_squence = -1;
            parms = null;
            OnTimeUpHandler = null;
           OnTimeUpHandlerWithParms = null;
    }
    }

}