using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ControlManager : MonoBehaviour
{
    [SerializeField] 
    private Shoot shoot;
    private Timer timer;

    public delegate void TouchEvent(Vector3 touchPosition);
    public delegate void DragEvent(Vector3 touchPosition);

    public TouchEvent touchEvent;
    public DragEvent dragEvent;

    private void Start()
    {
        timer = GetComponent<Timer>();
    }

    private void Update()
    {
        OnReceiveController();
    }

    private void OnReceiveController()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (shoot.isShootReady)
            {
                timer.StartTimerUpdateSeconds(0.1f, null, null);
                OnTouch(Input.mousePosition);
            }
        } 
        else
        {
            if (timer.CurrentState == Timer.PlayState.STOP)
            {
               OnDrag(Input.mousePosition);
            }
        }
    }

    public void OnTouch(Vector3 position)
    {
        if (touchEvent != null)
        {
            if(position.y >= 100.0f)
                touchEvent(position);
        }
    }

    public void OnDrag(Vector3 position)
    {
        if (dragEvent != null)
        {
            dragEvent(position);
        }
    }

    #region ¿Ã∫•∆Æ
    public void RegisterEventTouch(TouchEvent teventFunc)
    {
        touchEvent += teventFunc;
    }

    public void RegisterEventDrag(DragEvent deventFunc)
    {
        dragEvent += deventFunc;
    }

    public void UnRegisterEventTouch(TouchEvent teventFunc)
    {
        touchEvent -= teventFunc;
    }
    public void UnRegisterEventDrag(DragEvent deventFunc)
    {
        dragEvent -= deventFunc;
    }
    #endregion 
}

