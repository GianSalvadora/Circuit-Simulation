using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
   
    private Vector2Int gridPosition;
    private List<Component> connectedComponents = new List<Component>();

    public Cell(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;
    }

    public void ConnectComponent(Component component)
    {
        foreach (Component establishedConnection in connectedComponents)
        {
            establishedConnection.AddNext(component);
            component.AddNext(establishedConnection);
        }
        connectedComponents.Add(component);
    }
}
