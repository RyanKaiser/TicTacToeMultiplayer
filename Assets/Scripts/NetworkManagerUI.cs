using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button _hostSelectButton;
    [SerializeField] private Button _clientSelectButton;

    private void Awake()
    {
        Debug.Log("NetworkManagerUI Awake()");

        _hostSelectButton.onClick.AddListener(() =>
        {
            Debug.Log("Hosting Game...");
            NetworkManager.Singleton.StartHost();
            Hide();
        });
        _clientSelectButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
