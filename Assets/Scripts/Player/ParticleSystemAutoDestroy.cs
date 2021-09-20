using UnityEngine;

public class ParticleSystemAutoDestroy : MonoBehaviour
{
    private new ParticleSystem particleSystem;

    // Start is called before the first frame update
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (particleSystem)
        {
            if (!particleSystem.IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }
}
