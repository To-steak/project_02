using System;

namespace Project02.Service
{
    [Serializable]
    public struct GameSettings
    {
        // Graphic
        public int ResolutionWidth;
        public int ResolutionHeight;
        public WindowModeType WindowMode;
        public int TargetFrameRate;

        // Audio
        public float MasterVolume;
        public float MusicVolume;
        public float SFXVolume;

        // Keyboard
        public float MouseSensitivity;
        public string KeyBinding;

        // Language
        public string LocaleCode;
    }
}