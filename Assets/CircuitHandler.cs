    using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class CircuitHandler : MonoBehaviour
{
    private GridHandler gridHandler;
    private bool mouseDown = false;

    private GameObject currentComponent = null;
    private Vector2 componentStartPosition;
    
    public List<Component> allComponents;

    private Component.ComponentType _componentType = Component.ComponentType.Wire;
    private void Start()
    {
        gridHandler = FindObjectOfType<GridHandler>().GetComponent<GridHandler>();
    }

    private void Update()
    {
        CheckType();
        if (Input.GetKeyDown(KeyCode.S))
        {
            List<List<Component>> seriesLoop = LoopFinder();
            if (seriesLoop.Count > 1)
            {
                Debug.Log("Not a series Connection MAYBE IN THE FUTURE");
                return;
            }
            else
            {
                // Is a series circuit
                float totalVoltage = 0;
                float totalResistance = 0;
                List<Component> resistors = new List<Component>();

                // Calculate total resistance and voltage
                foreach (Component component in seriesLoop[0])
                {
                    if (component.CType == Component.ComponentType.Resistor)
                    {
                        totalResistance += component.resistance;
                        resistors.Add(component);
                    }
                    else if (component.CType == Component.ComponentType.VoltageSource)
                    {
                        if (component.isReversed)
                        {
                            totalVoltage -= component.voltage;
                            continue;
                        }
                        totalVoltage += component.voltage;
                    }
                }

                totalVoltage = Mathf.Abs(totalVoltage);
                Debug.Log("RT: " + totalResistance);
                Debug.Log("VT: " + totalVoltage);

                // Calculate the total current in the series circuit
                float totalCurrent = totalVoltage / totalResistance;
                Debug.Log("IT: " + totalCurrent);

                // Name resistors starting from 1 and calculate/debug their voltage drops
                for (int i = 0; i < resistors.Count; i++)
                {
                    resistors[i].current = totalCurrent;
                    float resistorVoltageDrop = totalCurrent * resistors[i].resistance; // Ohm's law V = IR
                    resistors[i].voltage = totalVoltage;
                    Debug.Log($"Resistor R{i + 1} Vx: {resistorVoltageDrop}");
                }
            }

        }
        
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0) && !mouseDown) //when dragging starts
        {
            mouseDown = true;
            Vector2Int gridCoordinate = gridHandler.GetGridCoordinate(mousePosition);
            Vector2 gridPosition = gridHandler.GetWorldPosition(gridCoordinate);
            componentStartPosition = gridPosition;
            if (_componentType == Component.ComponentType.Wire)
            {
                currentComponent = Instantiate(ReferenceHandler.singleton.wire, gridPosition, quaternion.identity);
            }else if (_componentType == Component.ComponentType.Resistor)
            {
                currentComponent = Instantiate(ReferenceHandler.singleton.resistor, gridPosition, quaternion.identity);
            }
            else if(_componentType == Component.ComponentType.VoltageSource)
            {
                currentComponent = Instantiate(ReferenceHandler.singleton.battery, gridPosition, quaternion.identity);
            }
            currentComponent.name = allComponents.Count().ToString();
            allComponents.Add(currentComponent.GetComponent<Component>());
            
            
            //addCell to the connectedComponents
            gridHandler.GetCell(gridCoordinate).ConnectComponent(currentComponent.GetComponent<Component>());
        }   

        if (currentComponent)
        {
            //set rotation
            Vector2 direction =  gridHandler.GetWorldPosition(gridHandler.GetGridCoordinate(mousePosition)) - componentStartPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            currentComponent.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            
            //set length
            float componentLength = Vector2.Distance(componentStartPosition, gridHandler.GetWorldPosition(gridHandler.GetGridCoordinate(mousePosition)));
            currentComponent.transform.localScale = new Vector3(componentLength, 1, 1);
        }

        if (Input.GetMouseButtonUp(0) && mouseDown)
        {
            Vector2Int gridCoordinate = gridHandler.GetGridCoordinate(mousePosition);
            gridHandler.GetCell(gridCoordinate).ConnectComponent(currentComponent.GetComponent<Component>());
            currentComponent = null;
            mouseDown = false;
        }
    }

    
    
    
    public List<List<Component>> LoopFinder()
{
    // Get all valid loops first   
    List<List<Component>> allLoops = new List<List<Component>>();
    foreach (Component component in allComponents)
    {
        foreach (List<Component> loops in GetLoops(component, null, new List<Component>()))
        {
            allLoops.Add(loops);
        }
    }

    // Get mesh loops
    List<List<Component>> meshLoops = FilterMeshLoops(allLoops);

    // Print all loops and mesh loops
    PrintLoops(allLoops, meshLoops);

    return meshLoops;
}
    //ARE YOU READING THIS XANDER FK YUOU
    private List<List<Component>> FilterMeshLoops(List<List<Component>> allLoops)
    {
    var meshLoops = new List<List<Component>>();
    
    // Sort loops by size (prefer smaller loops for mesh analysis)
    var sortedLoops = allLoops.OrderBy(loop => loop.Count).ToList();
    
    foreach (var loop in sortedLoops)
    {
        if (IsSuitableForMeshAnalysis(loop, meshLoops))
        {
            meshLoops.Add(loop);
        }
    }
    
    return meshLoops;
}

    private bool IsSuitableForMeshAnalysis(List<Component> candidateLoop, List<List<Component>> existingMeshes)
{
    // First loop is always suitable
    if (existingMeshes.Count == 0)
    {
        return true;
    }
    
    // Check relationships with existing mesh loops
    foreach (var existingMesh in existingMeshes)
    {
        // Find shared components between loops
        var sharedComponents = candidateLoop.Intersect(existingMesh).ToList();
        
        // If loops share more than 2 components, this isn't a basic mesh
        // (they should only share 1-2 components if they're adjacent meshes)
        if (sharedComponents.Count > 2)
        {
            return false;
        }
        
        // If they share exactly 2 components, ensure they're connected
        if (sharedComponents.Count == 2)
        {
            // Check if shared components are adjacent in both loops
            if (!AreComponentsAdjacent(sharedComponents[0], sharedComponents[1], candidateLoop) ||
                !AreComponentsAdjacent(sharedComponents[0], sharedComponents[1], existingMesh))
            {
                return false;
            }
        }
    }
    
    return true;
}

    private bool AreComponentsAdjacent(Component comp1, Component comp2, List<Component> loop)
{
    // Find positions of components in the loop
    int index1 = loop.IndexOf(comp1);
    int index2 = loop.IndexOf(comp2);
    
    // Check if they're next to each other (considering loop wrapping)
    int loopCount = loop.Count;
    return Math.Abs(index1 - index2) == 1 || 
           Math.Abs(index1 - index2) == loopCount - 1;
}

    public List<List<Component>> GetLoops(Component current, Component previous, List<Component> visited)
    {
        // Create a new list for the current path
        List<Component> currentPath = new List<Component>(visited);
        currentPath.Add(current);
    
        List<List<Component>> loops = new List<List<Component>>();
    
        foreach (Component next in current.GetNext())
        {
            // Skip if we're going back to the previous component
            if (previous != null && previous.GetNext().Contains(next))
            {
                continue;
            }

            // If we found the starting component and path length > 2, we have a loop
            if (currentPath.Contains(next))
            {
                if (next == currentPath[0] && currentPath.Count > 2)
                {
                    loops.Add(new List<Component>(currentPath));
                }
                continue;
            }

            // Recurse to find more loops
            var nextLoops = GetLoops(next, current, currentPath);
            loops.AddRange(nextLoops);
        }

        return loops;
    }
    
    private void PrintLoops(List<List<Component>> allLoops, List<List<Component>> meshLoops)
    {
        Debug.Log("=== All Possible Loops ===");
        foreach (var loop in allLoops)
        {
            string loopStatement = "Loop length - " + loop.Count + " loop values: ";
            foreach (var component in loop)
            {
                loopStatement += $"[{component.name}]";
            }
            Debug.Log(loopStatement);
        }

        Debug.Log("\n=== Mesh Analysis Loops ===");
        foreach (var loop in meshLoops)
        {
            string meshStatement = "Mesh length - " + loop.Count + " mesh values: ";
            foreach (var component in loop)
            {
                meshStatement += $"[{component.name}]";
            }
            Debug.Log(meshStatement);
        }
    }

    public void CheckType()
    {
        if (Input.GetKey(KeyCode.Alpha1))
        {
            _componentType = Component.ComponentType.Wire;
            Debug.Log("pressed 1");
        }else if (Input.GetKey(KeyCode.Alpha2))
        {
            _componentType = Component.ComponentType.Resistor;
        }
        else if(Input.GetKey(KeyCode.Alpha3))
        {
            _componentType = Component.ComponentType.VoltageSource;
        }
    }
}













/*
 *
 *          
 * 
 */

/*
 *
 *  Current weaknesses
 *      When not connecting in a clockwise order, loop connections are not made even though they are established (Must be place in ccw direction for now)
 *      Knowing where to start checking for loops
 * 
 */
