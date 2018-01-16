using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIFrameWork
{
	public delegate void OnTouchEventHandle(GameObject _sender, object _args, params object[]_objects);
	public enum EnumTouchEventType
	{
		OnClick,
		OnDoubleClick,
		OnDown,
		OnUp,
		OnEnter,
		OnExit,
		OnSelect,  
		OnUpdateSelect,  
		OnDeSelect, 
		OnDrag, 
		OnDragEnd,
		OnDrop,
		OnScroll, 
		OnMove,
	}
    public class TouchHandle
    {
        private event OnTouchEventHandle eventHandle = null;
        /// <summary>
        /// 点击附加的额外参数
        /// </summary>
        private object[] handleParams;
        public TouchHandle()
        {
            DestoryHandle();
        }
        /// <summary>
        /// 删除所有的委托
        /// </summary>
        public void DestoryHandle()
        {
            if (null != eventHandle)
            {
                eventHandle -= eventHandle;
                eventHandle = null;
            }
        }

        public void SetHandle(OnTouchEventHandle handle, params object[] _params)
        {
            eventHandle += handle;
            handleParams = _params;
        }

        public void CallEventHandle(GameObject go,object args)
        {
            if (null != eventHandle)
            {
                eventHandle(go, args, handleParams);
            }
        }
    }
    public class EventTriggerListener : MonoBehaviour,
    IPointerClickHandler,
    IPointerDownHandler,  
    IPointerUpHandler, 
    IPointerEnterHandler,  
    IPointerExitHandler,  
 
    ISelectHandler,  
    IUpdateSelectedHandler,  
    IDeselectHandler, 

    IDragHandler,  
    IEndDragHandler,  
    IDropHandler,  
    IScrollHandler,  
    IMoveHandler  
    {
        //定义相关点击事件
        public TouchHandle onClick;  
        public TouchHandle onDoubleClick; 
        public TouchHandle onDown;  
        public TouchHandle onEnter;  
        public TouchHandle onExit;  
        public TouchHandle onUp;  
        public TouchHandle onSelect;  
        public TouchHandle onUpdateSelect;  
        public TouchHandle onDeSelect;  
        public TouchHandle onDrag;  
        public TouchHandle onDragEnd;  
        public TouchHandle onDrop;  
        public TouchHandle onScroll;  
        public TouchHandle onMove;

        public static EventTriggerListener Get(GameObject go)
        {
            EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
            if (listener == null)
            {
                listener = go.AddComponent<EventTriggerListener>();
            }
            return listener;
        }

        void OnDestroy()
        {
            RemoveHandle (onClick);
            RemoveHandle (onDoubleClick);
            RemoveHandle (onDown);
            RemoveHandle (onEnter);
            RemoveHandle (onExit);
            RemoveHandle (onUp);
            RemoveHandle (onDrop);
            RemoveHandle (onDrag);
            RemoveHandle (onDragEnd);
            RemoveHandle (onScroll);
            RemoveHandle (onMove);
            RemoveHandle (onUpdateSelect);
            RemoveHandle (onSelect);
            RemoveHandle (onDeSelect);
        }

        private void RemoveHandle(TouchHandle handle)
        {
            if (null != handle)
            {
                handle.DestoryHandle();
                handle = null;
            }
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (onClick != null)
            {
                onClick.CallEventHandle(this.gameObject,eventData);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (onDown != null)
            {
                onDown.CallEventHandle(this.gameObject,eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (onUp != null)
            {
                onUp.CallEventHandle(this.gameObject,eventData);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (onEnter != null)
            {
                onEnter.CallEventHandle(this.gameObject,eventData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (onExit != null)
            {
                onExit.CallEventHandle(this.gameObject,eventData);
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (onSelect != null)
            {
                onSelect.CallEventHandle(this.gameObject,eventData);
            }
        }

        public void OnUpdateSelected(BaseEventData eventData)
        {
            if (onUpdateSelect != null)
            {
                onUpdateSelect.CallEventHandle(this.gameObject,eventData);
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (onDeSelect != null)
            {
                onDeSelect.CallEventHandle(this.gameObject,eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (onDrag != null)
            {
                onDrag.CallEventHandle(this.gameObject,eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (onDragEnd != null)
            {
                onDragEnd.CallEventHandle(this.gameObject,eventData);
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (onDrop != null)
            {
                onDrop.CallEventHandle(this.gameObject,eventData);
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (onScroll != null)
            {
                onScroll.CallEventHandle(this.gameObject,eventData);
            }
        }

        public void OnMove(AxisEventData eventData)
        {
            if (onMove != null)
            {
                onMove.CallEventHandle(this.gameObject,eventData);
            }
        }

        public void SetEventHandle(EnumTouchEventType type,OnTouchEventHandle _handle,params object[] _params)
        {
            switch (type)
            {
                case EnumTouchEventType.OnClick:
				if (null == onClick) {
					onClick = new TouchHandle();
				}
				onClick.SetHandle(_handle, _params);
				break;
			case EnumTouchEventType.OnDoubleClick:
				if (null == onDoubleClick) {
					onDoubleClick = new TouchHandle();
				}
				onDoubleClick.SetHandle(_handle, _params);
				break;
			case EnumTouchEventType.OnDown:
				if (onDown == null) {
					onDown = new TouchHandle();
				}
				onDown.SetHandle (_handle, _params);
				break;
			case EnumTouchEventType.OnUp:
				if (onUp == null) {
					onUp = new TouchHandle();
				}
				onUp.SetHandle (_handle, _params);
				break;
			case EnumTouchEventType.OnEnter:
				if (onEnter == null) {
					onEnter = new TouchHandle();
				}
				onEnter.SetHandle (_handle, _params);
				break;
			case EnumTouchEventType.OnExit:
				if (onExit == null) {
					onExit = new TouchHandle();
				}
				onExit.SetHandle (_handle, _params);
				break;
			case EnumTouchEventType.OnDrag:
				if (onDrag == null) {
					onDrag = new TouchHandle();
				}
				onDrag.SetHandle (_handle, _params);
				break;
			case EnumTouchEventType.OnDrop:
				if (onDrop == null) {
					onDrop = new TouchHandle();
				}
				onDrop.SetHandle (_handle, _params);
				break;

			case EnumTouchEventType.OnDragEnd:
				if (onDragEnd == null)
				{
					onDragEnd = new TouchHandle();
				}
				onDragEnd.SetHandle(_handle, _params);
				break;
			case EnumTouchEventType.OnSelect:
				if (onSelect == null)
				{
					onSelect = new TouchHandle();
				}
				onSelect.SetHandle(_handle, _params);
				break;
			case EnumTouchEventType.OnUpdateSelect:
				if (onUpdateSelect == null)
				{
					onUpdateSelect = new TouchHandle();
				}
				onUpdateSelect.SetHandle(_handle, _params);
				break;
			case EnumTouchEventType.OnDeSelect:
				if (onDeSelect == null)
				{
					onDeSelect = new TouchHandle();
				}
				onDeSelect.SetHandle(_handle, _params);
				break;
			case EnumTouchEventType.OnScroll:
				if (onScroll == null)
				{
					onScroll = new TouchHandle();
				}
				onScroll.SetHandle(_handle, _params);
				break;
			case EnumTouchEventType.OnMove:
				if (onMove == null)
				{
					onMove = new TouchHandle();
				}
				onMove.SetHandle(_handle, _params);
				break; 
            }
        }
    }  
}

