using UnityEngine;

public class PipeManager : MonoBehaviour
{
    public Enums.PipeTransitionTypes transitionType = Enums.PipeTransitionTypes.Cut;
    public bool entryAllowed = true, bottom, miniOnly, fromSubarea, fadeOutMusic;
    public PipeManager otherPipe;
}