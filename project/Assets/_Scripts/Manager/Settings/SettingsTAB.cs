using UnityEngine;

public abstract class SettingsTAB : MonoBehaviour
{
    protected SettingsManager Manager { get; private set; }
    protected GameSettings Current => Manager.Current;

    public void Bind(SettingsManager manager)
    {
        Manager = manager;
        OnBind();
    }

    public abstract void Refresh();
    protected abstract void OnBind();
    protected void NotifyChanged() => Manager.OnSettingsChanged();
}