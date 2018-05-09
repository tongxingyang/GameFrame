using System.Collections;
using System.Collections.Generic;
using UIFrameWork;
using UnityEngine;

namespace UIFrameWork
{
    public delegate void WindowStateChangeEvent(object sender, enWindowState newState, enWindowState oldState);
    public delegate void OnWindowSorted(List<WindowBase> inForms);
}
