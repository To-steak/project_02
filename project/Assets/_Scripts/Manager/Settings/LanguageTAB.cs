using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguageTAB : SettingsTAB
{
    [SerializeField] TMP_Dropdown languageDropdown;

    static readonly string[] LANGUAGES = { "한국어", "English" };

    protected override void OnBind()
    {
        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(new List<string>(LANGUAGES));
        languageDropdown.onValueChanged.AddListener(index =>
        {
            Current.LanguageIndex = index;
            NotifyChanged();
        });
    }

    public override void Refresh()
    {
        int index = Mathf.Clamp(Current.LanguageIndex, 0, LANGUAGES.Length - 1);
        languageDropdown.SetValueWithoutNotify(index);
        languageDropdown.RefreshShownValue();
    }
}