using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CamController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float zoomSensitivity;
    [SerializeField] private float movingSmoothness;
    [SerializeField] private float zoomSmoothness;
    private Camera cam;
    private Vector3 targetPos;
    private float targetZoom;
   
    // Start is called before the first frame update
    void Start()
    {
        targetPos = transform.position;
        cam = GetComponent<Camera>();
        targetZoom = cam.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        CalculateTargetPos();
        Move();
        CalculateTargetZoom();
        Zoom();
    }
    private void CalculateTargetPos()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 delta = new Vector3(h, v, 0) * speed*cam.orthographicSize/10;
        if(delta.magnitude > ((transform.position - targetPos).magnitude))
        {
            targetPos = transform.position + delta;
        }
    }
    private void Move()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, Mathf.Exp(-movingSmoothness));
    }
    private void CalculateTargetZoom()
    {
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        float delta = -scroll * zoomSensitivity*cam.orthographicSize/10;
        if(Mathf.Abs(delta) > Mathf.Abs(cam.orthographicSize - targetZoom))
        {
            targetZoom = cam.orthographicSize + delta;
        }

    }
    private void Zoom()
    {
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Mathf.Exp(-zoomSmoothness));
    }

}
