using System.Collections;
using System.Collections.Generic;
using NSMB.Utils;
using UnityEngine;

public record LevelModel
{
    public string UserName, Title, Description;
    public Dictionary<string, object> Contents;

    public LevelModel(string userName, string title, string description, Dictionary<string, object> contents)
    {
        UserName = userName.SanitizeForRichTextbox();
        Title = title.SanitizeForRichTextbox();
        Description = description.SanitizeForRichTextbox();
        Contents = contents;
    }

    public LevelModel(Dictionary<string, object> levelDict)
    {
        UserName = levelDict.GetValueOrDefault("user_name") as string;
        if (string.IsNullOrEmpty(UserName)) UserName = "Unknown";
        UserName = UserName.SanitizeForRichTextbox();
        Title = levelDict.GetValueOrDefault("title") as string;
        Title = Title.SanitizeForRichTextbox();
        Description = levelDict.GetValueOrDefault("description") as string;
        Description = Description.SanitizeForRichTextbox();
        Contents = levelDict.GetValueOrDefault("contents") as Dictionary<string, object>;
    }
}
