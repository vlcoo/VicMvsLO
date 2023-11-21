using System.Collections.Generic;
using UnityEngine;

public class SpinnerAnimator : MonoBehaviour
{
    [SerializeField] private Vector3 idleSpinSpeed = new(0, -100, 0), fastSpinSpeed = new(0, -1800, 0);
    [SerializeField] private Transform topArmBone;

    public Vector2 launchVelocity = new(0f, 12f);
    private readonly List<PlayerController> playersInside = new();

    private float spinPercentage;

    public void Update()
    {
        var players = playersInside.Count >= 1;
        float percentage = 0;
        if (players)
        {
            var playerWorldY = float.PositiveInfinity;
            foreach (var player in playersInside)
            {
                if (player.body.velocity.y > 0.2f)
                    continue;

                playerWorldY = Mathf.Min(playerWorldY, player.transform.position.y);
            }

            var spinnerWorldY = transform.position.y;

            if (playerWorldY != float.PositiveInfinity)
                percentage = 1 - (playerWorldY - spinnerWorldY - 0.1f) / 0.25f;
        }

        spinPercentage = Mathf.Clamp01(spinPercentage + (players ? 0.75f * Time.deltaTime : -1f * Time.deltaTime));

        topArmBone.eulerAngles +=
            (fastSpinSpeed * spinPercentage + idleSpinSpeed * (1 - spinPercentage)) * Time.deltaTime;
        topArmBone.localPosition = new Vector3(0, Mathf.Max(-0.084f, percentage * -0.07f), 0);
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        var cont = collider.gameObject.GetComponent<PlayerController>();
        if (cont)
            playersInside.Add(cont);
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
        var cont = collider.gameObject.GetComponent<PlayerController>();
        if (cont)
            playersInside.Remove(cont);
    }
}