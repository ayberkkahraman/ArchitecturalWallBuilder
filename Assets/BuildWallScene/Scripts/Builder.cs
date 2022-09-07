using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Builder : MonoBehaviour
{
    public static Builder instance;

    #region Variables

    [Header("----------Text Area")]
    public Text lengthText;
    public Text angleText;
    public Text gridText;
    [Space]
    [Space]
    public LengthType lengthType;

    public enum LengthType
    {
        Meter = 0,
        Inch = 1
    }

    [Range(.1f, 5f)]  
    float gridSize = .1f;
    private float angle;
    public float wallLength;

    
    [Header("----------Material & Color")]
    public Color completeMatColor;
    public Color defaultMatColor;
    public Material completedMat;
    [Space]
    [Space]

    private bool isDrawingWall;
    private bool canCompleteRoom;
    [HideInInspector]public bool roomBuilded;
    [HideInInspector]public bool isCollidingWithFirstWall;

    [Space]
    [Space]
    [SerializeField] GameObject startPoint;
    [SerializeField] GameObject endPoint;
    private Vector3 previousPointPosition;

    [Space]
    [Space]
    public GameObject wallPrefab;
    [SerializeField] Transform room;

    public GameObject currentWall;

    private Vector3 startPointPosition;
    private Vector3 endPointPosition;

    private GameObject lastPoint;
    private GameObject firstStartPoint;

    private GameObject firstWall;
    private GameObject previousWall;

    private Collider[] objectsWhichColliding;

    public List<Wall> walls = new List<Wall>();
    
    [Space]
    [SerializeField] GameObject mousePointer;

    #endregion
    private void Awake()
    {
        instance = this;
        gridSize = 1f;
    }
    private void Update()
    {
        //Restarts the scene
        if (Input.GetKeyDown(KeyCode.Space)) SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        //This line is a controller for the actions below
        if (roomBuilded == true) return;

        objectsWhichColliding = isDrawingWall ? Physics.OverlapBox(currentWall.transform.position, currentWall.transform.localScale / 2) : null;

        if(mousePointer!=null)mousePointer.transform.position = GetWorldPointFromInput();
        
        GetInput();
 
        //Sets the first wall object's properties
        if (IsRoomEmpty() && firstStartPoint != null) { Destroy(firstStartPoint); }
        else { if(firstWall != null) firstWall.tag = "FirstWall"; }
        

        if (currentWall != null)
        {
            canCompleteRoom = walls.Count > 3 && isCollidingWithFirstWall ? true : false;
            currentWall.GetComponent<MeshRenderer>().material.color = canCompleteRoom ? completeMatColor : defaultMatColor;//Updates the color if canCompleteRoom is true
        }

    }
    void GetInput()//Inputs for the building actions
    {
        //---Wall building beginning
        if (Input.GetMouseButtonDown(0))
        {
            if (!isDrawingWall) CreateWallProcess();
            else 
            {
                if (canCompleteRoom) { CompleteRoom(); }
                else AddWall();
            }   
        }

        //Wall building section
        if (isDrawingWall)
        {
            SetWallProcess();

            if (Input.GetMouseButtonDown(1))
            {
                if(room.transform.childCount > 0)
                {
                    RemoveWall();
                }

            }
        }

        //Wall building ending
        if (Input.GetMouseButtonUp(0)) {FinishWallProcess();}
    }
  
    bool IsRoomEmpty()//Returns true if there is no wall in the room
    {
        return room.childCount > 0 ? false : true;
    }

    int WallCountInRoom()//Returns the wall count in the room
    {
        return room.childCount;
    }

    void CreateWallProcess()//---Creates the first wall in the room
    {
        isDrawingWall = true;

        endPoint.SetActive(true);
        startPoint.SetActive(true);

        //---Set the start point with the mouse cursor
        startPoint.transform.position = GetWorldPointFromInput();

        lastPoint = startPoint;

        firstStartPoint = GameObject.Instantiate(startPoint);

        walls.Add(new Wall(currentWall = (GameObject)Instantiate(wallPrefab, startPoint.transform.position, Quaternion.identity, room), startPoint.transform.position));

        firstWall = currentWall;
    }

    void FinishWallProcess()//---Ends the current wall building section
    {
        endPoint.transform.position = GetWorldPointFromInput();

        endPoint.SetActive(false);
        startPoint.SetActive(false);

    }

    void AddWall()//Adds a wall into the room
    {
        //Updates the wall begininng point(Vector3)
        previousPointPosition = startPoint.transform.position;
        startPoint.transform.position = endPoint.transform.position;
        
        if(currentWall.GetComponent<CollisionDetector>() != null)
            Destroy(currentWall.GetComponent<CollisionDetector>());

        //Updates the previous wall
        previousWall = currentWall;

        walls.Add(new Wall((GameObject)Instantiate(wallPrefab, startPoint.transform.position, Quaternion.identity, room), startPoint.transform.position));   
        
        currentWall = walls[walls.Count - 1].wall;

        if(currentWall.GetComponent<CollisionDetector>() == null)
            currentWall.AddComponent<CollisionDetector>();

        currentWall.tag = "Wall";
        
        SetWallProcess();
        
    }

    void RemoveWall()//Removes the wall which builded last from the room
    {
        walls.Remove(walls.Find(x => x.wall == currentWall));
        Destroy(currentWall);

        #region "Destroying Room" Conditions
        if (walls.Count > 2)
            previousWall = walls[walls.Count - 2].wall;

        else if (walls.Count == 2)
            previousWall = currentWall;

        else if (walls.Count == 1)
        {
            previousWall = null;
            currentWall = firstWall;           
        }

        else
        {
            isDrawingWall = false;

            walls.Remove(walls.Find(x => x.wall == currentWall));

            Destroy(firstWall);
        }
        #endregion

        //Updates the current wall
        if (walls.Count > 0)
            currentWall = walls[walls.Count - 1].wall;

        if (currentWall.GetComponent<CollisionDetector>() == null)
            currentWall.AddComponent<CollisionDetector>();

        currentWall.tag = "Wall";

        //Updates the startPoint of the current wall
        if (walls.Count > 0)
            startPoint.transform.position = walls.Find(x => x.wall == currentWall).wallStartPoint;

        //Takes the previous wall and adjusts with the cursor after updating the wall iteration
        AdjustWall();
    }

    void SetWallProcess()//Sets the end point of the wall with the mouse cursor
    {
        endPoint.transform.position = GetWorldPointFromInput();
        walls.Find(x => x.wall == currentWall).wallEndPoint = endPoint.transform.position;

        AdjustWall();
    }

    void AdjustWall()//Adjusts the wall object
    {
        startPoint.transform.LookAt(endPoint.transform.position);
        endPoint.transform.LookAt(startPoint.transform.position);
        float _distance = Vector3.Distance(startPoint.transform.position, endPoint.transform.position);

        //---Adjusting the wall object 
        currentWall.transform.rotation = startPoint.transform.rotation;
        currentWall.transform.localScale = new Vector3(currentWall.transform.localScale.x, wallLength*2, _distance + endPoint.transform.localScale.z);
        currentWall.transform.position = startPoint.transform.position + _distance / 2 * startPoint.transform.forward;


    }

    void CompleteRoom()//Completes the building section
    {
        firstStartPoint.SetActive(true);
        lastPoint.SetActive(true);

        startPoint = lastPoint;
        endPoint = firstStartPoint;

        AdjustWall();

        firstStartPoint.SetActive(false);
        lastPoint.SetActive(false);

        isDrawingWall = false;
        canCompleteRoom = false;

        firstWall.tag = "Wall";

        isCollidingWithFirstWall = false;


        currentWall.GetComponent<MeshRenderer>().material = completedMat;
        roomBuilded = true;

        Destroy(mousePointer);

        for(int i = 0; i < room.childCount; i++)
        {
            room.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("Wall");
            room.transform.GetChild(i).gameObject.tag = "Wall"; 
        }
    }

    public void DestroyChildObjects(GameObject parentObject)//Destroys the children of the given transform
    {
        for(int i = 0; i < parentObject.transform.childCount; i++)
        {
            Destroy(parentObject.transform.GetChild(i).gameObject);
        }
    }

    Vector3 GetWorldPointFromInput()//Gets the world point from the mouse cursor
    {
        ///Ray ray = ;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit))
        {
            if (hit.collider.name == "Ground") return SnapToGridPoint(hit.point);
            else return SnapToGridPoint(endPoint.transform.position);
        }

        return SnapToGridPoint(Vector3.zero);
    }

    Vector3 SnapToGridPoint(Vector3 _base)//Snaps the point with grid size
    {
        Vector3 snapped;
        snapped.x = Mathf.Round(_base.x / gridSize) * gridSize;
        snapped.y = 0f;
        snapped.z = Mathf.Round(_base.z / gridSize) * gridSize;

        return snapped;
    }

    public int AngleOfCorner()//Gets the angle of between previous wall and the current wall
    {
        Vector3 targetDir = GetWorldPointFromInput() - lastPoint.transform.position;
        Vector3 previousDir = previousWall.transform.position - startPoint.transform.position;
        float angle = Vector3.Angle(targetDir, previousDir);

        return (int)Mathf.Round(angle);
    }

    private void LateUpdate()//This section is for UI actions
    {
        //Clamping the grid size between .1f-5f
        if(gridSize < .1f) { gridSize = .1f; }
        if(gridSize > 5f)  { gridSize = 5f; }
        if(gridSize >= .1f && gridSize <= 5f) { gridSize += Input.mouseScrollDelta.y / 10; }

        //Sets the grid size text 
        gridText.text = gridSize.ToString().Length >= 3 ? $"Grid size is : {gridSize.ToString().Substring(0, 3)}" : $"Grid size is : {gridSize}";

        //Updates the color of the previous wall
        if (previousWall != null) { previousWall.GetComponent<MeshRenderer>().material = completedMat; }

        //This section is a controller for the actions below
        if (!isDrawingWall) {

            lengthText.text = "";
            angleText.text = "";
            return;
        }


        float currentWall = Vector3.Distance(GetWorldPointFromInput(), lastPoint.transform.position) / 20;//Gets the length of the currentWall
        float meterMultiplier = lengthType == LengthType.Inch ? .4f : 1;//Conversion between meter & inch
        string lengthTypeString = lengthType == LengthType.Inch ? "Inch" : "Meter"; //Conversion between meter & inch

        lengthText.text = currentWall.ToString().Length >= 4 ? $"Length of the wall is : {(currentWall * meterMultiplier).ToString().Substring(0, 4)} {lengthTypeString}" : lengthText.text = $"Length of the wall is : {(currentWall * meterMultiplier).ToString()} {lengthTypeString}";

        //This section is a controller for the actions below
        if (previousWall == null || this.currentWall == null) return;


        float distance = Vector3.Distance(previousWall.transform.position, this.currentWall.transform.position) * 2;
        
        angle = AngleOfCorner();//Gets the angle between walls and restores in the angle variable

        angleText.text = angle.ToString().Length >= 4 ? $"Angle of the corner is : {angle.ToString().Substring(0, 4)}° Degree" : $"Angle of the corner is : {angle.ToString()}° Degree";

    }

    private void OnDrawGizmos()//Draws the triangle with the cursor point, previous wall and the current wall
    {
        if (previousWall == null || currentWall == null || roomBuilded) return;

        Gizmos.DrawLine(previousWall.transform.position, currentWall.transform.position);
        Gizmos.DrawLine(previousWall.transform.position, startPoint.transform.position);
        Gizmos.DrawLine(currentWall.transform.position, startPoint.transform.position);
    }
}

[System.Serializable]
public class Wall
{
    public GameObject wall;
    public Vector3 wallStartPoint;
    public Vector3 wallEndPoint;

    public Wall(GameObject _wall, Vector3 _wallStartPoint)
    {
        wall = _wall;
        wallStartPoint = _wallStartPoint;
    }
}