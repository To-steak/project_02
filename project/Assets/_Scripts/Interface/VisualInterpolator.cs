using System;
using UnityEngine;

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
