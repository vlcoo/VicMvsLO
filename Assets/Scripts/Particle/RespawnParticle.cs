using System.Collections;
using Photon.Pun;
using UnityEngine;

public class RespawnParticle : MonoBehaviour
{
    public PlayerController player;

    [SerializeField] private float respawnTimer = 1.5f;

    public void Start()
    {
        foreach (var system in GetComponentsInChildren<ParticleSystem>())
        {
            var main = system.main;
            main.startColor = player.AnimationController.GlowColor;

            system.Play();
        }

        // null propagation should be ok
        if (player?.photonView.IsMine ?? false) StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnTimer);
        player.photonView.RPC("Respawn", RpcTarget.All);
    }
}