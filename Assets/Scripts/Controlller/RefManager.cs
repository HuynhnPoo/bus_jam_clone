using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefManager : MonoBehaviour
{
    // Panel chính thứ nhất (Ví dụ: Main Menu hoặc Shop Items)
    [Tooltip("")]
    [SerializeField] private GameObject first_UIGO;   // game play pause
                                                      //main menu button new game

    [SerializeField] private GameObject second_UIGO;// game play status reulst game
                                                    //main menu panel shop

    [SerializeField] private GameObject thirdUI_GO;  //main menu panel shop


    private void Awake()
    {
        UIManager.Instance.Setup(first_UIGO, second_UIGO, thirdUI_GO);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
