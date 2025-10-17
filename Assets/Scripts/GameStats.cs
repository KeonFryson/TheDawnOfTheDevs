using System;

public static class GameStats
{
    private static float sessionStartTime;
    private static int totalEnemiesDefeated;
    private static float totalDamageDealt;
    private static float totalDamageReceived;
    private static int highestWaveReached;

    private static bool sessionActive = false;

    public struct Summary
    {
        public int highestWave;
        public int totalEnemiesDefeated;
        public float totalDamageDealt;
        public float totalDamageReceived;
        public float timeSurvived;
    }

    public static void StartSession()
    {
        sessionStartTime = UnityEngine.Time.time;
        totalEnemiesDefeated = 0;
        totalDamageDealt = 0f;
        totalDamageReceived = 0f;
        highestWaveReached = 0;
        sessionActive = true;
    }

    public static void RecordEnemyKilled()
    {
        if (!sessionActive) return;
        totalEnemiesDefeated++;
    }

    public static void RecordDamageDealt(float amount)
    {
        if (!sessionActive) return;
        totalDamageDealt += Math.Max(0, amount);
    }

    public static void RecordDamageReceived(float amount)
    {
        if (!sessionActive) return;
        totalDamageReceived += Math.Max(0, amount);
    }

    public static void RecordWaveReached(int wave)
    {
        if (!sessionActive) return;
        if (wave > highestWaveReached) highestWaveReached = wave;
    }

    // Ends the session and returns a summary (does NOT reset internal counters until StartSession).
    public static Summary EndSession()
    {
        sessionActive = false;
        float elapsed = UnityEngine.Time.time - sessionStartTime;
        return new Summary
        {
            highestWave = highestWaveReached,
            totalEnemiesDefeated = totalEnemiesDefeated,
            totalDamageDealt = totalDamageDealt,
            totalDamageReceived = totalDamageReceived,
            timeSurvived = elapsed
        };
    }
}