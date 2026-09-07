using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleState
{
    public int id;
    public Color color;
    public int capacity;        // số người xe có thể chứa
    public int currentOccupied; // số người đã lên xe
    public int length;
    public Vector3Int position;
    public MoveDirection direction;
    public bool isActive;

    public bool IsFull => currentOccupied >= capacity;
    public int AvailableSeats => capacity - currentOccupied;

    public void GotOnBus()
    {
        if (currentOccupied < capacity)
            currentOccupied++;
    }
}

public class VisualVehical : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderers;
    MaterialPropertyBlock propertyBlock;
    public void SetupVehicle(Color color)
    {
        if (propertyBlock == null) return;

        meshRenderers.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", color);
        meshRenderers.SetPropertyBlock(propertyBlock);
    }

    private void Awake()
    {
        meshRenderers = GetComponent<MeshRenderer>();
        propertyBlock = new MaterialPropertyBlock(); // khoi tạo 
    }

    private Vector3 DirToVector3(MoveDirection d)
    {
        return d switch
        {
            MoveDirection.Up => Vector3.forward,
            MoveDirection.Down => Vector3.back,
            MoveDirection.Left => Vector3.left,
            MoveDirection.Right => Vector3.right,
            _ => Vector3.zero
        };
    }
}
