using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private float timeDelaycoin = 0.05f;
    [SerializeField] private Ease ease = Ease.OutQuad;

    [SerializeField] private GameObject coinPrefabs;
    [SerializeField] private ObjectPool pool;
    [SerializeField] private RectTransform pivot;
    private void OnEnable()
    {
        //pool = GetComponent<ObjectPool>();
        pool.CreatePool(Constans.coinKeyPool, coinPrefabs, 20, this.transform);
    }
    private void Update()
    {
       // if (Input.GetKeyDown(KeyCode.S)) Spawn();
    }
    // Hàm sinh và di chuyển 20 coin cùng lúc
    public void Spawn()
    {
        int coinCount = 20;

        for (int i = 0; i < coinCount; i++)
        {
            // 1. Lấy coin từ ObjectPool (nhớ truyền key tương ứng trong Pool của bạn)
            GameObject coin = pool.Get(Constans.coinKeyPool);

            if (coin == null) continue;
            RectTransform coinRect = coin.GetComponent<RectTransform>();

            // 2. Random vị trí xuất hiện xung quanh điểm spawn
            Vector2 randomOffset = Random.insideUnitCircle * Random.Range(180, 220); // Tạo độ phân tán nhỏ cho đẹp mắt
            coinRect.anchoredPosition += randomOffset;

            Vector2 targetPos = coinRect.parent.InverseTransformPoint(pivot.position);
            // 3. Di chuyển coin tới pivot bằng DOTween
            coinRect.DOKill();
            coinRect.DOAnchorPos(targetPos, speed).SetDelay(i * timeDelaycoin)
                .SetEase(ease)
                .OnComplete(() =>
                {
                    // 4. Trả coin về pool sau khi bay xong

                    pool.ReturnToPool(Constans.coinKeyPool, coinRect.gameObject);
                    coinRect.anchoredPosition = Vector2.zero;
                });
        }
    }
}