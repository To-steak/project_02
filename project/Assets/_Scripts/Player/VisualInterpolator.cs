using System;
using UnityEngine;

/// <summary>
/// <see cref="Record"/>는 FixedUpdate에서, <see cref="Interpolate"/>는 Update에서 호출한다.
/// 보간 비율을 Time.fixedTime 기준으로 계산하기 때문에 시뮬레이션 루프가 바뀌면 함께 바꿔야 한다.
/// </summary>
/// <typeparam name="T"></typeparam>
public class VisualInterpolator<T>
{
    public T Current => _current;

    private readonly Func<T, T, float, T> _lerp;
    private T _previous;
    private T _current;

    public VisualInterpolator(Func<T, T, float, T> lerp)
    {
        _lerp = lerp;
    }

    public void Reset(T value)
    {
        _previous = _current = value;
    }

    public void Record(T value)
    {
        _previous = _current;
        _current = value;
    }

    public T Interpolate()
    {
        float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
        return _lerp(_previous, _current, alpha);
    }
}
