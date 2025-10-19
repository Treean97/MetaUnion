public static class EmoteKeys
{
    // ===== Room-scoped (앵커 네임스페이스) =====
    public static string _ACTIVE(int vid)   => $"_EMOTE_{vid}_ACTIVE";
    public static string _EMOTE_ID(int vid) => $"_EMOTE_{vid}_ID";
    public static string _START(int vid)    => $"_EMOTE_{vid}_START";
    public static string _SLOTS(int vid)    => $"_EMOTE_{vid}_SLOTS";

    // ===== Player-scoped (솔로 이모트) =====
    public const string _SOLO_ID    = "_SOLO_ID";
    public const string _SOLO_START = "_SOLO_START";
}
