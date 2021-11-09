using UnityEngine;

public class ParticlePlayer : MonoBehaviour
{
    public float lifetime = 1f;

    public void Play()
    {
        foreach(ParticleSystem particle in GetComponentsInChildren<ParticleSystem>())
        {
            particle.Stop();
            particle.Play();
        }

        Destroy(gameObject, lifetime);
    }
}
