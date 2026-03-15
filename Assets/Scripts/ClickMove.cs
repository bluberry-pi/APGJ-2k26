using UnityEngine;

public class ClickMove : MonoBehaviour
{
    public float speed = 5f;
    public Vector2 targetPosition;

    private bool isMoving = false;

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.deltaTime
            );

            if (Vector2.Distance(transform.position, targetPosition) < 0.05f)
            {
                isMoving = false;
            }
        }
    }

    void OnMouseDown()
    {
        isMoving = true;
    }
}