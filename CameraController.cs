using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform orbitCenter;          // 중심점
    public float radius = 60f;             // 원형 궤도 반지름
    public float height = -60f;            // 카메라 높이
    public float tiltAngle = 60f;          // z축 아래로 보는 각도
    public float totalFrames = 100000f;    // 한 바퀴 도는 프레임 수
   
    private float speedPerFrame;
    private float currentAngle = 0f;
    private bool isOrbiting = false;
    // 이동 속도
    public float moveSpeed = 5f;
    // 회전 속도
    public float rotationSpeed = 100f;
    // === 카메라 줌 관련 ===
    public float zoomSpeed = 40f;          // 줌 속도 (field of view 단위)
    public float minFOV = 20f;             // 최소 FOV (줌 인 한계)
    public float maxFOV = 80f;             // 최대 FOV (줌 아웃 한계)
        // === 원형 회전 관련 ===
    void Start()
    {
        speedPerFrame = 360f / totalFrames;
    }
    // Upd

    void Update()
    {
        // === 방향키 이동 (카메라 기준) ===
        float moveX = Input.GetKey(KeyCode.RightArrow) ? 3 : Input.GetKey(KeyCode.LeftArrow) ? -3 : 0;
        float moveZ = Input.GetKey(KeyCode.PageUp) ? 3 : Input.GetKey(KeyCode.PageDown) ? -3 : 0;
        float moveY = Input.GetKey(KeyCode.UpArrow) ? 3 : Input.GetKey(KeyCode.DownArrow) ? -3 : 0;

        // 카메라의 로컬 방향 기준으로 이동 계산
        Vector3 move = (transform.right * moveX) + (transform.up *moveY ) + (transform.forward * moveZ);
        transform.position += move * moveSpeed * Time.deltaTime;

        // === WASD 회전 ===
        float rotX = 0f;
        float rotY = 0f;

        if (Input.GetKey(KeyCode.W)) rotX = -1f;
        if (Input.GetKey(KeyCode.S)) rotX = 1f;
        if (Input.GetKey(KeyCode.A)) rotY = -1f;
        if (Input.GetKey(KeyCode.D)) rotY = 1f;

        Vector3 rotation = new Vector3(rotX, rotY, 0f) * rotationSpeed * Time.deltaTime;
        transform.eulerAngles += rotation;

         // === 카메라 줌 조절 ===
        if (Input.GetKey(KeyCode.Z))
        {
            Camera.main.fieldOfView -= zoomSpeed * Time.deltaTime;
            Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, minFOV, maxFOV);
        }

        if (Input.GetKey(KeyCode.X))
        {
            Camera.main.fieldOfView += zoomSpeed * Time.deltaTime;
            Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, minFOV, maxFOV);
        }
         // === 궤도 회전 토글 ===
        if (Input.GetKeyDown(KeyCode.R))
        {
            isOrbiting = !isOrbiting;
        }
        if (isOrbiting)
        {
            // === 자동 궤도 회전 ===
            currentAngle += speedPerFrame;
            float rad = currentAngle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Cos(rad) * radius,
                height,
                Mathf.Sin(rad) * radius
            );

            transform.position = orbitCenter.position + offset;

            Quaternion lookRotation = Quaternion.LookRotation(orbitCenter.position - transform.position);
            Quaternion tilt = Quaternion.Euler(tiltAngle, 0f, 0f);
            transform.rotation = lookRotation * tilt;
        }
    }
}
