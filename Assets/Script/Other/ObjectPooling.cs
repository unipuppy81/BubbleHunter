using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : Singleton<ObjectPooling>
{
    public GameObject bubblePrefab;
    public int poolSize = 50;

    private Queue<GameObject> pool;
    private List<Bubble> cachedBubbles = new List<Bubble>();

    private void Start()
    {
        pool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bubble = Instantiate(bubblePrefab);
            Bubble bubbleComponent = bubble.GetComponent<Bubble>();
            bubble.SetActive(false);
            pool.Enqueue(bubble);
            cachedBubbles.Add(bubbleComponent);
        }
    }

    public Bubble GetPooledBubble()
    {
        if (pool.Count > 0)
        {
            GameObject bubble = pool.Dequeue();
            return bubble.GetComponent<Bubble>();
        }
        else
        {
            GameObject bubble = Instantiate(bubblePrefab);
            Bubble bubbleComponent = bubble.GetComponent<Bubble>();
            cachedBubbles.Add(bubbleComponent);
            return bubbleComponent;
        }
    }

    public void ReturnToPool(GameObject bubble)
    {
        bubble.SetActive(false);
        pool.Enqueue(bubble);
    }
}
