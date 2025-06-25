using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public GameObject cubePrefab;
    public Transform player;
    public Transform surroundingWalls;
    public float spawnHeight = 10f;
    public float spawnInterval = 5f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnCube();
            timer = 0;
        }
    }

    void SpawnCube()
    {
        Vector3 cylinderCenter = surroundingWalls.position;
        float radius = Mathf.Abs(surroundingWalls.GetChild(0).position.x);

        Vector3 forwardDir = player.forward;
        forwardDir.y = 0;
        forwardDir.Normalize();

        float angleOffset = Random.Range(-30f, 30f);
        Quaternion rot = Quaternion.AngleAxis(angleOffset, Vector3.up);
        Vector3 spawnDir = rot * forwardDir;

        float safeOffset = 0.2f;
        Vector3 spawnPos = cylinderCenter + spawnDir * (radius + safeOffset);
        spawnPos.y = spawnHeight;

        GameObject cube = Instantiate(cubePrefab, spawnPos, Quaternion.identity);
        CubeController controller = cube.GetComponent<CubeController>();
        Vector3 direction = (player.position - spawnPos);
        direction.y = 0; // y축 제거
        controller.targetDirection = direction.normalized;
        controller.moveSpeed = Random.Range(100f, 150f);
        Debug.DrawLine(spawnPos, player.position, Color.red, 2f);
    }
}