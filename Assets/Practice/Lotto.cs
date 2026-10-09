using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class LottoBall
{
    public GameObject obj;
    public float radius;
    public Vector3 v;
    public Vector3 f;   //공이 받는 힘
    public float m;
    public bool mousedrag = false;
}


public class Lotto: MonoBehaviour
{
    // 통
    GameObject bucket;

    //Start ,Update 외 함수에서 쓰려면 const 선언
    const float radius= 10.0f;   //반지름
    const float side = 10.0f;
    const float thk = 0.3f;     //두께
    const float s2 = 2 * radius - thk;
    const float s3 = 2 * radius + thk;

    public Material glassMat;
    // 공
    List<LottoBall> balls = new List<LottoBall>();

    bool drag = false;
    LottoBall chosen = null;


    bool zeroGravity = false;
    const float g = 9.8f;
    const float ks = 100f;   // 스프링 계수 -> 얼마나 세게 밀어내는가
    const float kd = 1f;     // 댐퍼 계수 -> 브레이크를 얼마나 세게 거는가
    const float e = 0.9f;   // 탄성 계수 -> 벽에 부딪혔을 때 속도가 얼마나 남는가

    void OnGUI()
    {
        zeroGravity = GUI.Toggle(new Rect(10, 10, 150, 25), zeroGravity, "zero-gravity");
    }

    float t = 0f;
    float dt = 0.01f;

    void Start()
    {
        Camera.main.transform.position = new Vector3(0, 0, 30);
        Camera.main.transform.LookAt(Vector3.zero);
        makeBucket();
        for (int i = 0; i < 45; i++)
        {
            LottoBall b = new LottoBall();
            b.radius = 1.0f;
            b.obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            b.obj.transform.localScale = Vector3.one * b.radius * 2f;
            b.obj.GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);

            // pos
            Vector3 pos = 0.8f * radius * RandomVec();
            pos.z = side - 2;
            b.obj.transform.position = pos;

            // v 
            b.v = radius * RandomVec();
            b.v.z = 0;

            b.m = b.radius * b.radius * b.radius;

            balls.Add(b);
        }

    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;
        if (mouse.leftButton.wasPressedThisFrame) down();
        if (mouse.leftButton.isPressed) move();
        if (mouse.leftButton.wasReleasedThisFrame) up();

        for (int i = 0; i < balls.Count; i++)
        {
            LottoBall bi = balls[i];

            // 중력
            bi.f = zeroGravity ? Vector3.zero : bi.m * new Vector3(0, -g, 0);
            // 다른 공과 충돌했는지
            for (int j = 0; j < balls.Count; j++)
            {
                if (i == j) continue;
                LottoBall bj = balls[j];

                Vector3 rij = bi.obj.transform.position - bj.obj.transform.position;    //나 - 상대 : 상대에서 나를 향하는 화살표
                Vector3 vij = bi.v - bj.v;
                float sumRadius = bi.radius + bj.radius;

                if (sumRadius > rij.magnitude)
                {
                    Vector3 n = rij.normalized;                      // norm(r_ij)
                    bi.f -= ks * (rij.magnitude - sumRadius) * n;    // 스프링
                    bi.f -= kd * Vector3.Dot(vij, n) * n;            // 감쇠
                }
            }
        }
        for (int i = 0; i < balls.Count; i++)
        {   //벽과 부딪힘
            LottoBall b = balls[i];
            float limit = radius-b.radius;

            Vector3 pos = b.obj.transform.position;
            if (pos.magnitude > limit)
            {
                Vector3 n = pos.normalized;                  // 원점 → 공 방향 (벽의 법선)
                b.obj.transform.position = n * limit;        // 위치를 한계선 안으로 되돌림

                float vn = Vector3.Dot(b.v, n);              // 속도 중 벽 방향 성분
                if (vn > 0)                                  // 벽 쪽으로 나가는 중일 때만 반사
                    b.v -= (1 + e) * vn * n;
            }
        }

        for (int i = 0; i < balls.Count; i++)
        {
            if (balls[i].mousedrag == false)
            {
                balls[i].v += balls[i].f / balls[i].m * dt;
                balls[i].obj.transform.position += balls[i].v * dt;
            }

        }
        t += dt;
    }

    void makeBucket()
    {
       
        bucket = GameObject.CreatePrimitive(PrimitiveType.Sphere); 
        bucket.transform.localScale = Vector3.one * radius * 2f;
        bucket.GetComponent<Renderer>().material = glassMat;

        // pos
        Vector3 pos = Vector3.zero;
        bucket.transform.position = pos;
    }

    Vector3 RandomVec() //랜덤 백터 만들기
    {
        return new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f));
    }

    // 조작함수
    void down()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            foreach (LottoBall b in balls)
            {
                if (b.obj == hit.collider.gameObject)
                {
                    chosen = b;
                    drag = true;
                    break;
                }
            }
        }
    }
    void move()
    {
        if (!drag || chosen == null) return;

        // 마우스 광선이 z = side-2 평면과 만나는 점 = scene.mouse.pos 대체
        Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, side - 2));
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 p = ray.GetPoint(enter);

            if (-side < p.x && p.x < side && -side < p.y && p.y < side)
            {
                chosen.obj.transform.position = new Vector3(p.x, p.y, side - 2);
                chosen.mousedrag = true;
            }
        }
    }
    void up()
    {
        drag = false;
        if (chosen != null) chosen.mousedrag = false;
        chosen = null;
    }
}
