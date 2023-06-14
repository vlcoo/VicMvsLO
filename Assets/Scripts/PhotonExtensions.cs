using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public static class PhotonExtensions {

    private static readonly Dictionary<string, string> SPECIAL_PLAYERS = new() {
        ["ba2785e6fa17c692cccd52fcf76a6be756f160522d2a0130040c2a39d07a112c"] = "vic",
        ["f5178c698cbb80d7366e7cb8367f9643998d691115ee491c51ec0b617708463a"] = "vic",
        ["db118536670aed448cd25c16d1a12e5ca1ab84fc1e248a2aa29e28d2fe5ca051"] = "Bilhal",
        ["fd7fb495ee60bc5e6cad8a9de56f0753adc2861cfe9217f18f73caf0c24d06ba"] = "romanian",
        ["66ed39ea0bed6c5622ba5a71b3025c8e53f032442eae0924527f83e5be2dd0cb"] = "flichka",
        ["b79e9a33701b437bad4e4a5bbb79800330e9de79813ce7674533b7b00f62fbf4"] = "miyavmeow"
    };

    public static bool IsMineOrLocal(this PhotonView view) {
        return !view || view.IsMine;
    }

    public static bool HasRainbowName(this Player player) {
        if (player == null || player.UserId == null)
            return false;

        byte[] bytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(player.UserId));
        StringBuilder sb = new();
        foreach (byte b in bytes)
            sb.Append(b.ToString("X2"));

        string hash = sb.ToString().ToLower();
        return SPECIAL_PLAYERS.ContainsKey(hash);
    }

    //public static void RPCFunc(this PhotonView view, Delegate action, RpcTarget target, params object[] parameters) {
    //    view.RPC(nameof(action), target, parameters);
    //}
}