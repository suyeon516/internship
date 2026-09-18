using UnityEngine;

public class VectorDemo : MonoBehaviour
{
    void Start()
    {
        // === 1. x, y, z 축 화살표 (Scene 뷰에서만 보임, Game 뷰엔 안 나옴) ===
        Debug.DrawRay(Vector3.zero, new Vector3(10, 0, 0), Color.red, 100f);
        Debug.DrawRay(Vector3.zero, new Vector3(0, 10, 0), Color.green, 100f);
        Debug.DrawRay(Vector3.zero, new Vector3(0, 0, 10), Color.blue, 100f);

        // === 2. ball 생성 ===
        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.transform.position = new Vector3(3, 4, 5);
        ball.transform.localScale = Vector3.one * 0.2f * 2; // radius=0.2 → 지름은 반지름*2

        // === 3. 스칼라곱(나눗셈): 위치를 절반으로 ===
        ball.transform.position = ball.transform.position / 2.0f;
        // Vector3(1.5, 2, 2.5) 가 됨 ? x,y,z 각각 2로 나눈 것

        // === 4. 벡터 크기(magnitude) 계산 ===
        float ballMag = ball.transform.position.magnitude; // 내장 프로퍼티
        float checkBallMag = Mathf.Sqrt(
            ball.transform.position.x * ball.transform.position.x +
            ball.transform.position.y * ball.transform.position.y +
            ball.transform.position.z * ball.transform.position.z
        );
        Debug.Log($"mag: {ballMag}, check: {checkBallMag}"); // 둘이 같은 값

        // === 5. 단위 벡터(정규화) ===
        Vector3 ballDir = ball.transform.position / ballMag;
        // 또는 그냥: Vector3 ballDir = ball.transform.position.normalized;

        // === 6. 원점→ball 을 가리키는 노란 화살표 ===
        Debug.DrawRay(Vector3.zero, ball.transform.position, Color.yellow, 100f);
    }
}