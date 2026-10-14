using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Project02.View
{
    public class DropdownView : MonoBehaviour
    {
        public event Action<int> Changed;

        [SerializeField] private TMP_Dropdown _dropdown;

        private void Awake()
        {
            _dropdown.onValueChanged.AddListener(index => Changed?.Invoke(index));
        }

        public void SetOptions(List<string> labels)
        {
            _dropdown.ClearOptions();
            _dropdown.AddOptions(labels);
        }

        public void Render(int index)
        {
            _dropdown.SetValueWithoutNotify(index);
            _dropdown.RefreshShownValue();
        }
    }
}