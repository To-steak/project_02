using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GraphicsSettingsView : GameSettingsTabView
{
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] TMP_Dropdown windowmodeDropdown;
    [SerializeField] TMP_Dropdown framerateDropdown;

    readonly List<Vector2Int> resolutions = new();
    static readonly int[] FRAME_RATES = { 60, 120, 144, -1 }; // -1 = inf
    const int DEFAULT_FRAME_RATE_INDEX = 0;

    protected override void OnBind()
    {
        HashSet<Vector2Int> seen = new();
        foreach (var resolution in Screen.resolutions)
        {
            Vector2Int size = new(resolution.width, resolution.height);
            if (seen.Add(size))
            {
                resolutions.Add(size);
            }
        }

        List<string> resolutionLabels = new();
        foreach (var resolution in resolutions)
        {
            resolutionLabels.Add($"{resolution.x} x {resolution.y}");
        }
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(resolutionLabels);

        windowmodeDropdown.ClearOptions();
        List<string> windowmodeLabels = new() { "Full", "Borderless", "Windowed" };
        windowmodeDropdown.AddOptions(windowmodeLabels);

        framerateDropdown.ClearOptions();
        List<string> framerateLabels = new();
        foreach (int fps in FRAME_RATES)
        {
            framerateLabels.Add(fps < 0 ? "inf" : $"{fps} FPS");
        }
        framerateDropdown.AddOptions(framerateLabels);

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        windowmodeDropdown.onValueChanged.AddListener(OnWindowModeChanged);
        framerateDropdown.onValueChanged.AddListener(OnFrameRateChanged);
    }

    public override void Refresh()
    {
        int width = Current.ResolutionWidth > 0 ? Current.ResolutionWidth : Screen.width;
        int height = Current.ResolutionHeight > 0 ? Current.ResolutionHeight : Screen.height;
        int resolutionIndex = Mathf.Max(0, resolutions.Count - 1);
        for (int i = 0; i < resolutions.Count; i++)
        {
            if (resolutions[i].x == width && resolutions[i].y == height)
            {
                resolutionIndex = i;
                break;
            }
        }

        resolutionDropdown.SetValueWithoutNotify(resolutionIndex);
        resolutionDropdown.RefreshShownValue();

        windowmodeDropdown.SetValueWithoutNotify((int)Current.WindowMode);
        windowmodeDropdown.RefreshShownValue();

        int framerateIndex = DEFAULT_FRAME_RATE_INDEX;
        for (int i = 0; i < FRAME_RATES.Length; i++)
        {
            if (FRAME_RATES[i] == Current.TargetFrameRate)
            {
                framerateIndex = i;
                break;
            }
        }

        framerateDropdown.SetValueWithoutNotify(framerateIndex);
        framerateDropdown.RefreshShownValue();
    }

    private void OnResolutionChanged(int index)
    {
        if (index < 0 || index >= resolutions.Count)
        {
            return;
        }

        Vector2Int resolution = resolutions[index];
        Modify(s =>
        {
            s.ResolutionWidth = resolution.x;
            s.ResolutionHeight = resolution.y;
        });
    }

    private void OnWindowModeChanged(int index)
    {
        Modify(s => s.WindowMode = (GameSettings.WindowModeType)index);
    }

    private void OnFrameRateChanged(int index)
    {
        int frameRate = FRAME_RATES[Mathf.Clamp(index, 0, FRAME_RATES.Length - 1)];
        Modify(s => s.TargetFrameRate = frameRate);
    }
}