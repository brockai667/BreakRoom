using UnityEngine;
using UnityEngine.InputSystem;

/// Prvoosobový pohyb hráča (CharacterController): WASD, skok, myš na rozhliadanie.
/// Citlivosť a FOV číta z PlayerPrefs. Rešpektuje pauzu a koniec kola.
public class PlayerController : MonoBehaviour
{
    public float speed = 6f;
    public float mouseSensitivity = 2f;
    public float jumpHeight = 1.4f;
    public float gravity = -20f;

    private float verticalRotation = 0f;
    private CharacterController controller;
    private Camera playerCamera;
    private Vector3 velocity;
    private PauseMenu pauseMenu;

    void Start()
    {
        controller   = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        pauseMenu    = FindFirstObjectByType<PauseMenu>();

        // Nastavenia z menu (citlivost + FOV)
        mouseSensitivity = PlayerPrefs.GetFloat("Sensitivity", mouseSensitivity);
        float fov = PlayerPrefs.GetFloat("FOV", 0f);
        if (fov > 1f && playerCamera != null) playerCamera.fieldOfView = fov;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Keyboard.current == null || Mouse.current == null) return;

        if (pauseMenu != null && pauseMenu.IsPaused) return;
        if (GameManager.Instance != null && !GameManager.Instance.roundActive) return;

        bool grounded = controller.isGrounded;
        if (grounded && velocity.y < 0f) velocity.y = -2f;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime * 10f;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime * 10f;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        float x = 0f, z = 0f;
        if (Keyboard.current.dKey.isPressed) x = 1f;
        if (Keyboard.current.aKey.isPressed) x = -1f;
        if (Keyboard.current.wKey.isPressed) z = 1f;
        if (Keyboard.current.sKey.isPressed) z = -1f;
        Vector3 move = (transform.right * x + transform.forward * z);
        if (move.sqrMagnitude > 1f) move.Normalize();
        controller.Move(move * speed * Time.deltaTime);

        if (grounded && Keyboard.current.spaceKey.wasPressedThisFrame)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
