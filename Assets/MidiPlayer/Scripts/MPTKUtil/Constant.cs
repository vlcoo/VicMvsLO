//#define MPTK_PRO

namespace MidiPlayerTK
{
    public class Constant
    {
        public const string forumSite = "https://forum.unity.com/threads/midi-player-tool-kit-good-news-for-your-rhythm-game.526741/";
        public const string paxSite = "https://www.paxstellar.com";
        public const string apiSite = "https://mptkapi.paxstellar.com/annotated.html";
        public const string blogSite = "https://paxstellar.fr/midi-player-tool-kit-for-unity-v2/";
        public const string UnitySite = "https://assetstore.unity.com/packages/tools/audio/midi-tool-kit-pro-115331";
        public const string DiscordSite = "https://discord.gg/NhjXPTdeWk";

#if MPTK_PRO
        public const string version = "2.11.2 Pro";
#else
        public const string version = "2.11.2 Free";
#endif
        public const string releaseDate = "February, 07 2024";

    }
}
