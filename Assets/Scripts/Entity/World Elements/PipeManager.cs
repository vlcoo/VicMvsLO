using UnityEngine;

public class PipeManager : MonoBehaviour
{
    public Enums.PipeTransitionTypes transitionType = Enums.PipeTransitionTypes.Cut;
    public bool entryAllowed = true, bottom = false, miniOnly = false, fromSubarea = false;
    public PipeManager otherPipe;
}
