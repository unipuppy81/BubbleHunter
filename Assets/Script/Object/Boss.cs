using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    [SerializeField] private BubbleManager bubbleManager;

    [Header("Var")]
    private float bossHealth;
    public float curHealth;
    private float damage = 20f;
    private float fillDuration = 1f;

    [SerializeField] private Image healthImage;

    private void Awake()
    {
        bossHealth = 200f;
        curHealth = bossHealth;
        damage = 20.0f;
    }

    private void Update()
    {
        if (curHealth <= 0)
            GameManager.Instance.OnWin();

        healthImage.fillAmount = curHealth / bossHealth;
    }

    public void SpwanBubble()
    {
        GetDamage(damage);
        StartCoroutine(FillCoroutine());
        bubbleManager.SetNewBubble();
    }

    public void GetDamage(float damage)
    {
        curHealth -= damage;
    }

    private IEnumerator FillCoroutine()
    {
        float startFillAmount = curHealth;
        float endFillAmount = Mathf.Max(curHealth - damage, 0f);
        float elapsedTime = 0f;

        while (elapsedTime < fillDuration)
        {
            curHealth = Mathf.Lerp(startFillAmount, endFillAmount, elapsedTime / fillDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        curHealth = endFillAmount;
    }
}
