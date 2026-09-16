using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguageTAB : SettingsTAB
{
    [SerializeField] TMP_Dropdown languageDropdown;

    static readonly string[] LOCALE_CODES = { "en", "ko" };
    static readonly string[] LOCALE_LABELS = { "English", "HanGeul" };

    protected override void OnBind()
    {
        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(new List<string>(LOCALE_LABELS));
        languageDropdown.onValueChanged.AddListener(index =>
        {
            Current.LocaleCode = LOCALE_CODES[index];
            NotifyChanged();
        });
    }

    public override void Refresh()
    {
        int index = System.Array.IndexOf(LOCALE_CODES, Current.LocaleCode);
        if (index < 0) index = 0;
        languageDropdown.SetValueWithoutNotify(index);
        languageDropdown.RefreshShownValue();
    }
}