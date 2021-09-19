using UnityEngine;

public class ParticlePlayer : MonoBehaviour
{
    public ParticleSystem[] allParticles;
    public float lifetime = 1f;

    void Start()
    {
        allParticles = GetComponentsInChildren<ParticleSystem>();
        Destroy(gameObject, lifetime);
    }

    public void Play()
    {
        foreach(ParticleSystem particle in allParticles)
        {
            particle.Stop();
            particle.Play();
        }
    }
}
