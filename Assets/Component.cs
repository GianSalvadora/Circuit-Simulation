using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Component : MonoBehaviour
{
    [SerializeField] private List<Component> nextComponents;

    public float voltage;
    public float current;
    public float resistance;

    public ComponentType CType = ComponentType.Wire;
    public enum ComponentType
    {
        Wire,
        VoltageSource,
        Resistor
    }
    public void AddNext(Component component)
    {
        nextComponents.Add(component);
    }

    public List<Component> GetNext()
    {
        return nextComponents;
    }
}
