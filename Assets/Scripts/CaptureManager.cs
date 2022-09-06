using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class CaptureManager : MonoBehaviour
{
    #region Variables

    private Camera _camera;

    private static int _captureCount = 0;
    
    #endregion Variables

    #region Unity Methods

    private void Start()
    {
        _camera = Camera.main;
    } // End of Start

    private void Update()
    {
        Capture();
    } // End of Update

    #endregion Unity Methods

    #region Help Methods

    private void Capture()
    {
        if (!Input.GetKey(KeyCode.Space)) return;
        
        var path = Application.dataPath + "/CaptureFiles/capture" + _captureCount++ + ".png";
        StartCoroutine(Co_Capture(path));
    } // End of CaptureScreen

    private IEnumerator Co_Capture(string path)
    {
        if (path == null) yield break;

        yield return new WaitForEndOfFrame();

        var rect = new Rect(0f, 0f, Screen.width, Screen.height);
        var texture = CaptureScreen(Camera.main, rect);

        var bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log("캡쳐 완료");
    } // End of C0_Capture

    private Texture2D CaptureScreen(Camera camera, Rect pRect)
    {
        Texture2D capture;
        CameraClearFlags preClearFlags = camera.clearFlags;
        
        Color preBackgroundColor = camera.backgroundColor;
        {
            camera.clearFlags = CameraClearFlags.SolidColor;

            camera.backgroundColor = Color.black;
            camera.Render();
            Texture2D blackBackgroundCapture = CaptureView(pRect);

            camera.backgroundColor = Color.white;
            camera.Render();
            Texture2D whiteBackgroundCapture = CaptureView(pRect);

            for (int x = 0; x < whiteBackgroundCapture.width; ++x)
            {
                for (int y = 0; y < whiteBackgroundCapture.height; ++y)
                {
                    Color black = blackBackgroundCapture.GetPixel(x, y);
                    Color white = whiteBackgroundCapture.GetPixel(x, y);
                    if (black != Color.clear)
                    {
                        whiteBackgroundCapture.SetPixel(x, y, GetColor(black, white));
                    }
                }
            }

            whiteBackgroundCapture.Apply();
            capture = whiteBackgroundCapture;
            Object.DestroyImmediate(blackBackgroundCapture);
        }
        camera.backgroundColor = preBackgroundColor;
        camera.clearFlags = preClearFlags;
        return capture;
    }

    private Color GetColor(Color black, Color white)
    {
        float alpha = GetAlpha(black.r, white.r);
        return new Color(
            black.r / alpha,
            black.g / alpha,
            black.b / alpha,
            alpha);
    }

    private float GetAlpha(float black, float white)
    {
        return 1 + black - white;
    }

    private Texture2D CaptureView(Rect rect)
    {
        Texture2D captureView = new Texture2D((int) rect.width, (int) rect.height, TextureFormat.ARGB32, false);
        captureView.ReadPixels(rect, 0, 0, false);
        return captureView;
    }

    #endregion Help Methods
}