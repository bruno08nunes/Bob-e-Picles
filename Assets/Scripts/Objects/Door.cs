using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    [SerializeField] UnityEvent onEnterDoor;

    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (onEnterDoor == null)
        {
            Debug.Log("Door without function");
            return;
        }

        spriteRenderer.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        onEnterDoor?.Invoke();
    }

    public void GoToUpRoom()
    {
        SceneController.GoToUpScene();
    }

    public void GoToLeftRoom()
    {
        SceneController.GoToLeftScene();
    }

    public void GoToRighttRoom()
    {
        SceneController.GoToRightScene();
    }

    public void GoToDownRoom()
    {
        SceneController.GoToDownScene();
    }
}
