using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangerColor : PowerUpBase
{
    private int amout = 1;
    private PersonVisual _personVisual;
    public void SetPersonVisual(PersonVisual personVisual)
    {
        this._personVisual = personVisual;
    }

    protected override void ExecutePowerUp()
    {
        //throw new System.NotImplementedException();
    }

    public List<GameObject> GetFirstPersonGroup()
    {
        List<GameObject> group = new List<GameObject>();
        List<GameObject> line = GameManager.Instance.LinePperson;

        PersonVisual personVisual = line[0].GetComponentInChildren<PersonVisual>();
        if (personVisual == null) return group;

        Color firstColor = personVisual.ColorPerson;
        foreach (GameObject person in line)
        {
            if (person == null) break;

        }
            


        
        

        return group;
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
