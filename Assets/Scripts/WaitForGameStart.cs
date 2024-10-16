using Photon.Pun;

public abstract class WaitForGameStart : MonoBehaviourPun
{
    public enum FunctionTarget
    {
        ALL,
        MASTER_ONLY,
        OWNER_ONLY
    }

    public FunctionTarget target = FunctionTarget.ALL;

    public void AttemptExecute()
    {
        if (!photonView)
        {
            Execute();
            return;
        }

        switch (target)
        {
            case FunctionTarget.ALL:
            {
                Execute();
                break;
            }
            case FunctionTarget.MASTER_ONLY:
            {
                if (PhotonNetwork.IsMasterClient)
                    Execute();
                break;
            }
            case FunctionTarget.OWNER_ONLY:
            {
                if (photonView.IsMine)
                    Execute();
                break;
            }
        }
    }

    public abstract void Execute();
}