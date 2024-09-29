using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI centerText;
    public TextMeshProUGUI score;

    public void DisplayGameOver()
    {
        centerText.gameObject.SetActive(true);
        centerText.text = "Game Over";
    }

    public void DisplayWin()
    {
        centerText.gameObject.SetActive(true);
        centerText.text = "Win";
    }

    public void UpdateScore(int score)
    {
        this.score.text = score.ToString();
    }

    public void DisableText()
    {
        centerText.gameObject.SetActive(false);
    }
}