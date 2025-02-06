using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplaySeperate : MonoBehaviour
{
    private void Start()
    {
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }
        if (Display.displays.Length > 2)
        {
            Display.displays[2].Activate();
        }
    }
}
