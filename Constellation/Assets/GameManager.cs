using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEditor.Progress;

public class GameManager : MonoBehaviour
{
    #region Singleton pattern
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<AudioManager>().Play("Music");

        InputManager.Instance.OnSaveScreenshots += TakeScreenshots;
    }

    public void TakeScreenshots()
    {
        string d = Environment.GetFolderPath(
               Environment.SpecialFolder.Desktop) + "/CO-Stellation/Screenshots/" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        Directory.CreateDirectory(d);

        StartCoroutine(ScreenshotsCoroutine(d));
    }

    public IEnumerator ScreenshotsCoroutine( string d)
    {
        var constellations = Constellation.Constellations;

        CursorManager.Instance.HideCursor();

        ScreenCapture.CaptureScreenshot(d + "/Screenshot_Regular.png");
        yield return null;

        foreach (var constellation in constellations)
        {
            constellation.StopChangingAlpha = true;
            constellation.ChangeConstellationAlpha(0);
        }

        var i = 0;
        foreach (var constellation in constellations)
        {
            constellation.ChangeConstellationAlpha(1);
            ScreenCapture.CaptureScreenshot(d + "/Screenshot_" + i++ + ".png");
            yield return null;
            constellation.ChangeConstellationAlpha(0);
        }

        ScreenCapture.CaptureScreenshot(d + "/Screenshot_Empty.png");
        yield return null;

        foreach (var constellation in constellations)
        {
            constellation.ChangeConstellationAlpha(1);
            constellation.StopChangingAlpha = false;
        }
        ScreenCapture.CaptureScreenshot(d + "/Screenshot_Global.png");

        yield return null;

        CursorManager.Instance.ShowCursor();
        Time.timeScale = 1;
    }
}
