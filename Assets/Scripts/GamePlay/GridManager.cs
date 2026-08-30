using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int Width { get; set; }
    public int Height { get; set; }


    [Header("Holder")]
    [SerializeField] private Transform holderSLotParking;
    [SerializeField] private Transform holderVehical;
    [SerializeField] private Transform holderPerson;
    public Transform HolderPerson => holderPerson;


    [Header("Prefabs")]
    [SerializeField] private GameObject[] tilePrefab;       // Prefab ô bàn cờ (1x1)
    [SerializeField] private GameObject personPrefab;

    [SerializeField] private LevelData levelData;
    public Board Board { get; private set; } // Đổi sang public để các hệ thống khác dễ truy cập


    [SerializeField] private Transform[] whiteSlots = new Transform[5];
    public Transform[] WhiteSlots => whiteSlots;
    public GameObject[] slotOccupants { private set; get; }

    public Vector3 posFirtsPerson;

    private void Awake()
    {
        if (holderVehical != null) return;
        holderVehical = transform.GetChild(0);

        whiteSlots = holderSLotParking.GetComponentsInChildren<Transform>()
            .Where(t => t != holderSLotParking)
            .ToArray(); //kiem tra cac con va bo cha

        slotOccupants = new GameObject[whiteSlots.Length];
        posFirtsPerson = holderPerson.transform.position;
    }


    private void Start()
    {
        this.Width = levelData.width;
        this.Height = levelData.height;

        // 1. Khởi tạo Logic Board từ dữ liệu màn chơi
        Board = Board.FromLevelData(levelData);

        // 2. Sinh các ô lưới (Grid Tiles) dựa trên kích thước lưới
        SpawnGrid();// sinh xe

        SpawnPersonGroup(); // sinh nguoi
        //// 3. Sinh các xe (Vehicles) dựa trên danh sách xe trong LevelData
        //SpawnVehicles();
    }
    public void Spawn(LevelData levelData)
    {

    }

    // Hàm tính toán vị trí World Space từ tọa độ Grid (x, y)
    private Vector3 GetWorldPosition(int x, int y)
    {
        // Khoảng cách thực tế giữa các tâm ô = kích thước ô + khoảng cách
        float step = levelData.cellSize + levelData.spacing;

        // Tính toán để tâm của toàn bộ grid nằm ở vị trí (0, 0, 0) của GridManager
        float offsetX = (Width - 1) * step / 2f;
        float offsetY = (Height - 1) * step / 2f;

        return new Vector3(x * step - offsetX, 0, y * step - offsetY);
    }

    private void SpawnGrid()
    {
        if (tilePrefab == null) return;

        foreach (var vehical in Board.vehicles)
        {
            Vector3 posSpawn = Board.GetPostionWorld(vehical.position.x, vehical.position.y);
            Quaternion quaternion = Board.GetRotationWorld(vehical.direction);

            GameObject obj = Instantiate(tilePrefab[vehical.length - 1], posSpawn, quaternion, holderVehical);
            obj.name = $"vehical {vehical.id}";

            VisualVehical visualVehical = obj.GetComponentInChildren<VisualVehical>();

            VehicleController controller = obj.GetComponent<VehicleController>();
            if (controller == null)
            {
                // Nếu trên Prefab chưa gắn sẵn script thì thêm vào
                controller = obj.AddComponent<VehicleController>();
            }
            controller.Setup(vehical);

            visualVehical.SetupVehicle(vehical.color);
           
        }
    }

    void SpawnPersonGroup()
    {
        int totalsGroupPerson = 0;

        foreach (var personGroup in levelData.personGroups)
        {
            VehicleState ownerVehicle = Board.vehicles.FirstOrDefault(v => v.id == personGroup.ownerVehicleId);
            Color personColor = ownerVehicle != null ? ownerVehicle.color : Color.white;
            // Sinh từng người trong nhóm (Group)
            Debug.Log(
       $"GROUP ID: {personGroup.groupPersonId} | " +
       $"OWNER VEHICLE ID: {personGroup.ownerVehicleId} | " +
       $"COLOR: {personColor}"
   );
            for (int i = 0; i < personGroup.Count; i++)
            {
                // Tính khoảng cách đứng giãn cách giữa các người trong cùng 1 nhóm
                Vector3 posPerson = GameMechanics.CalculatePathPosition(totalsGroupPerson,holderPerson.position);

                GameObject personObj = Instantiate(personPrefab, posPerson, Quaternion.identity, holderPerson);
                personObj.name = $"Person_{personGroup.groupPersonId}_{i}";

                // Set up visual màu sắc
                PersonVisual personVisual = personObj.GetComponentInChildren<PersonVisual>();
                if (personVisual != null)
                {
                   // Debug.Log("person Id "+ personGroup.personId);
                    personVisual.Setup(personColor,personGroup.groupPersonId);

                }
                
                totalsGroupPerson++;
               
                GameManager.Instance.LinePperson.Add(personObj); // them nguoi vafo lisst ddeer de quan li
                
                // spawnedPersons.Add(personObj);
            }
        }
    }



    // Xác định góc quay của xe trên trục Y
    private Quaternion GetRotationFromDirection(MoveDirection direction)
    {
        float angle = direction switch
        {
            MoveDirection.Up => 0f,
            MoveDirection.Right => 90f,
            MoveDirection.Down => 180f,
            MoveDirection.Left => 270f,
            _ => 0f
        };
        return Quaternion.Euler(0, angle, 0);
    }
}