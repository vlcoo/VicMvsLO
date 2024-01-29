using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BulletBillLauncher : MonoBehaviourPun
{
    public float playerSearchRadius = 7, playerCloseCutoff = 1;
    public float initialShootTimer = 5;
    public bool isBanzai;
    public Animator animator;
    private readonly List<GameObject> bills = new();
    private readonly Vector2 closeSearchBox = new(1.5f, 1f);
    private Vector2 spawnOffset;
    private string prefabPath;

    private Vector2 searchBox;
    private Vector2 searchOffset;
    private float shootTimer;

    private void Start()
    {
        searchBox = new Vector2(playerSearchRadius, playerSearchRadius);
        searchOffset = new Vector2(playerSearchRadius / 2 + playerCloseCutoff, 0);

        if (isBanzai)
        {
            prefabPath = "Prefabs/Enemy/BanzaiBill";
            spawnOffset = new Vector2(0.25f, 0.2f);
        }
        else
        {
            prefabPath = "Prefabs/Enemy/BulletBill";
            spawnOffset = new Vector2(0.25f, -0.2f);
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient || GameManager.Instance.gameover)
            return;

        if ((shootTimer -= Time.deltaTime) <= 0)
        {
            shootTimer = initialShootTimer;
            TryToShoot();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position + Vector3.down * 0.5f, closeSearchBox);
        Gizmos.color = new Color(0, 0, 1, 0.5f);
        Gizmos.DrawCube((Vector2)transform.position - searchOffset, searchBox);
        Gizmos.DrawCube((Vector2)transform.position + searchOffset, searchBox);
    }

    private void TryToShoot()
    {
        /*if (!Utils.IsTileSolidAtWorldLocation(transform.position))
            return;*/
        for (var i = 0; i < bills.Count; i++)
            if (bills[i] == null)
                bills.RemoveAt(i--);
        if (bills.Count >= 3)
            return;

        //Check for players close by
        if (IntersectsPlayer(transform.position + Vector3.down * 0.25f, closeSearchBox))
            return;

        //Shoot left
        if (IntersectsPlayer((Vector2)transform.position - searchOffset, searchBox))
        {
            var newBill = PhotonNetwork.InstantiateRoomObject(prefabPath,
                transform.position + new Vector3(-spawnOffset.x, spawnOffset.y), Quaternion.identity, 0,
                new object[] { true });
            bills.Add(newBill);
            animator.SetTrigger("launch");
            return;
        }

        //Shoot right
        if (IntersectsPlayer((Vector2)transform.position + searchOffset, searchBox))
        {
            var newBill = PhotonNetwork.InstantiateRoomObject(prefabPath,
                transform.position + new Vector3(spawnOffset.x, spawnOffset.y), Quaternion.identity, 0,
                new object[] { false });
            bills.Add(newBill);
            animator.SetTrigger("launch");
        }
    }

    private bool IntersectsPlayer(Vector2 origin, Vector2 searchBox)
    {
        foreach (var hit in Physics2D.OverlapBoxAll(origin, searchBox, 0))
            if (hit.gameObject.CompareTag("Player"))
                return true;
        return false;
    }
}