using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class UpdateChecker
{
    private static readonly string API_URL = "http://api.github.com/repos/vlcoo/VicMvsLO/releases/latest";

    /// <summary>
    ///     Returns if we're up to date, OR newer, compared to the latest GitHub release version number
    /// </summary>
    public static async void IsUpToDate(Action<string> callback)
    {
        //get http results
        var request = (HttpWebRequest)WebRequest.Create(API_URL);
        request.Accept = "application/json";
        request.UserAgent = "vlcoo/VicMvsLO";

        var response = (HttpWebResponse)await request.GetResponseAsync();

        if (response.StatusCode != HttpStatusCode.OK)
            return;

        try
        {
            //get the latest release version number from github
            var json = new StreamReader(response.GetResponseStream()).ReadToEnd();
            var data = JObject.Parse(json);

            var tag = data.Value<string>("tag_name");
            if (tag.StartsWith("v"))
                tag = tag[1..];
            if (tag.Contains("-"))
                tag = tag.Split("-")[0];

            var splitTag = tag.Split(".");

            var ver = Application.version;
            if (ver.StartsWith("v"))
                ver = ver[1..];
            if (ver.Contains("-"))
                ver = ver.Split("-")[0];

            var splitVer = ver.Split(".");

            Debug.Log($"[UPDATE CHECK] Local version: {ver} / Remote version: {tag}");

            //check if we're a higher version
            for (var i = 0; i < 4; i++)
            {
                int.TryParse(splitTag[i], out var remote);
                int.TryParse(splitVer[i], out var local);

                if (local < remote) break;
            }

            callback(tag);
        }
        catch
        {
        }
    }
}