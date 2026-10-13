using UnityEngine;

public static class Bootstrapper
{
    private const string SERVICES_PREFAB = "Services";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateServices()
    {
        var prefab = Resources.Load<GameObject>(SERVICES_PREFAB);
        if (prefab == null)
        {
            Debug.LogError($"[{nameof(Bootstrapper)}] Resources/{SERVICES_PREFAB} 프리팹이 없습니다.");
            return;
        }

        var instance = Object.Instantiate(prefab);
        instance.name = prefab.name;
        Object.DontDestroyOnLoad(instance);
    }
}