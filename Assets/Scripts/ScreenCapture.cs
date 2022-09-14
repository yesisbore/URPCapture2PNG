using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScreenCapture : MonoBehaviour
{
    #region Variables

    public TMP_Text Log;

    #endregion Variables


    #region UnityMethods

//private void Start() { Initialize();}

//private void Update(){}

    #endregion UnityMethods


    #region HelpMethods

//private void Initialize(){}

    public void OnSaveScreenshotPress()
    {
        ScreenCaptureManager.SaveScreenshot("MyScreenshot", "ScreenshotApp", "jpeg");
        ShowLog("Save Screen Shot");
    } // End of OnSaveScreenShotPress

    private void ShowLog(string logText)
    {
        if(!Log) return;
        
        Log.text = logText;
    }
    #endregion HelpMethods
}
