using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Grid
{
    private Vector2Int dimension;
    private int cellSize;
    private Cell[,] gridCells;
    private GridHandler owner;
    
    public Grid(int height, int width, int cellSize, GridHandler owner) {
        dimension.x = width;
        dimension.y = height;
        this.cellSize = cellSize;
        this.owner = owner;

        gridCells = new Cell[width, height];

        for (int x = 0; x < dimension.x; x++)
        {
            for (int y = 0; y < dimension.y; y++)
            {
                gridCells[x, y] = new Cell( new Vector2Int(x, y));
                Debug.DrawLine(new Vector3(x, y, 0), new Vector3(x, y + cellSize, 0), Color.black, Mathf.Infinity);     
                Debug.DrawLine(new Vector3(x, y, 0), new Vector3(x + cellSize, y, 0), Color.black, Mathf.Infinity);  
            }
        }
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        Vector2 worldPosition =  new Vector2(x, y) * cellSize;
        return worldPosition;
    }
    
    public Vector2 GetWorlPositionCentered(int x, int y)
    {
        Vector2 worldPosition =  new Vector2(x, y) * cellSize;
        worldPosition +=  (Vector2.one * cellSize) / 2;
        return worldPosition;
    }

    public Vector2Int GetXYCoordinates(Vector2 worldPos) {
        int x = Mathf.FloorToInt(worldPos.x / cellSize);
        int y = Mathf.FloorToInt(worldPos.y / cellSize);
        return new Vector2Int(x, y);
        
        //needs error handling for outside of grid
    }
    
    public Cell GetCell(Vector2Int gridCoordinate)
    {
        return gridCells[gridCoordinate.x, gridCoordinate.y];
    }
}
