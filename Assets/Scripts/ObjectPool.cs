using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic high-performance Object Pool for effects/GameObjects to eliminate GC Alloc and Instantiate/Destroy overhead.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [Header("Pool Setup")]
    [SerializeField] private GameObject confettiPrefab;
    [SerializeField] private int initialPoolSize = 5;

    private readonly Queue<GameObject> confettiPool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePool();
    }

    private void InitializePool()
    {
        if (confettiPrefab == null) return;

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = CreateNewInstance();
            obj.SetActive(false);
            confettiPool.Enqueue(obj);
        }
    }

    private GameObject CreateNewInstance()
    {
        GameObject obj = Instantiate(confettiPrefab, transform);
        return obj;
    }

    /// <summary>
    /// Spawns a pooled Confetti effect at the given position and auto-returns it to pool after lifetime.
    /// </summary>
    public GameObject SpawnConfetti(Vector3 position, float lifetime = 4f)
    {
        GameObject effectObj;

        if (confettiPool.Count > 0)
        {
            effectObj = confettiPool.Dequeue();
        }
        else
        {
            // Expand pool if necessary
            effectObj = CreateNewInstance();
        }

        effectObj.transform.position = position;
        effectObj.transform.rotation = Quaternion.identity;
        effectObj.SetActive(true);

        // Play particle systems
        ParticleSystem[] particles = effectObj.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Play(true);
        }

        StartCoroutine(ReturnToPoolRoutine(effectObj, lifetime));
        return effectObj;
    }

    private IEnumerator ReturnToPoolRoutine(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (obj != null)
        {
            // Stop particles and reset
            ParticleSystem[] particles = obj.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            obj.SetActive(false);
            obj.transform.SetParent(transform);
            confettiPool.Enqueue(obj);
        }
    }
}
