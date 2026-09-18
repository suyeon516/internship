using UnityEngine;

public class ProjectileDemo : MonoBehaviour
{
    void Start()
    {
        // === 1. 발사각을 라디안으로 변환 ===
        float th = 30f * Mathf.Deg2Rad; // Unity엔 Deg2Rad 상수가 이미 있음 (30/180*pi 와 동일)

        // === 2. 방향 벡터(단위벡터) ===
        Vector3 direction = new Vector3(Mathf.Cos(th), Mathf.Sin(th), 0);

        // === 3. 속력과 속도 벡터 ===
        float speed = 5f;
        Vector3 velocity = speed * direction;

        // === 4. 화살표 시각화 (Scene 뷰 전용) ===
        Debug.DrawRay(Vector3.zero, direction, Color.white, 100f);
        Debug.DrawRay(Vector3.zero, velocity, Color.cyan, 100f);

        // === 5. 출력 확인 ===
        Debug.Log($"direction: {direction}, mag: {direction.magnitude}");
        Debug.Log($"velocity: {velocity}, mag: {velocity.magnitude}");
    }
}