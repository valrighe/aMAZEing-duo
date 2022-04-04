using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Creates a console that can be seen on screen during the game
public class ConsoleToGUI : MonoBehaviour
{
    static string myLog = "";
    private string output;
    private string stack;
     
    // Called when the Log Message Received becomes enabled and active
    void OnEnable()
    {
        Application.logMessageReceived += Log;
    }
    
    // Called when the Log Message Received becomes disabled
    void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }
    
    // Logs the messages to the console
    // Updates every action or Debug.Log in the console
    public void Log(string logString, string stackTrace, LogType type)
    {
        output = logString;
        stack = stackTrace;
        myLog = output + "\n" + myLog;
        if (myLog.Length > 5000)
        {
            myLog = myLog.Substring(0, 4000);
        }
    }
    
    // Called for rendering and handling GUI events, in this case the console
    void OnGUI()
    {
        myLog = GUI.TextArea(new Rect(10, 10, 500, 50), myLog);
    }
}