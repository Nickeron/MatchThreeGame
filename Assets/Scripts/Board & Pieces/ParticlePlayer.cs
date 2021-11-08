using UnityEngine;

public class ParticlePlayer : MonoBehaviour
{
    public float lifetime = 1f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Play()
    {
        foreach(ParticleSystem particle in GetComponentsInChildren<ParticleSystem>())
        {
            particle.Stop();
            particle.Play();
        }
    }
}
