using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class StartingWall : MonoBehaviour
{
    [FormerlySerializedAs("START_DELAY")] public int startDelay;

    private void Start()
    {
        StartCoroutine(nameof(WaitAndDestroy));
    }

    private IEnumerator WaitAndDestroy()
    {
        yield return new WaitUntil(() => GameManager.Instance.started);
        yield return new WaitForSeconds(startDelay - (GlobalController.Instance.fastLoad ? 3.5f : 0));
        Instantiate(Resources.Load("Prefabs/Particle/Explosion"), transform.position + new Vector3(2, 3, 0),
            Quaternion.identity);
        GameManager.Instance.sfx.PlayOneShot(Enums.Sounds.Enemy_Bobomb_Explode.GetClip());
        GameManager.Instance.ResetStartSpeedrunTimer(false);
        Destroy(gameObject);
    }
}