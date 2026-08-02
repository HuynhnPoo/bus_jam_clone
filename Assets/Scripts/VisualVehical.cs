using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualVehical : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private MeshRenderer meshRenderers;
  
    public void SetupVehicle(Color color)
    {
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock(); // khoi tạo 
       // this.meshRenderers = meshRenderer;


        propertyBlock.SetColor("_Color",color);
        meshRenderers.SetPropertyBlock(propertyBlock);

    }

    private void Awake()
    {
        meshRenderers = GetComponent<MeshRenderer>();
    }

    private Vector3 DirToVector3(MoveDirection d)
    {
        return d switch
        {
            MoveDirection.Up    => Vector3.forward,
            MoveDirection.Down  => Vector3.back,
            MoveDirection.Left  => Vector3.left,
            MoveDirection.Right => Vector3.right,
            _ => Vector3.zero
        };
    }
}
