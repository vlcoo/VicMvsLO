using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using NSMB.Utils;
using UnityEngine;

public class LevelModel
{
    public int Id;
    public string UserName, Title, Description;
    public Dictionary<string, object> Contents;

    public LevelModel(string userName, string title, string description, Dictionary<string, object> contents, int id = -1)
    {
        Id = id;
        UserName = userName.SanitizeForRichTextbox();
        Title = title.SanitizeForRichTextbox();
        Description = description.SanitizeForRichTextbox();
        Contents = contents;
    }

    public LevelModel(Dictionary<string, object> levelDict)
    {
        if (levelDict["identifiers"] is not Dictionary<string, object> identifiers || levelDict["contents"] is not Dictionary<string, object> contents)
        {
            Debug.LogError("Invalid level content format");
            return;
        }

        Id = int.Parse(identifiers.GetValueOrDefault("id").ToString());
        UserName = identifiers.GetValueOrDefault("user_name") as string;
        if (string.IsNullOrEmpty(UserName)) UserName = "Unknown";
        UserName = UserName.SanitizeForRichTextbox();
        Title = identifiers.GetValueOrDefault("title") as string;
        if (string.IsNullOrEmpty(Title)) Title = "???";
        Title = Title.SanitizeForRichTextbox();
        Description = identifiers.GetValueOrDefault("description") as string;
        Description = Description.SanitizeForRichTextbox();
        Contents = contents;
    }
    
    public ExitGames.Client.Photon.Hashtable ToHashtable()
    {
        return new ExitGames.Client.Photon.Hashtable
        {
            {"user_name", UserName},
            {"title", Title},
            {"description", Description},
            {"contents", Contents}
        };
    }

    public static LevelModel FromHashtable(ExitGames.Client.Photon.Hashtable levelHashtable)
    {
        return new LevelModel(levelHashtable.GetValueOrDefault("user_name") as string,
            levelHashtable.GetValueOrDefault("title") as string,
            levelHashtable.GetValueOrDefault("description") as string,
            levelHashtable.GetValueOrDefault("contents") as Dictionary<string, object>);
    }
}
