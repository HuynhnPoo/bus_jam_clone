using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinCount : MonoBehaviour
{
    int coinCount = 0;
    float delayTime = 0.1f;

   private CoinSpawner coinSpawner;
    // Start is called before the first frame update
    private void Awake()
    {
        coinSpawner = GetComponent<CoinSpawner>();
    }
    void Start()
    {
        StartCoroutine(CountCoinRoutine());
    }

    IEnumerator CountCoinRoutine(){
        while (coinCount < GameManager.Instance.ReachedCoin)
        {

            GameMechanics.Add(1);
           // Debug.Log(1);
            coinCount++;

            yield return new WaitForSeconds(delayTime);
        }

        coinSpawner.Spawn();
    }

   
}
