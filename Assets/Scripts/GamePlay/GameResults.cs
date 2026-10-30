using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameResults : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) 
        {
            GameManager.isGameOver = true;
            //GameManager.isWon = true;
        }

        if (GameManager.isGameOver)
        {
            EventManager.Notify(EventGameType.GameOver);
        }
        if (GameManager.isWon) EventManager.Notify(EventGameType.GameWon);
    }
}
