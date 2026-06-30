using System;
using UnityEngine;

// 상점 거래 성공 이벤트 관리
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public event Action OnTradeSuccess;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // 거래 성공 알림
    public void NotifyTradeSuccess()
    {
        OnTradeSuccess?.Invoke();
    }
}