using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Photon.Pun;
using Photon.Realtime;

public static class PhotonExtensions
{
    public static bool IsMineOrLocal(this PhotonView view)
    {
        return !view || view.IsMine;
    }

    public static bool HasSpecialName(this Player player)
    {
        return player.GetAuthorityLevel() > Enums.AuthorityLevel.NORMAL;
    }

    public static Enums.AuthorityLevel GetAuthorityLevel(this Player player)
    {
        return Enums.AuthorityLevel.NORMAL;
    }

    //public static void RPCFunc(this PhotonView view, Delegate action, RpcTarget target, params object[] parameters) {
    //    view.RPC(nameof(action), target, parameters);
    //}
}