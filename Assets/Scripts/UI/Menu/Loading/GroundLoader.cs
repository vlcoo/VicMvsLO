using System.Collections.Generic;
using NSMB.Utils;
using UnityEngine;
using UnityEngine.UI;

public class GroundLoader : MonoBehaviour
{
    public List<Image> sprites;
    public List<Sprite> levelGroundMapping;

    private void Start()
    {
        Utils.GetCustomProperty(Enums.NetRoomProperties.Level, out int? level);
        if (level == null || level < 0 || level > levelGroundMapping.Count) level = 0;

        foreach (var sprite in sprites) sprite.sprite = levelGroundMapping[(int)level];
    }
}