using Photon.Pun;
using UnityEngine;

public class KillplaneKill : MonoBehaviourPun
{
    [SerializeField] private float killTime;
    private float timer;

    public void Update()
    {
        if (transform.position.y >= GameManager.Instance.GetLevelMinY())
            return;

        if ((timer += Time.deltaTime) < killTime)
            return;

        if (!photonView)
        {
            Destroy(gameObject);
            return;
        }

        if (photonView.IsMine) PhotonNetwork.Destroy(photonView);
    }
}