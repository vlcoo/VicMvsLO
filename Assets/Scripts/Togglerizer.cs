using System.Collections;
using System.Collections.Generic;
using NSMB.Utils;
using UnityEngine;

public class Togglerizer : MonoBehaviour
{
    public List<string> currentEffects = new();

    // Start is called before the first frame update
    void Start()
    {
        Utils.GetCustomProperty(Enums.NetRoomProperties.SpecialRules, out currentEffects);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
