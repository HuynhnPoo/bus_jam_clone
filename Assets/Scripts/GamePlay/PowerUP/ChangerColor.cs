using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class ChangerColor : PowerUpBase
{
    private int amout = 1;
    private PersonVisual _personVisual;

    private void Awake()
    {
        BoostType = BoostType.ColorChangeBoost;
    }

    public void SetPersonVisual(PersonVisual personVisual)
    {
        this._personVisual = personVisual;
    }

    protected override void ExecutePowerUp()
    {
        List<GameObject> firstGroup = GetFirstPersonGroup();
        if (firstGroup.Count == 0) return;

        VehicleState targetVehicle = GetVehiclesStateForColorChange();

        Color newColor =targetVehicle.color;
        int newGroupId = targetVehicle.id;

        foreach (GameObject group in firstGroup) 
        {
            if (group == null) continue;
            PersonVisual personVisual = group.GetComponent<PersonVisual>();

            if (personVisual == null) return;
            personVisual.Setup(newColor, newGroupId);
        }
    }
    private VehicleState GetVehiclesStateForColorChange()
    {
        GridManager gridManager = GameManager.Instance.gridManager;

        if (gridManager == null && gridManager.Board == null) return null;

        List<VehicleState> availableVehicles = gridManager.Board.vehicles;// lấy các 
        return availableVehicles.FirstOrDefault();
    }

    public List<GameObject> GetFirstPersonGroup()
    {
        List<GameObject> group = new List<GameObject>();
        List<GameObject> line = GameManager.Instance.LinePperson;

        PersonVisual firstPersonVisual = line[0].GetComponentInChildren<PersonVisual>();
        if (firstPersonVisual == null) return group;

        int targetGroupId = firstPersonVisual.GroupPesonId;
        foreach (GameObject person in line)
        {
            if (person == null) break;

           PersonVisual personVisual = person.GetComponentInChildren<PersonVisual>();
            if (personVisual != null && personVisual.GroupPesonId == targetGroupId)
            {
                group.Add(person);
            }
            else
            {
                break;
            }
        }
        return group;
    }

}
