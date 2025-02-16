using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loaerColor;

    private void Start()
    {
        Hide();
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;

    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (e.winPlayerType == GameManager.Instance.GetLocalPlayerType())
        {
            gameOverText.text = "You Win!";
            gameOverText.color = winColor;
        }
        else
        {
            gameOverText.text = "You Loose!";
            gameOverText.color = loaerColor;
        }

        Show();
    }

    void Show()
    {
        gameObject.SetActive(true);
    }

    void Hide()
    {
        gameObject.SetActive(false);
    }
}
