using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Photon.Pun;
using Photon.Realtime;

public static class PhotonExtensions
{
    public static string SPECIALS_URL = "http://mvloml.vlcoo.net/specials.json";

    public static bool IsMineOrLocal(this PhotonView view)
    {
        return !view || view.IsMine;
    }

    public static bool HasRainbowName(this Player player)
    {
        return player.GetAuthorityLevel() > Enums.AuthorityLevel.NORMAL;
    }

    public static Enums.AuthorityLevel GetAuthorityLevel(this Player player)
    {
        if (player?.UserId == null)
            return Enums.AuthorityLevel.NORMAL;

        var bytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(player.UserId));
        StringBuilder sb = new();
        foreach (var b in bytes)
            sb.Append(b.ToString("X2"));

        var hash = sb.ToString().ToLower();
        return GlobalController.Instance.SPECIAL_PLAYERS.Any(specialPlayer => specialPlayer.shaUserId.Equals(hash))
            ? GlobalController.Instance.SPECIAL_PLAYERS.Find(specialPlayer => specialPlayer.shaUserId.Equals(hash))
                .authorityLevel
            : Enums.AuthorityLevel.NORMAL;
    }

    //public static void RPCFunc(this PhotonView view, Delegate action, RpcTarget target, params object[] parameters) {
    //    view.RPC(nameof(action), target, parameters);
    //}
}