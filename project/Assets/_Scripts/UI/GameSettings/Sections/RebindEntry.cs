using System;
using TMPro;
using UnityEngine.UI;

[Serializable]
public class RebindEntry
{
    public string Action;   // "Jump", "Move" ...
    public string CompositePart;     // 컴포지트 파트 이름("up", "left" ...), 일반 바인딩이면 비움
    public Button Button;
    public TMP_Text Label;
    [NonSerialized] public int BindingIndex = -1;
}