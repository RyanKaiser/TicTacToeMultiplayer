using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loaerColor;
    [SerializeField] private Button rematchButton;

    private void Awake()
    {
        rematchButton.onClick.AddListener(() => GameManager.Instance.RematchRpc());
    }
    private void Start()
    {
        Hide();
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnGameRematch += GameManager_OnGameRematch;
    }

    private void GameManager_OnGameRematch(object sender, System.EventArgs e)
    {
        Hide();

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
