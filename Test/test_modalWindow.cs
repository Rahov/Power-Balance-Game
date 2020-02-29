using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class modalWindow_test : MonoBehaviour
{
    public Rect windowRect = new Rect(20, 20, 120, 50);

    void OnGUI()
    {
        // Register the window. Notice the 3rd parameter
        windowRect = GUI.ModalWindow(0, windowRect, DoMyWindow, "My Window");
    }

    // Make the contents of the window
    void DoMyWindow(int windowID)
    {
        if (GUI.Button(new Rect(10, 20, 100, 20), "Next"))
        {
            print("Got a click");
        }
    }
}
