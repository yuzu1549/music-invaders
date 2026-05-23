using UnityEngine;

public class EnemySnakeMover : MonoBehaviour
{
    public enum MoveDirection
    {
        Right,
        Left
    }

    [Header("移動設定")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float downDistance = 1f;
    [SerializeField] private MoveDirection firstDirection = MoveDirection.Right;

    private Vector2 horizontalDirection;
    private bool isMovingDown;
    private float targetY;

    private void Start()
    {
        horizontalDirection = firstDirection == MoveDirection.Right
            ? Vector2.right
            : Vector2.left;
    }

    private void Update()
    {
        if (isMovingDown)
        {
            MoveDown();
        }
        else
        {
            MoveHorizontal();
        }
    }

    private void MoveHorizontal()
    {
        transform.position += (Vector3)(horizontalDirection * speed * Time.deltaTime);
    }

    private void MoveDown()
    {
        Vector3 targetPosition = new Vector3(
            transform.position.x,
            targetY,
            transform.position.z
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        if (Mathf.Approximately(transform.position.y, targetY))
        {
            isMovingDown = false;
            horizontalDirection *= -1f;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Wall")) return;
        StartMoveDown();
    }

    private void StartMoveDown()
    {
        isMovingDown = true;
        targetY = transform.position.y - downDistance;
    }

    public void SetMoveSettings(float newSpeed, float newDownDistance, MoveDirection newFirstDirection)
    {
        speed = newSpeed;
        downDistance = newDownDistance;
        firstDirection = newFirstDirection;

        horizontalDirection = firstDirection == MoveDirection.Right
            ? Vector2.right
            : Vector2.left;
    }
}
