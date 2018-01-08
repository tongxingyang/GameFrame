using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    public class Event
    {
        public enEventID EnEventId;
        public stEventParms StEventParms;
        public bool m_isused;

        public void Clear()
        {
            m_isused = false;
            EnEventId = enEventID.None;
        }
    }
}