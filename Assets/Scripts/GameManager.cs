using System;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{

   private static GameManager _instance;
   public static GameManager Instance
   {
      get
      {
         if (_instance == null)
         {
            return FindObjectOfType<GameManager>();
         }

         return _instance;
      }
   }

   public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
   public class OnClickedOnGridPositionEventArgs : EventArgs
   {
      public int x;
      public int y;
      public PlayerType playerType;
   }

   public event EventHandler OnGameStarted;
   public event EventHandler OnCurrentPlayablePlayerTypeChanged;

   public enum PlayerType
   {
      None,
      Circle,
      Cross,
   }

   private PlayerType _localPlayerType;
   private NetworkVariable<PlayerType> _currentPlayablePlayerType = new NetworkVariable<PlayerType>();
   private PlayerType[,] playerTypeArray;


   private void Awake()
   {
      if (_instance == null)
      {
         _instance = this;
      }

      playerTypeArray = new PlayerType[3, 3];
   }

   public override void OnNetworkSpawn()
   {
      Debug.Log($"Network Id: {NetworkManager.Singleton.LocalClientId}");
      if (NetworkManager.Singleton.LocalClientId == 0)
      {
         _localPlayerType = PlayerType.Cross;
      }
      else
      {
         _localPlayerType = PlayerType.Circle;
      }

      if (IsServer)
      {
         NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallBack;
      }

      _currentPlayablePlayerType.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) =>
      {
         OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
      };


   }

   private void NetworkManager_OnClientConnectedCallBack(ulong obj)
   {
      if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
      {
         _currentPlayablePlayerType.Value = PlayerType.Cross;

         // OnGameStarted?.Invoke(this, EventArgs.Empty); ==>
         TriggerOnGameStartRpc();
      }
   }

   [Rpc(SendTo.ClientsAndHost)]
   private void TriggerOnGameStartRpc()
   {
      OnGameStarted?.Invoke(this, EventArgs.Empty);
   }

   [Rpc(SendTo.Server)]
   public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
   {
      Debug.Log($"ClickedOnGridPosition ({x}, {y}), LocalPlayerType = {GetLocalPlayerType()}, CurrentTurn = {_currentPlayablePlayerType}");
      if (playerType != _currentPlayablePlayerType.Value)
         return;

      if (playerTypeArray[x, y] != PlayerType.None)
         return;
      playerTypeArray[x, y] = playerType;

      OnClickedOnGridPosition.Invoke(this, new OnClickedOnGridPositionEventArgs
      {
         x = x,
         y = y,
         playerType = playerType
      });

      switch (_currentPlayablePlayerType.Value)
      {
         default:
         case PlayerType.Cross:
            _currentPlayablePlayerType.Value = PlayerType.Circle;
            break;

         case PlayerType.Circle:
              _currentPlayablePlayerType.Value = PlayerType.Cross;
               break;
      }

      //this is called only on server
      //OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty); =>
      // TriggerOnCurrentPlayablePlayerTypeChangedRpc();
   }

   // [Rpc(SendTo.ClientsAndHost)]
   // private void TriggerOnCurrentPlayablePlayerTypeChangedRpc()
   // {
   //    OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
   // }

   public PlayerType GetLocalPlayerType()
   {
      return _localPlayerType;
   }

   public PlayerType GetCurrentPlayablePlayerType()
   {
      return _currentPlayablePlayerType.Value;
   }
}
