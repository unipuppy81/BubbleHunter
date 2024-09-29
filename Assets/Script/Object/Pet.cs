using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pet : MonoBehaviour
{
    public float maxFillAmount = 100;
    public float curFillAmount;
    private float fillAmountIncrement = 20f;
    private float fillDuration = 1f;

    [SerializeField] private Image box;
    [SerializeField] private BubbleManager bubbleManager;
    [SerializeField] private Transform specialBubbleTransform;
    [SerializeField] private Shoot shoot;

    private void Awake()
    {
        curFillAmount = 0;
    }

    private void Update()
    {
        if (curFillAmount >= maxFillAmount)
            ResetAmount();

        box.fillAmount = curFillAmount / maxFillAmount;
    }

    public void ResetAmount()
    {
        curFillAmount = 0f;

        Bubble newBubble = bubbleManager.GenerateSpecialBubbleTypeShoot();
        shoot.LoadSpecialBubble(newBubble);
    }

    public void ClickBoxBtn()
    {
        shoot.BubbleMoveToBox(specialBubbleTransform);
        StartCoroutine(FillCoroutine());

        GameManager.Instance.OnShootAction();
    }

    private IEnumerator FillCoroutine()
    {
        float startFillAmount = curFillAmount;
        float endFillAmount = Mathf.Min(curFillAmount + fillAmountIncrement, maxFillAmount);
        float elapsedTime = 0f;

        while (elapsedTime < fillDuration)
        {
            curFillAmount = Mathf.Lerp(startFillAmount, endFillAmount, elapsedTime / fillDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        curFillAmount = endFillAmount;
    }
}
