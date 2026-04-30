using UnityEngine;

public class ParticleAbsorbToCore : MonoBehaviour
{
    public Transform core;     // 코어 중심
    public float attractForce = 8f;  // 흡수 속도
    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[ps.main.maxParticles];
    }

    void LateUpdate()
    {
        int count = ps.GetParticles(particles);
        for (int i = 0; i < count; i++)
        {
            Vector3 dir = (core.position - particles[i].position).normalized;
            particles[i].velocity = dir * attractForce;
        }
        ps.SetParticles(particles, count);
    }
}