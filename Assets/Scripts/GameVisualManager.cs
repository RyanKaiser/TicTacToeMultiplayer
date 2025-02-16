using System;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform lineCompletePrefab;
    private const float GRID_SIZE = 3.1f;
    private void Awake()
    {

    }

    private void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        float eularAngle = 0f;
        switch (e.line.Orientation)
        {
            case GameManager.Orientation.Horizontal:
                eularAngle = 0f;
                break;
            case GameManager.Orientation.Vertical :
                eularAngle = 90f;
                break;
            case GameManager.Orientation.DiagonalA:
                eularAngle = 45f;
                break;
            case GameManager.Orientation.DiagonalB:
                eularAngle = -45f;
                break;
        }
        Transform lineCompleteTransform =  Instantiate(
            lineCompletePrefab,
            GetGridWorldPosition(e.line.CenterGridPosition.x, e.line.CenterGridPosition.y),
            Quaternion.Euler(0, 0, eularAngle));
        lineCompleteTransform.GetComponent<NetworkObject>().Spawn(true);

    }

    private void GameManager_OnClickedOnGridPosition(object sender, GameManager.OnClickedOnGridPositionEventArgs e)
    {
        Debug.Log("GameManager_OnClickedOnGridPosition");

        SpawnObjectRpc(e.x, e.y, e.playerType);
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType)
    {
        Debug.Log("SpawnObject");
        Transform prefab = playerType == GameManager.PlayerType.Circle
            ? circlePrefab
            : crossPrefab;

        Transform spawnedCrossTransform = Instantiate(prefab, GetGridWorldPosition(x, y), Quaternion.identity);
        spawnedCrossTransform.GetComponent<NetworkObject>().Spawn(true);
        // spawnedCrossTransform.position = GetGridWorldPosition(x, y);
    }

    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }

}
