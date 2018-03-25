using System.Collections;
using System.Collections.Generic;
using UIFrameWork;
using UnityEngine;

public delegate void UIStateChangeEvent(object sender, UIState newState, UIState oldState);

public delegate void OnTouchEventHandle(GameObject _sender, object _args, params object[]_objects);