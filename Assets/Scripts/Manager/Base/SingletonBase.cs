using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public enum EventGameType
{
    Pause,
    Resume,
    
    //
    SoundOn,
    SoundOff,

    //
    GameStart,
    GameOver,
    GameWon

}
public static class EventManager
{
    private static Dictionary<EventGameType, Action> _events = new Dictionary<EventGameType, Action>();

    public  static void Subscribe(EventGameType eventGame, Action listener)
    {
        if (_events.ContainsKey(eventGame)) _events[eventGame] += listener;
        else _events.Add(eventGame, listener);
    }

    public static void UnSubscribe(EventGameType eventGame, Action listener)
    {
        if (_events.ContainsKey(eventGame))
        {
            _events[eventGame] -= listener;
            if (_events[eventGame] == null)
            {
                _events.Remove(eventGame);
            }
        }
    }

    public static void Notify(EventGameType eventGame)
    {
        if (_events.TryGetValue(eventGame, out Action listener))
        {
            Debug.Log($"[EVENT] Notify: {eventGame} | Listener: {listener}");
            listener?.Invoke();
        }
        else
        {
            Debug.LogWarning($"[EVENT] {eventGame} không có listener!");
        }
    }

    public static void ClearEvent(EventGameType eventGame)
    {
        if (_events.ContainsKey(eventGame)) _events.Remove(eventGame);
    }

    public static void ClearAllEvents()
    {
        _events.Clear();
    }

}

public class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
{
    private  static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    instance = obj.AddComponent<T>();
                }
            }
            return instance;
        }
    }

   
    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this as T;

        DontDestroyOnLoad(gameObject);
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}

