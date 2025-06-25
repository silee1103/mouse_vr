using UnityEngine;

public class CubeController : MonoBehaviour
{
    public Vector3 targetDirection;
    public float moveSpeed;
    void Update()
    {
        transform.position += targetDirection * moveSpeed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.CompareTag("Animal") ||
            collision.gameObject.CompareTag("Floor") ||
            collision.gameObject.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
