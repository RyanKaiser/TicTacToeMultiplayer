using System;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossArrowGameObj;
    [SerializeField] private GameObject circleArrowGameObj;
    [SerializeField] private GameObject crossYouTextObj;
    [SerializeField] private GameObject circleYouTextObj;
    [SerializeField] private TextMeshProUGUI crossScoreText;
    [SerializeField] private TextMeshProUGUI circleScoreText;


    private void Awake()
    {
        crossArrowGameObj.SetActive(false);
        circleArrowGameObj.SetActive(false);
        crossYouTextObj.SetActive(false);
        circleYouTextObj.SetActive(false);

        crossScoreText.text = circleScoreText.text = "";
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged += GameManager_OnCurrentPlayerablePlayerTypeChanged;
        // GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnScoreChanged += GameManager_OnScoreChanged;

    }

    private void GameManager_OnScoreChanged(object sender, System.EventArgs e)
    {
        GameManager.Instance.GetScores(out int playerCrossScore, out int playerCircleScore);
        crossScoreText.text = playerCrossScore.ToString();
        circleScoreText.text = playerCircleScore.ToString();
    }

    private void GameManager_OnCurrentPlayerablePlayerTypeChanged(object sender, System.EventArgs e)
    {
        UpdateCurrentArrow();
    }
    private void GameManager_OnGameStarted(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Cross)
        {
            crossYouTextObj.SetActive(true);
        }
        else
        {
            circleYouTextObj.SetActive(true);
        }

        crossScoreText.text = circleScoreText.text = "0";

        UpdateCurrentArrow();
    }

    private void UpdateCurrentArrow()
    {
        bool isCross = GameManager.Instance.GetCurrentPlayablePlayerType() == GameManager.PlayerType.Cross;

        crossArrowGameObj.SetActive(isCross);
        circleArrowGameObj.SetActive(!isCross);
    }
}
