using System;
using UnityEngine;

[Serializable]
public class GameSettings
{
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
    /// 깊은 복사. 저장 형식(JSON)을 거치므로 필드가 추가되거나 배열·리스트가 생겨도 따로 고칠 필요가 없다.
    /// current = saved로 대입하면 같은 참조를 가리켜 Back(취소)가 동작하지 않으므로 복사본을 쓴다.
    /// </summary>
    public GameSettings Clone() => JsonUtility.FromJson<GameSettings>(JsonUtility.ToJson(this));

    /// <summary>
    /// 저장했을 때 같은 내용이 되는지로 비교한다. 필드가 추가되어도 따로 고칠 필요가 없다.
    /// </summary>
    public static bool AreEqual(GameSettings a, GameSettings b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a == null || b == null) return false;

        return JsonUtility.ToJson(a) == JsonUtility.ToJson(b);
    }
}