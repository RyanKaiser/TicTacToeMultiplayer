using System;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossArrowGameObj;
    [SerializeField] private GameObject circleArrowGameObj;
    [SerializeField] private GameObject crossYouTextObj;
    [SerializeField] private GameObject circleYouTextObj;

    private void Awake()
    {
        crossArrowGameObj.SetActive(false);
        circleArrowGameObj.SetActive(false);
        crossYouTextObj.SetActive(false);
        circleYouTextObj.SetActive(false);
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged += GameManager_OnCurrentPlayerablePlayerTypeChanged;
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

        UpdateCurrentArrow();
    }

    private void UpdateCurrentArrow()
    {
        bool isCross = GameManager.Instance.GetCurrentPlayablePlayerType() == GameManager.PlayerType.Cross;

        crossArrowGameObj.SetActive(isCross);
        circleArrowGameObj.SetActive(!isCross);
    }
}
