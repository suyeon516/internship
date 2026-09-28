using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Ball
{
    public GameObject obj;   
    public float radius;
    public Vector3 v;
    public Vector3 f;
    public float m;          
    public bool mousedrag = false;
}

public class ForcesAndMotion : MonoBehaviour
{
    // 벽
    List<GameObject> walls = new List<GameObject>();

    //Start ,Update 외 함수에서 쓰려면 const 선언
    const float side = 10.0f;
    const float thk = 0.3f;
    const float s2 = 2 * side - thk;
    const float s3 = 2 * side + thk;

    // 공
    List<Ball> balls = new List<Ball>();

    bool drag = false;
    Ball chosen = null;


    bool zeroGravity = false;
    const float g = 9.8f;    
    const float ks = 100f;   // 스프링 계수
    const float kd = 1f;     // 댐퍼 계수
    const float  e = 0.9f;   // 탄성 계수

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
        makeCube();
        for (int i = 0; i < 50; i++)
        {
            Ball b = new Ball();
            b.radius = 0.5f + Random.value;
            b.obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            b.obj.transform.localScale = Vector3.one * b.radius * 2f;
            b.obj.GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);

            // pos
            Vector3 pos = 0.8f * side * RandomVec();
            pos.z = side - 2;
            b.obj.transform.position = pos;

            // v 
            b.v = side * RandomVec();
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
            Ball bi = balls[i];

            // 중력
            bi.f = zeroGravity ? Vector3.zero : bi.m * new Vector3(0, -g, 0);

            for (int j = 0; j < balls.Count; j++)
            {
                if (i == j) continue;
                Ball bj = balls[j];

                Vector3 rij = bi.obj.transform.position - bj.obj.transform.position;
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
        {
            Ball b = balls[i];
            float sideB = side - thk * 0.5f - b.radius;

            Vector3 pos = b.obj.transform.position;

            if (!(pos.x < sideB && pos.x > -sideB))
            {
                pos.x = Mathf.Min(sideB, pos.x);
                pos.x = Mathf.Max(-sideB, pos.x);
                b.obj.transform.position = pos;   

                b.v.x = -e * b.v.x;               
            }
            if (!(pos.y < sideB && pos.y > -sideB))
            {
                pos.y = Mathf.Min(sideB, pos.y);
                pos.y = Mathf.Max(-sideB, pos.y);
                b.obj.transform.position = pos;  

                b.v.y = -e * b.v.y;               
            }
            if (!(pos.z < sideB && pos.z > -sideB))
            {
                pos.z = Mathf.Min(sideB, pos.z);
                pos.z = Mathf.Max(-sideB, pos.z);
                b.obj.transform.position = pos;   

                b.v.z = -e * b.v.z;               
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

    void makeCube()
    {
        walls.Add(makeWall(new Vector3(side, 0, side - 4), new Vector3(thk, s2, 6)));
        walls.Add(makeWall(new Vector3(-side, 0, side - 4), new Vector3(thk, s2, 6)));
        walls.Add(makeWall(new Vector3(0, -side, side - 4), new Vector3(s3, thk, 6)));
        walls.Add(makeWall(new Vector3(0, side, side - 4), new Vector3(s3, thk, 6)));
        walls.Add(makeWall(new Vector3(0, 0, side - 4), new Vector3(s2, s2, thk)));
    }

    GameObject makeWall(Vector3 pos, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        return wall;
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
            foreach (Ball b in balls)
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
