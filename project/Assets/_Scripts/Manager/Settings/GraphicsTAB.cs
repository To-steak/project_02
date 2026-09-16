using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GraphicsTAB : SettingsTAB
{
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] TMP_Dropdown windowmodeDropdown;
    [SerializeField] TMP_Dropdown framerateDropdown;

    readonly List<Vector2Int> resolutions = new();
    static readonly int[] FRAME_RATES = { 60, 120, 144, -1 }; // -1 = inf
    const int DEFAULT_FRAME_RATE_INDEX = 0;

    protected override void OnBind()
    {
        BuildResolutionList();

        windowmodeDropdown.ClearOptions();
        windowmodeDropdown.AddOptions(new List<string>
        {
           "Full", "Borderless", "Windowed"
        });

        framerateDropdown.ClearOptions();
        List<string> labels = new();
        foreach (int fps in FRAME_RATES)
        {
            labels.Add(fps < 0 ? "inf" : $"{fps} FPS");
        }
        framerateDropdown.AddOptions(labels);

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        windowmodeDropdown.onValueChanged.AddListener(OnWindowModeChanged);
        framerateDropdown.onValueChanged.AddListener(OnFrameRateChanged);
    }

    private void BuildResolutionList()
    {
        resolutions.Clear();

        HashSet<Vector2Int> seen = new();

        foreach (var resolution in Screen.resolutions)
        {
            Vector2Int size = new(resolution.width, resolution.height);
            if (seen.Add(size))
            {
                resolutions.Add(size);
            }
        }

        List<string> labels = new();
        foreach (var resolution in resolutions)
        {
            labels.Add($"{resolution.x} x {resolution.y}");
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(labels);
    }

    public override void Refresh()
    {
        resolutionDropdown.SetValueWithoutNotify(FindResolutionIndex());
        resolutionDropdown.RefreshShownValue();

        windowmodeDropdown.SetValueWithoutNotify((int)Current.WindowMode);
        windowmodeDropdown.RefreshShownValue();

        framerateDropdown.SetValueWithoutNotify(FindFrameRateIndex());
        framerateDropdown.RefreshShownValue();
    }

    int FindResolutionIndex()
    {
        int width = Current.ResolutionWidth > 0 ? Current.ResolutionWidth : Screen.width;
        int height = Current.ResolutionHeight > 0 ? Current.ResolutionHeight : Screen.height;

        for (int i = 0; i < resolutions.Count; i++)
        {
            if (resolutions[i].x == width && resolutions[i].y == height) return i;
        }

        return Mathf.Max(0, resolutions.Count - 1);
    }

    private int FindFrameRateIndex()
    {
        for (int i = 0; i < FRAME_RATES.Length; i++)
        {
            if (FRAME_RATES[i] == Current.TargetFrameRate)
            {
                return i;
            }
        }

        return DEFAULT_FRAME_RATE_INDEX;
    }

    private void OnResolutionChanged(int index)
    {
        if (index < 0 || index >= resolutions.Count)
        {
            return;
        }

        Current.ResolutionWidth = resolutions[index].x;
        Current.ResolutionHeight = resolutions[index].y;

        NotifyChanged();
    }

    private void OnWindowModeChanged(int index)
    {
        Current.WindowMode = (GameSettings.WindowModeType)index;

        NotifyChanged();
    }

    private void OnFrameRateChanged(int index)
    {
        Current.TargetFrameRate = FRAME_RATES[Mathf.Clamp(index, 0, FRAME_RATES.Length - 1)];

        NotifyChanged();
    }
}