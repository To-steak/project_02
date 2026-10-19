using System;
using Project02.Service;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Project02.View
{
    /// <summary>키 바인딩 버튼들. 리바인딩이 끝나면 바인딩 오버라이드 JSON을 알린다.</summary>
    public class KeyBindingView : MonoBehaviour
    {
        [Serializable]
        public class RebindEntry
        {
            public string Action;          // "Jump", "Move" ...
            public string CompositePart;   // 컴포지트 파트 이름("up", "left" ...), 일반 바인딩이면 비움
            public Button Button;
            public TMP_Text Label;
            [NonSerialized] public int BindingIndex = -1;
        }

        public event Action<string> Changed;

        [SerializeField] private RebindEntry[] _bindings;

        private InputActionRebindingExtensions.RebindingOperation _rebind;
        private static InputActionAsset Asset => InputService.Instance.Actions.asset;

        private void Awake()
        {
            foreach (var item in _bindings)
            {
                var action = Asset.FindAction(item.Action?.Trim());
                if (action == null)
                {
                    Debug.LogError($"[{nameof(KeyBindingView)}] no action: '{item.Action}'", this);
                    continue;
                }

                item.BindingIndex = FindBindingIndex(action, item.CompositePart?.Trim());
                if (item.BindingIndex < 0)
                {
                    Debug.LogError($"[{nameof(KeyBindingView)}] no binding: {item.Action} / Part='{item.CompositePart}'", this);
                    continue;
                }

                if (item.Button == null)
                {
                    continue;
                }

                var captured = item;
                item.Button.onClick.AddListener(() => StartRebind(captured));
            }
        }

        private void OnEnable()
        {
            Render();
        }

        private void OnDisable()
        {
            _rebind?.Cancel();
        }

        /// <summary>현재 입력 에셋의 바인딩을 버튼 라벨에 표시한다.</summary>
        public void Render()
        {
            foreach (var item in _bindings)
            {
                if (item.BindingIndex < 0 || item.Label == null)
                {
                    continue;
                }

                var action = Asset.FindAction(item.Action);
                item.Label.text = action.GetBindingDisplayString(item.BindingIndex);
            }
        }

        private void StartRebind(RebindEntry key)
        {
            var action = Asset.FindAction(key.Action);
            bool enabled = action.enabled;
            action.Disable();

            key.Label.text = "...";
            SetButtonsInteractable(false);

            _rebind = action.PerformInteractiveRebinding(key.BindingIndex)
                .WithControlsExcluding("<Mouse>/position")
                .WithControlsExcluding("<Mouse>/delta")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(_ => FinishRebind(action, key.BindingIndex, enabled, true))
                .OnCancel(_ => FinishRebind(action, key.BindingIndex, enabled, false))
                .Start();
        }

        private void FinishRebind(InputAction action, int bindingIndex, bool enabled, bool changed)
        {
            _rebind.Dispose();
            _rebind = null;

            if (changed)
            {
                var bind = action.bindings[bindingIndex];
                if (bind.overridePath == bind.path)
                {
                    action.RemoveBindingOverride(bindingIndex);
                }
            }

            if (enabled)
            {
                action.Enable();
            }

            SetButtonsInteractable(true);
            Render();

            if (changed)
            {
                Changed?.Invoke(SaveOverride());
            }
        }

        private void SetButtonsInteractable(bool value)
        {
            foreach (var item in _bindings)
            {
                if (item.Button != null)
                {
                    item.Button.interactable = value;
                }
            }
        }

        private static int FindBindingIndex(InputAction action, string part)
        {
            var bindings = action.bindings;
            for (int i = 0; i < bindings.Count; i++)
            {
                var bind = bindings[i];
                if (string.IsNullOrEmpty(part))
                {
                    if (!bind.isComposite && !bind.isPartOfComposite)
                    {
                        return i;
                    }
                }
                else if (bind.isPartOfComposite && string.Equals(bind.name, part, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private static string SaveOverride()
        {
            foreach (var bind in Asset.bindings)
            {
                if (!string.IsNullOrEmpty(bind.overridePath))
                {
                    return Asset.SaveBindingOverridesAsJson();
                }
            }

            return string.Empty;
        }
    }
}