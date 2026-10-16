using UnityEngine;

public class Camera : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSpeed = 100f;
    public float gravity = 9.81f;

    private float rotationX = 0f;
    private CharacterController controller;
    private float verticalVelocity = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);
        transform.localRotation = Quaternion.Euler(rotationX, transform.localRotation.eulerAngles.y, 0);

        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        Vector3 moveDirection = (transform.forward * inputZ) + (transform.right * inputX);
        moveDirection.y = 0;
        moveDirection.Normalize();

        Vector3 finalVelocity = moveDirection * moveSpeed;
        if (controller.isGrounded)
        {
            verticalVelocity = -0.5f;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        finalVelocity.y = verticalVelocity;

        controller.Move(finalVelocity * Time.deltaTime);

    }
}
