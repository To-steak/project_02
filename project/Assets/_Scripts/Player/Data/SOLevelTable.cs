using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SOLevelTable", menuName = "Scriptable Objects/SOLevelTable")]
public class SOLevelTable : ScriptableObject
{
    [Serializable]
    public struct Row
    {
        public int TotalExp;
        public int MaxHp;
        public float MaxMp;
    }

    [SerializeField] private Row[] _rows = Array.Empty<Row>();

    public int MaxLevel => _rows.Length;

    public int GetLevel(int exp)
    {
        int level = 1;
        for (int i = 0; i < _rows.Length; i++)
        {
            if (exp < _rows[i].TotalExp)
            {
                break;
            }

            level = i + 1;
        }

        return level;
    }

    public Row Get(int level)
    {
        Debug.Assert(_rows.Length > 0, $"{name}: is empty", this);
        return _rows.Length == 0 ? default : _rows[Mathf.Clamp(level, 1, _rows.Length) - 1];
    }

#if UNITY_EDITOR
    [SerializeField] private string _url;
    [ContextMenu("Pull from google sheet")]
    private void Pull()
    {
        var request = UnityEngine.Networking.UnityWebRequest.Get(_url);
        request.SendWebRequest().completed += _ =>
        {
            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError($"{nameof(SOLevelTable)} pull failed: {request.error}");
                request.Dispose();
                return;
            }
            string text = request.downloadHandler.text;
            Debug.Log($"{nameof(SOLevelTable)} response:\n{text.Substring(0, Mathf.Min(300, text.Length))}");
            string[] lines = request.downloadHandler.text.Split('\n');
            request.Dispose();

            // Level, TotalExp, MaxHp, MaxMp
            var rows = new List<Row>();
            for (int i = 0; i < lines.Length; i++)
            {
                string[] cells = lines[i].Trim().Split(',');
                if (cells.Length < 4
                || !int.TryParse(cells[1], out int exp)
                || !int.TryParse(cells[2], out int hp)
                || !float.TryParse(cells[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float mp))
                {
                    continue;
                }

                rows.Add(new Row { TotalExp = exp, MaxHp = hp, MaxMp = mp });
            }

            UnityEditor.Undo.RecordObject(this, "Pull LevelTable");
            _rows = rows.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            Debug.Log($"{nameof(SOLevelTable)} pulled {_rows.Length} levels", this);
        };
    }
#endif
}
