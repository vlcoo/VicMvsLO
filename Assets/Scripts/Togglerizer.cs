using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NSMB.Utils;
using UnityEngine;

public class Togglerizer : MonoBehaviour
{
    public List<string> currentEffects = new();

    // Start is called before the first frame update
    void Start()
    {
        Dictionary<string, bool> currentEffectsDict;
        Utils.GetCustomProperty(Enums.NetRoomProperties.SpecialRules, out currentEffectsDict);
        currentEffects = currentEffectsDict.Keys.ToList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
