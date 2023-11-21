using UnityEngine;

public class ParticleSound : MonoBehaviour
{
    private AudioSource sfx;

    private ParticleSystem system;

    public void Start()
    {
        system = GetComponent<ParticleSystem>();
        sfx = GetComponent<AudioSource>();
    }

    public void Update()
    {
        if (system.isEmitting && !sfx.isPlaying)
            sfx.Play();
        if (!system.isEmitting && sfx.isPlaying)
            sfx.Stop();
    }
}