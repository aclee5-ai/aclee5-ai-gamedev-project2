using UnityEngine;

public static class NoiseUtil
{
    public static void Emit(Vector3 pos, float radius)
    {
        foreach (EnemyAI e in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
            if (Vector3.Distance(e.transform.position, pos) <= radius)
                e.HearNoise(pos);
    }
}
