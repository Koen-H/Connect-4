using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraController : MonoBehaviour
{
    private Transform target;

    [Header("Camera settings")]
    [SerializeField]
    private float rotateSpeed = 5f;
    [SerializeField]
    private float zoomSpeed = 5f;

    [Header("In-scene Objects")]
    [SerializeField]
    private GameBoard board;


    private void Awake()
    {
        board.OnGameBoardGenerated += SetTarget;
    }

    private void OnDisable()
    {
        board.OnGameBoardGenerated -= SetTarget;
    }

    /// <summary>
    /// Set the target to the center of the board
    /// </summary>
    private void SetTarget()
    {
        target = board.GetCenter();
        this.transform.position = target.position;
        this.transform.position -= new Vector3(0, 0, 10);
    }


    private void Update()
    {
        if (target == null) return;
        HandleZoomInput();
        HandleRotationInput();
    }

    private void HandleZoomInput()
    {
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        Vector3 zoom = new Vector3(0f, 0f, scrollWheel * zoomSpeed);
        transform.Translate(zoom, Space.Self);
    }

    /// <summary>
    /// Rotates the camera around a target transform.
    /// </summary>
    private void HandleRotationInput()
    {
        if (!Input.GetKey(KeyCode.Mouse1)) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.RotateAround(target.position, Vector3.up, mouseX * rotateSpeed);
        transform.RotateAround(target.position, transform.right, -mouseY * rotateSpeed);
    }

}