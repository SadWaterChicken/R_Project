using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Coin : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float respawnTimeSeconds = 8;
    [SerializeField] private int goldGained = 1;

    private SphereCollider sphere;
    private SpriteRenderer visual;

    private void Awake()
    {
        sphere = GetComponent<SphereCollider>();
        //visual = GetComponentInChildren<SpriteRenderer>();
    }

    private void CollectCoin()
    {
        sphere.enabled = false;
        //visual.gameObject.SetActive(false);
        GameEventsManager.instance.goldEvents.GoldGained(goldGained);
        GameEventsManager.instance.miscEvents.CoinCollected();
        StopAllCoroutines();
        StartCoroutine(RespawnAfterTime());
    }

    private IEnumerator RespawnAfterTime()
    {
        yield return new WaitForSeconds(respawnTimeSeconds);
        sphere.enabled = true;
        //visual.gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        if (otherCollider.CompareTag("Player"))
        {
            CollectCoin();
        }
    }
}