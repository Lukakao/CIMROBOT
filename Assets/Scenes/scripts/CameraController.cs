using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Transform target;
    Transform initialTarget;
    Camera secondCamera;
    GameObject bp_above;
    GameObject bp_side;
    GameObject bp_front;

    private bool isPerspective = true;
    public Material mPanel_hover;
    public float startDistance = 100f;

    public Material mDefault_material;
    public GameObject abp_above;
    public GameObject abp_side;
    public GameObject abp_front;
    float initialSmoothSpeed;
    float smoothSpeed = 10f;

    [SerializeField] List<Transform> targets;

    void Start()
    {
        yaw = -20;
        pitch= 40;
        distance = startDistance;
        initialSmoothSpeed = smoothSpeed;
        secondCamera = GameObject.FindGameObjectWithTag("second_camera").GetComponent<Camera>();
        bp_above = GameObject.FindGameObjectWithTag("bp_above");
        bp_side = GameObject.FindGameObjectWithTag("bp_side");
        bp_front = GameObject.FindGameObjectWithTag("bp_front");

        bp_above.SetActive(true);
        bp_side.SetActive(false);
        bp_front.SetActive(false);

        initialTarget = targets[0].transform;
        target = initialTarget;
        finalTargetPos = target.position;
    }

    Vector3 finalTargetPos;
    public float speed = 10f;
    public float distance = 5f;
    public float sensitivity = 3f;

    public float minPitch = -80f;
    public float maxPitch = 80f;
    float yaw;
    float pitch;


    public float zoomSpeed = 3f;
    public float minDistance = 1.5f;
    public float maxDistance = 80;
    private float hor;
    private float ver;
    private float mx;
    private float my;

    public float panSpeed = 50f;

    public bool canMove = true;
    Transform orbitTarget;
    bool orbiting = false;
    Vector3 prevPosition;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) //:v
        {
            Application.Quit();
        }
        if(!canMove) return;
        if (orbiting && orbitTarget != null) finalTargetPos = orbitTarget.position;
        hor = Input.GetAxis("Horizontal");
        ver = Input.GetAxis("Vertical");
        mx = Input.GetAxis("Mouse X");
        my = Input.GetAxis("Mouse Y");
        
        for (int i = 0; i < targets.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                OrbitPoint(i);
                Debug.Log("should orbit " + i);
                break;
            }
        }
        Vector3 moveVec = transform.right * hor + transform.forward * ver;
        MovePlanar(moveVec);
        MoveHeight();
        Orbit();
        //if(Input.GetMouseButton(2)) Pan();
        HandleZoom();
        HandlePlanes();
    }
    void MoveHeight()
    {
        int up = 0;
        int down = 0;
        if(Input.GetKey(KeyCode.LeftShift)) up = 1;
        if(Input.GetKey(KeyCode.LeftControl)) down = -1;
        if(up+down == 0) return;
        MovePlanar(new Vector3(0,up+down,0));
    }
    void OrbitPoint(int index)
    {
        //con numericos igual
        orbiting = true;
        orbitTarget = targets[index];
        finalTargetPos = orbitTarget.position;
    }
    void Pan()
    {
        Vector3 panVec = (transform.up * panSpeed * Time.deltaTime * -my + transform.right * panSpeed * Time.deltaTime * -mx)*distance;
        MovePlanar(panVec);
    }
    void Orbit()
    {
        if (Input.GetMouseButton(1))
        {
            SwitchToPerspective();

            yaw   += mx * sensitivity;
            pitch -= my * sensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.001f)
        {
            distance -= scroll * zoomSpeed*distance;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }
    void MovePlanar(Vector3 moveVector)
    {
        if (moveVector.sqrMagnitude > 0.0001f) orbiting = false;
        finalTargetPos += moveVector * speed * Time.deltaTime;
    }

    void LateUpdate()
    {
        if (!isPerspective) return;
        
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -distance);
        transform.position = finalTargetPos + offset;
        Vector3 copy = transform.position;
        if(transform.position.y< 1) {
            copy.y = 1;
            transform.position = copy;
        }
        transform.rotation = rotation;

    }

    void HandlePlanes()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = secondCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("arrows_above"))
                {
                    GoToOrthographicPlane(new Vector3(0f, 60f, 0f),new Vector3(90f, 0f, 0f));
                    bp_side.SetActive(false);
                    bp_front.SetActive(false);
                }
                else if (hit.collider.CompareTag("arrows_side"))
                {
                    GoToOrthographicPlane(new Vector3(0f, 10f, -60f),new Vector3(0f, 0f, 0f));
                    bp_side.SetActive(true);
                    bp_front.SetActive(false);
                }
                else if (hit.collider.CompareTag("arrows_front"))
                {
                    GoToOrthographicPlane(new Vector3(60f, 10f, 0f),new Vector3(0f, -90f, 0f));
                    bp_side.SetActive(false);
                    bp_front.SetActive(true);
                }
            }
        }
        abp_above.GetComponent<Renderer>().material = mDefault_material;
        abp_side.GetComponent<Renderer>().material = mDefault_material;
        abp_front.GetComponent<Renderer>().material = mDefault_material;

        Ray rayo = secondCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit choque;

        if (Physics.Raycast(rayo, out choque))
        {
            string tag = choque.collider.tag;

            if (tag == "arrows_above")
                choque.collider.GetComponent<Renderer>().material = mPanel_hover;

            if (tag == "arrows_side")
                choque.collider.GetComponent<Renderer>().material = mPanel_hover;

            if (tag == "arrows_front")
                choque.collider.GetComponent<Renderer>().material = mPanel_hover;
        }
    }

    void GoToOrthographicPlane(Vector3 move, Vector3 rotate)
    {
        prevPosition = transform.position;
        isPerspective = false;
        finalTargetPos = move;
        transform.position = move;
        transform.eulerAngles = rotate;
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 25;
    }    

    void SwitchToPerspective()
    {
        isPerspective = true; Camera.main.orthographic = false;
        bp_side.SetActive(false);
        bp_front.SetActive(false);
        finalTargetPos = target.position;
    }
}
