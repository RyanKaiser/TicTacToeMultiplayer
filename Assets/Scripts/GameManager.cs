using System;
using Unity.Mathematics;
using UnityEngine;

public class GameManager : MonoBehaviour
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
   }


   private void Awake()
   {
      if (_instance == null)
      {
         _instance = this;
      }

   }

   public void ClickedOnGridPosition(int x, int y)
   {
      Debug.Log($"ClickedOnGridPosition {x}, {y}");
      OnClickedOnGridPosition.Invoke(this, new OnClickedOnGridPositionEventArgs
      {
         x = x,
         y = y,
      });
   }

}
