using System;
using System.Collections.Generic;
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
   public event EventHandler<OnGameWinEventArgs> OnGameWin;

   public class OnGameWinEventArgs : EventArgs
   {
      public Line line;
      public PlayerType winPlayerType;
   }
   public event EventHandler OnCurrentPlayablePlayerTypeChanged;
   public event EventHandler OnGameRematch;
   public event EventHandler OnGameTie;
   public event EventHandler OnScoreChanged;
   public event EventHandler OnPlacedObject;

   public enum PlayerType
   {
      None,
      Circle,
      Cross,
   }

   public enum Orientation
   {
      Horizontal,
      Vertical,
      DiagonalA,
      DiagonalB,
   }

   public struct Line
   {
      public List<Vector2Int> GridVector2IntList { get; set; }
      public Vector2Int CenterGridPosition { get; set; }
      public Orientation Orientation { get; set; }
   }

   private PlayerType _localPlayerType;
   private NetworkVariable<PlayerType> _currentPlayablePlayerType = new NetworkVariable<PlayerType>();
   private PlayerType[,] playerTypeArray;
   private List<Line> lineList;
   private NetworkVariable<int> _playerCrossScore = new NetworkVariable<int>();
   private NetworkVariable<int> _playerCircleScore = new NetworkVariable<int>();


   private void Awake()
   {
      if (_instance == null)
      {
         _instance = this;
      }

      playerTypeArray = new PlayerType[3, 3];
      lineList = new List<Line>
      {
         //horizontal
         new Line
         {
            GridVector2IntList = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0)},
            CenterGridPosition = new Vector2Int(1, 0),
            Orientation = Orientation.Horizontal
         },
         new Line
         {
            GridVector2IntList = new List<Vector2Int> { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1)},
            CenterGridPosition = new Vector2Int(1, 1),
            Orientation = Orientation.Horizontal
         },
         new Line
         {
            GridVector2IntList = new List<Vector2Int> { new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2)},
            CenterGridPosition = new Vector2Int(1, 2),
            Orientation = Orientation.Horizontal
         },

         //vertical
         new Line
         {
            GridVector2IntList = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2)},
            CenterGridPosition = new Vector2Int(0, 1),
            Orientation = Orientation.Vertical
         },
         new Line
         {
            GridVector2IntList = new List<Vector2Int> { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2)},
            CenterGridPosition = new Vector2Int(1, 1),
            Orientation = Orientation.Vertical
         },
         new Line
         {
            GridVector2IntList = new List<Vector2Int> { new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2)},
            CenterGridPosition = new Vector2Int(2, 1),
            Orientation = Orientation.Vertical
         },
         //diagonal
         new Line
         {
            GridVector2IntList = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2)},
            CenterGridPosition = new Vector2Int(1, 1),
            Orientation = Orientation.DiagonalA
         },
         new Line
         {
            GridVector2IntList = new List<Vector2Int> { new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0)},
            CenterGridPosition = new Vector2Int(1, 1),
            Orientation = Orientation.DiagonalB
         }
      };
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

      _playerCrossScore.OnValueChanged += (int prevScore, int newScore) =>
      {
         OnScoreChanged?.Invoke(this, EventArgs.Empty);
      };
      _playerCircleScore.OnValueChanged += (int prevScore, int newScore) =>
      {
         OnScoreChanged?.Invoke(this, EventArgs.Empty);
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
      TriggerOnPlacedObjectRpc();

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

      TestWinner();
   }

   [Rpc(SendTo.ClientsAndHost)]
   private void TriggerOnPlacedObjectRpc()
   {
      OnPlacedObject?.Invoke(this, EventArgs.Empty);
   }

   [Rpc(SendTo.ClientsAndHost)]
   private void TriggerOnGameWinRpc(int lineIndex, PlayerType winPlayerType)
   {
      Line line = lineList[lineIndex];
      OnGameWin?.Invoke(this, new OnGameWinEventArgs
      {
         line = line,
         winPlayerType = winPlayerType//[line.CenterGridPosition.x, line.CenterGridPosition.y]
      });
   }

   [Rpc(SendTo.Server)]
   public void RematchRpc()
   {
      Debug.Log("RematchRpc() called");
      //reset playertype array
      for (int i = 0; i < playerTypeArray.GetLength(0); i++)
         for (int j = 0; j < playerTypeArray.GetLength(1); j++)
            playerTypeArray[i, j] = PlayerType.None;

      _currentPlayablePlayerType.Value = PlayerType.Cross;
      TriggerOnRematchRpc();
   }

   [Rpc(SendTo.ClientsAndHost)]
   private void TriggerOnRematchRpc()
   {
      OnGameRematch?.Invoke(this, EventArgs.Empty);
   }

   [Rpc(SendTo.ClientsAndHost)]
   private void TriggerOnGameTieRpc()
   {
      OnGameTie?.Invoke(this, EventArgs.Empty);
   }

   public PlayerType GetLocalPlayerType()
   {
      return _localPlayerType;
   }

   public PlayerType GetCurrentPlayablePlayerType()
   {
      return _currentPlayablePlayerType.Value;
   }


   private bool TestWinnerLine(Line line)
   {
      return TestWinnerLine(
         playerTypeArray[line.GridVector2IntList[0].x, line.GridVector2IntList[0].y],
         playerTypeArray[line.GridVector2IntList[1].x, line.GridVector2IntList[1].y],
         playerTypeArray[line.GridVector2IntList[2].x, line.GridVector2IntList[2].y]
         );
   }
   private bool TestWinnerLine(PlayerType a, PlayerType b, PlayerType c)
   {
      return a != PlayerType.None &&
             a == b &&
             b == c;
   }

   private void TestWinner()
   {
      // foreach(var line in lineList)
      for (int i = 0; i < lineList.Count; i++)
      {
         Line line = lineList[i];
         if (TestWinnerLine(line))
         {
            Debug.Log("Winner!");
            _currentPlayablePlayerType.Value = PlayerType.None;
            PlayerType winner = playerTypeArray[line.CenterGridPosition.x, line.CenterGridPosition.y];
            switch (winner)
            {
               case PlayerType.Circle: _playerCircleScore.Value++; break;
               case PlayerType.Cross: _playerCrossScore.Value++; break;
            }

            TriggerOnGameWinRpc(i, winner);
            return;
         }
      }

      //Tie
      for (int i = 0; i < playerTypeArray.GetLength(0); i++)
         for (int j = 0; j < playerTypeArray.GetLength(1); j++)
            if (playerTypeArray[i,j] == PlayerType.None)
               return;

      TriggerOnGameTieRpc();
   }

   public void GetScores(out int crossScore, out int circleScore)
   {
      crossScore = _playerCrossScore.Value;
      circleScore = _playerCircleScore.Value;
   }

}
