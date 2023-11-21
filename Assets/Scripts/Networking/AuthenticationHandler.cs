using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Networking;

public class AuthenticationHandler
{
    private static readonly string URL = "https://mariovsluigi.azurewebsites.net/auth/init";

    public static void Authenticate(string userid, string token, string region)
    {
        var request = URL + "?";
        if (userid != null)
            request += "&userid=" + userid;
        if (token != null)
            request += "&token=" + token;

        var client = UnityWebRequest.Get(request);

        client.certificateHandler = new MvLCertificateHandler();
        client.disposeCertificateHandlerOnDispose = true;
        client.disposeDownloadHandlerOnDispose = true;
        client.disposeUploadHandlerOnDispose = true;

        var resp = client.SendWebRequest();
        resp.completed += a =>
        {
            if (client.result != UnityWebRequest.Result.Success)
            {
                if (MainMenuManager.Instance)
                {
                    MainMenuManager.Instance.OpenErrorBox(
                        client.error.Contains("Cannot resolve")
                            ? "Your device's internet connection is poor."
                            : "Servers might be down; try again later.");
                    MainMenuManager.Instance.OnDisconnected(DisconnectCause.CustomAuthenticationFailed);
                }

                return;
            }

            AuthenticationValues values = new();
            values.AuthType = CustomAuthenticationType.Custom;
            values.UserId = userid;
            values.AddAuthParameter("data", client.downloadHandler.text.Trim());
            PhotonNetwork.AuthValues = values;

            PhotonNetwork.ConnectToRegion(region);

            client.Dispose();
        };
    }
}