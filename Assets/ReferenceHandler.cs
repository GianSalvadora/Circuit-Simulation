using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceHandler : MonoBehaviour
{
    public static ReferenceHandler singleton;
    public GameObject wire;
    public GameObject resistor;
    public GameObject battery;
    public Sprite temporary2;
    public Sprite temporary3;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
        }
    }
}
