using System;
using UnityEngine;

[Serializable]
public class GameSettings
{
    // 필드 추가 시 최하단의 AreEqual에도 추가 해야 함.
    public int Version = 1;

    // Graphic
    public int ResolutionWidth = 0;
    public int ResolutionHeight = 0;
    public enum WindowModeType { ExclusiveFullScreen = 0, FullScreenWindow = 1, Windowed = 2 }
    public WindowModeType WindowMode = WindowModeType.FullScreenWindow;
    public int TargetFrameRate = 60;

    // Audio
    public float MasterVolume = 1.0f;
    public float MusicVolume = 0.75f;
    public float SFXVolume = 0.75f;

    // Keyboard
    public float MouseSensitivity = 0.5f;
    public string KeyBinding = string.Empty;

    // Language
    public string LocaleCode = "en";

    /// <summary>
    /// 얕은 복사
    /// current = saved 로 대입하면 같은 참조를 가리킴.
    /// Back(취소)가 동작하지 않음.
    /// 그래서 복사본을 만들어 씀.
    /// </summary>
    /// <returns>복사본</returns>
    public GameSettings Clone() => (GameSettings)MemberwiseClone();

    /// <summary>
    /// 두 설정이 같은지 비교.
    /// 필드를 추가하면 여기도 추가할 것.
    /// float 필드는 부동소수점 오차 때문에 근사값으로 비교.
    /// </summary>
    /// <param name="a">비교군 A</param>
    /// <param name="b">비교군 B</param>
    /// <returns>비교 결과</returns>
    public static bool AreEqual(GameSettings a, GameSettings b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a == null || b == null) return false;

        return a.Version == b.Version
            && a.ResolutionWidth == b.ResolutionWidth
            && a.ResolutionHeight == b.ResolutionHeight
            && a.WindowMode == b.WindowMode
            && a.TargetFrameRate == b.TargetFrameRate
            && Mathf.Approximately(a.MasterVolume, b.MasterVolume)
            && Mathf.Approximately(a.MusicVolume, b.MusicVolume)
            && Mathf.Approximately(a.SFXVolume, b.SFXVolume)
            && Mathf.Approximately(a.MouseSensitivity, b.MouseSensitivity)
            && a.KeyBinding == b.KeyBinding
            && a.LocaleCode == b.LocaleCode;
    }
}