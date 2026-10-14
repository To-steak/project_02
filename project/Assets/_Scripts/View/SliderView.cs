using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project02.View
{
    public class SliderView : MonoBehaviour
    {
        public event Action<int> Changed;

        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _label;

        private void Awake()
        {
            _slider.wholeNumbers = true;
            _slider.onValueChanged.AddListener(OnValueChanged);
        }

        public void SetRange(int min, int max)
        {
            _slider.minValue = min;
            _slider.maxValue = max;
        }

        public void Render(int value)
        {
            _slider.SetValueWithoutNotify(value);
            _label.text = value.ToString();
        }

        private void OnValueChanged(float value)
        {
            int rounded = Mathf.RoundToInt(value);
            _label.text = rounded.ToString();
            Changed?.Invoke(rounded);
        }
    }
}