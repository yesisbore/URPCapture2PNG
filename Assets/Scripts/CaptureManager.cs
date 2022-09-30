using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

public class CaptureManager : MonoBehaviour
{
    #region Variables

    public KeyCode CaptureJPG = KeyCode.J;
    public KeyCode CapturePNG = KeyCode.P;
    
    private Camera _camera;
    private bool _isCapturing = false;
    private WaitForSeconds _waitTime = new WaitForSeconds(0.1f);
    
    private string Date => DateTime.Now.ToString("yy-MM-dd_hh-mm-ss");
    private string CaptureFolder => Application.dataPath + "/CaptureFiles";
    private string Path(string fileExt)
    {
#if UNITY_EDITOR
        if (!AssetDatabase.IsValidFolder(CaptureFolder))
        {
            AssetDatabase.CreateFolder("Assets", "CaptureFiles");
        }
#endif
        return CaptureFolder + "/capture_" + Date + "." + fileExt;
    }
    private Rect _screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
    
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
        if (_isCapturing) return;
        
        if (Input.GetKey(CaptureJPG))
        {
            _isCapturing = true;

            StartCoroutine(CO_CaptureJPG());
        }
        else if (Input.GetKey(CapturePNG))
        {
            _isCapturing = true;

            StartCoroutine(CO_CapturePNG()); 
        }
    } // End of CaptureScreen

    private IEnumerator CO_CaptureJPG()
    {
        yield return new WaitForEndOfFrame();
        
        System.IO.File.WriteAllBytes(Path("jpg"), GetBytesJPG());
        
        yield return StartCoroutine(CO_WaitForRecurring());
    } // End of CO_CaptureJPG
    
    private byte[] GetBytesJPG() => CaptureView(_screenRect).EncodeToJPG();
    
    private IEnumerator CO_CapturePNG()
    {
        _isCapturing = true;

        yield return new WaitForEndOfFrame();

        var texture = CaptureScreen(Camera.main, _screenRect);

        var bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(Path("png"), bytes);

        yield return StartCoroutine(CO_WaitForRecurring());
    } // End of C0_Capture

    private IEnumerator CO_WaitForRecurring()
    {
        yield return _waitTime;
        Debug.Log("캡쳐 완료");
        _isCapturing = false;
    }
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