using UnityEngine;

[CreateAssetMenu(fileName = "SOWeapon", menuName = "Scriptable Objects/SOWeapon")]
public class SOWeapon : ScriptableObject
{
    public string DisplayName;
    public float WeaponCoefficient;
    public float RapidFire;
    public SOBullet Bullet;
    public WeaponView Prefab;
    public float CriticalChance;
    public float HipSpread;
    public float MaxSpread;
    public float SpreadPerShot;
    public float SpreadRecovery;
    public float SpreadRecoveryDelay;
    public float AdsSpreadMultiplier;
    public float IdleSpreadMultiplier;
    public float WalkSpreadMultiplier;
    public float RunSpreadMultiplier;
    public float RecoilHorizontal;
    public float RecoilVertical;
    public float AdsRecoilMultiplier;
    public float AdsZoom;
    public float Range;
    public AnimationCurve Falloff;
    public float NoiseRange;
    public float ShakeRandom;
    public bool IsSuppressed;
}