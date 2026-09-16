using System;
using UnityEngine;

[Serializable]
public class GameSettings
{
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
    /// 필드가 늘어나도 수정할 필요 없도록 직렬화 비교.
    /// 슬라이더 값이 바뀔 때마다 호출되므로 필드가 크게 늘면 필드 비교로 교체할 것.
    /// </summary>
    /// <param name="a">대조군 A</param>
    /// <param name="b">대조군 B</param>
    /// <returns>비교 결과</returns>
    public static bool AreEqual(GameSettings a, GameSettings b) => JsonUtility.ToJson(a) == JsonUtility.ToJson(b);
}