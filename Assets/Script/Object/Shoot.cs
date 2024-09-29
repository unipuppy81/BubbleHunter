using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    private bool isBlock;
    public bool isShootReady;
    public float force; 

    public Transform bubbleTransform; 
    public Transform nextBubbleTransform; 
    public Transform bubblesRoot;
    public Transform boxTransform;

    private Bubble bubble;
    private Bubble nextBubble;

    public ControlManager controller;
    private Timer timer;


    private void Start()
    {
        RegisterEvents();
        timer = GetComponent<Timer>(); 
    }

    #region 세팅
    private void RegisterEvents()
    {
        controller.RegisterEventTouch(ShootBubble);
        controller.RegisterEventDrag(RotateArrow);
    }

    private void RotateArrow(Vector3 pos)
    {
        if (!isBlock)
        {
            Vector3 direction = pos - transform.position;
            if (Vector3.Angle(Vector3.up, direction) < 60)
                transform.up = direction;
        }
    }

    public void ResetGunDirection()
    {
        transform.up = Vector3.up;
    }

    public void ClickSwapBtn()
    {
        isShootReady = false;

        LoadShootBubble(bubble);
    }

    public void ClearBullets()
    {
        Destroy(bubble.gameObject);
        Destroy(nextBubble.gameObject);
    }

    public void BlockArrow()
    {
        isBlock = true;
    }

    public void UnBlockArrow()
    {
        isBlock = false;
    }
    #endregion


    #region 발사 & 장전
    private void ShootBubble(Vector3 pos)
    {
        if (isShootReady && !isBlock)
        {
            Vector3 direction = transform.up;
            Vector3 _force = direction.normalized * force * 1000;

            bubble.BubbleShooted(bubblesRoot, _force);
            isShootReady = false;
            GameManager.Instance.OnShootAction();

            GameManager.Instance.LifeMinus();
        }
    }

    public void LoadShootBubble(Bubble newBubble)
    {
        if (timer.CurrentState == Timer.PlayState.STOP)
        {
            timer.StartTimerUpdatePercentage(0.1f, () =>
            {
                LoadDoneBubble(nextBubble, newBubble);
            }, null);
        }
    }

    public void LoadDoneBubble(Bubble first, Bubble second)
    {
        bubble = first;
        nextBubble = second;
        bubble.transform.parent = bubbleTransform;
        bubble.transform.localPosition = Vector3.zero;

        GameManager.Instance.SetLineColor(first.GetBubbleRealColor());

        nextBubble.transform.parent = nextBubbleTransform;
        nextBubble.transform.localPosition = Vector3.zero;
        isShootReady = true;
    }

    public void LoadSpecialBubble(Bubble sBubble)
    {
        if (timer.CurrentState == Timer.PlayState.STOP)
        {
            timer.StartTimerUpdatePercentage(0.1f, () =>
            {
                Destroy(nextBubble);
                LoadDoneBubble(sBubble, bubble);
            }, null);
        }
    }

    public void BubbleMoveToBox(Transform boxTransform)
    {
        bubble.MoveToBox(boxTransform);
        bubble.transform.parent = boxTransform;

        StartCoroutine(DestroyAfterDelay(bubble.gameObject));
    }

    IEnumerator DestroyAfterDelay(GameObject obj)
    {
        yield return new WaitForSeconds(1f);

        Destroy(obj);
    }
    #endregion
}
