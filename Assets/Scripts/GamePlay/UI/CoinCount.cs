using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinCount : MonoBehaviour
{
    int startCoin = 0;
    float delayTime = 0.1f;



    [Header("time")]
    float totalDuration = 2f;
    float elapsedTime = 0f;

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

    IEnumerator CountCoinRoutine()
    {
        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / totalDuration;
            int currentCoinValue = Mathf.RoundToInt(Mathf.Lerp(startCoin, GameManager.Instance.ReachedCoin, progress));

            int coinDelta = currentCoinValue - startCoin;
            if (coinDelta > 0)
            {
                GameMechanics.Add(coinDelta);
                startCoin = currentCoinValue;
            }
            // Debug.Log(1);
            // startCoin++;

            yield return null;
        }

        //int remainingDelta = GameManager.Instance.ReachedCoin - startCoin;
        //if(remainingDelta > 0)
        //{
        //    GameMechanics.Add(-remainingDelta);
        //    startCoin = GameManager.Instance.ReachedCoin;
        //}
        coinSpawner.Spawn();

        GameManager.Instance.totalCoin += GameManager.Instance.ReachedCoin;
        PlayerPrefs.SetInt(Constans.coinSaveKey, GameManager.Instance.totalCoin);
        PlayerPrefs.Save();

    }


}
