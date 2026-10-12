using UnityEngine;
using System.Collections.Generic;

public class LottoBall
{
    public GameObject obj;
    public GameObject label;   // 번호 글자
    public int number;
    public float radius;
    public Vector3 v;
    public Vector3 f;
    public float m;
}

public class Lotto : MonoBehaviour
{
    public Material glassMat;               // Inspector에서 투명 머티리얼 연결

    // 통
    GameObject bucket;
    const float radius = 10.0f;             // 통 반지름

    // 출구 
    const float exitR = 2.5f;               // 출구 구멍 반지름
    bool exitOpen = false;                  // 추첨 중에만 열림

    const float g = 9.8f;                     // 중력 
    const float airDrag = 0.5f;             // 공기 저항
    const float ks = 100f;                  // 스프링 계수(얼마나 세게 밀어내는ㄴ지)
    const float kd = 3f;                    // 댐퍼 계수 (브레이크를 얼마나 세게 거는지)
    const float e = 0.7f;                   // 탄성 계수 (벽에 튕길 때 남는 속도 비율)
    float dt = 0.02f;

    // 바람 
    const float windMax = 115f;
    const float windMin = 2.5f;
    const float rampTime = 4f;  // 바람이 최대까지 세지는 시간
    bool blowing = false;   // 지금 바람이 불고 있나
    float windTimer = 0f;

    // 공
    List<LottoBall> balls = new List<LottoBall>();    // 통 안에서 움직이는 공
    List<LottoBall> all = new List<LottoBall>();      // 전체 공 (글자 위치 갱신용)
    List<int> pickedNumbers = new List<int>();        // 뽑힌 번호

    const int pickCount = 6;
    bool zeroGravity = false;

    void OnGUI()
    {
        zeroGravity = GUI.Toggle(new Rect(10, 10, 150, 25), zeroGravity, "zero-gravity");

        if (GUI.Button(new Rect(10, 40, 100, 30), "추첨 시작"))
        {
            if (!blowing && pickedNumbers.Count < pickCount)    // 추첨 중이 아니고 아직 6개를 안뽑았을 때
            {
                blowing = true;
                windTimer = 0f;
            }
        }

        List<int> sorted = new List<int>(pickedNumbers);
        sorted.Sort();
        GUI.Label(new Rect(10, 80, 400, 25), "뽑힌 번호: " + string.Join(", ", sorted));
    }

    void Start()
    {
        Camera.main.transform.position = new Vector3(0, 0, 35);
        Camera.main.transform.LookAt(Vector3.zero);

        makeBucket();   // 통 만들기

        for (int i = 0; i < 45; i++)
        {
            LottoBall b = new LottoBall();
            b.number = i + 1;
            b.radius = 1.0f;
            b.m = b.radius * b.radius * b.radius;

            b.obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            b.obj.transform.localScale = Vector3.one * b.radius * 2f;
            b.obj.GetComponent<Renderer>().material.color =
                new Color(Random.value, Random.value, Random.value);

            b.obj.transform.position = Random.insideUnitSphere * (radius - 1.5f);   //반지름 8.5짜리 구 안의 랜덤 위치
            b.v = 2f * RandomVec();

            // 번호 글자 붙이기
            b.label = new GameObject("Label" + b.number);
            TextMesh tm = b.label.AddComponent<TextMesh>();
            tm.text = b.number.ToString();
            tm.fontSize = 100;
            tm.characterSize = 0.1f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.black;

            balls.Add(b);
            all.Add(b);
        }
    }

    void Update()
    {
        // 출구는 추첨 중이고 아직 6개 안 뽑았을 때 열림
        exitOpen = blowing && pickedNumbers.Count < pickCount;

        // 바람 세기: 서서히 세지다가(ramp) 출렁임(wave)
        float wind = 0f;
        if (blowing)
        {
            windTimer += dt;
            float ramp = Mathf.Clamp01(windTimer / rampTime);   //시간이 지날수록 0~1로 감 4초 뒤엔 1에서 멈춤
            float wave = (Mathf.Cos(windTimer * 2f) + 1f) * 0.5f;   //cos파 0~1 값
            wind = ramp * Mathf.Lerp(windMin, windMax, wave);   // 둘을 곱해서 서서히 세지고 출렁이는 바람ㅁ을 만듦
        }

        // 힘 계산
        for (int i = 0; i < balls.Count; i++)
        {
            LottoBall bi = balls[i];

            bi.f = zeroGravity ? Vector3.zero : bi.m * new Vector3(0, -g, 0);

            // 공기 저항 (중력이 켜져 있을 때만)
            if (!zeroGravity) bi.f -= airDrag * bi.v;

            // 바람: 통 아래쪽에 있을 때만 위로 wind만큼 밀기 x,z,에는 랜덤하게 살짝만 힘 줌
            if (blowing && bi.obj.transform.position.y < -radius * 0.3f)
            {
                bi.f += new Vector3(Random.Range(-2f, 2f), wind, Random.Range(-2f, 2f));
            }

            // 공끼리 충돌
            for (int j = 0; j < balls.Count; j++)
            {
                if (i == j) continue;
                LottoBall bj = balls[j];

                Vector3 rij = bi.obj.transform.position - bj.obj.transform.position;
                Vector3 vij = bi.v - bj.v;
                float sumRadius = bi.radius + bj.radius;

                if (sumRadius > rij.magnitude)
                {
                    Vector3 n = rij.normalized;
                    bi.f -= ks * (rij.magnitude - sumRadius) * n;
                    bi.f -= kd * Vector3.Dot(vij, n) * n;
                }
            }
        }

        // 속도, 위치 갱신
        for (int i = 0; i < balls.Count; i++)
        {
            balls[i].v += balls[i].f / balls[i].m * dt;
            balls[i].obj.transform.position += balls[i].v * dt;
        }

        //  통 벽 충돌 (출구 구멍 근처는 열려 있으면 통과)
        for (int i = 0; i < balls.Count; i++)
        {
            LottoBall b = balls[i];
            float limit = radius - b.radius;    // 공 중심이 갈 수 있는 한계
            Vector3 pos = b.obj.transform.position;

            if (pos.magnitude > limit)
            {
                float horizontal = new Vector2(pos.x, pos.z).magnitude;   // 위에서 봤을 때 중심으로부터 거리
                bool inExit = exitOpen && pos.y > 0 && horizontal < exitR;
                if (inExit) continue;                                      // 출구면 벽 충돌 건너뜀

                Vector3 n = pos.normalized;
                b.obj.transform.position = n * limit;

                float vn = Vector3.Dot(b.v, n);
                if (vn > 0) // 이미 벽에서 멀어지는 중이면 다시 반사하지 않는다는....
                    b.v -= (1 + e) * vn * n;
            }
        }

        //  추첨: 출구로 빠져나가 통 밖으로 완전히 나온 공
        if (exitOpen)
        {
            for (int i = balls.Count - 1; i >= 0; i--)
            {
                if (pickedNumbers.Count >= pickCount) break;

                LottoBall b = balls[i];
                Vector3 pos = b.obj.transform.position;
                if (pos.y > 0 && pos.magnitude > radius + b.radius) // 공 전체가 통 밖으로 나갔는지
                {
                    pickBall(b);
                }
            }

            if (pickedNumbers.Count >= pickCount)
                blowing = false;                                           // 6개 다 뽑으면 바람 끔
        }
    }

    // 번호 글자를 카메라 쪽 공 표면 앞에 붙임
    void LateUpdate()
    {
        Camera cam = Camera.main;
        foreach (LottoBall b in all)
        {
            Vector3 toCam = (cam.transform.position - b.obj.transform.position).normalized;
            b.label.transform.position = b.obj.transform.position + toCam * (b.radius + 0.05f);
            b.label.transform.rotation = cam.transform.rotation;
        }
    }

    void pickBall(LottoBall b)
    {
        balls.Remove(b);                    // 물리 계산 대상에서 제외
        b.v = Vector3.zero;

        int idx = pickedNumbers.Count;
        // 통 아래쪽에 세우기
        b.obj.transform.position = new Vector3(-12.5f + idx * 5f, -14f, 0);
        pickedNumbers.Add(b.number);
    }

    void makeBucket()
    {
        bucket = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bucket.transform.localScale = Vector3.one * radius * 2f;
        bucket.transform.position = Vector3.zero;

        if (glassMat != null)
            bucket.GetComponent<Renderer>().material = glassMat;

        Destroy(bucket.GetComponent<Collider>());

        // 출구 관 (눈으로 출구 위치를 보여주는 용도...)
        GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pipe.transform.position = new Vector3(0, radius + 1f, 0);
        pipe.transform.localScale = new Vector3(exitR * 2f, 1f, exitR * 2f);
        Destroy(pipe.GetComponent<Collider>());
    }

    Vector3 RandomVec()
    {
        return new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f));
    }
}