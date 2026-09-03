using UnityEngine;

public class VisualInterpolator : MonoBehaviour
{
    [SerializeField] float _snapDistance = 2f;

    Transform _root;
    Vector3 _prevPos, _currPos;
    Quaternion _prevRot, _currRot;

    void Awake()
    {
        _root = transform.parent;

        if (_root == null)
        {
            enabled = false;
        }
    }

    void Start()
    {
        transform.SetParent(null, true);

        _prevPos = _currPos = _root.position;
        _prevRot = _currRot = _root.rotation;
    }

    void FixedUpdate()
    {
        if (_root == null)
        {
            Destroy(gameObject);
            return;
        }

        _prevPos = _currPos;
        _prevRot = _currRot;

        _currPos = _root.position;
        _currRot = _root.rotation;

        if (Vector3.Distance(_prevPos, _currPos) > _snapDistance)
        {
            _prevPos = _currPos;
            _prevRot = _currRot;
        }
    }

    void LateUpdate()
    {
        if (_root == null) return;

        float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);

        transform.SetPositionAndRotation(Vector3.Lerp(_prevPos, _currPos, alpha), Quaternion.Slerp(_prevRot, _currRot, alpha));
    }
}