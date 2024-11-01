using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class GridHandler : MonoBehaviour
{
   public Vector2Int gridSize;
   public int cellSize;
   private Grid circuitGrid;

   private void Start()
   {
      circuitGrid = new Grid(gridSize.y, gridSize.x, cellSize, this);
   }
   public Vector2Int GetGridCoordinate(Vector2 worldPosition)
   {
      return circuitGrid.GetXYCoordinates(worldPosition);
   }

   public Vector2 GetWorldPosition(Vector2Int gridPosition)
   {
      return circuitGrid.GetWorlPositionCentered(gridPosition.x, gridPosition.y);
   }
   public Cell GetCell(Vector2Int gridCoordinate)
   {
      return circuitGrid.GetCell(gridCoordinate);
   }
}
