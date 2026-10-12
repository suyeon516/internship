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

    // 물리 상수
    const float g = 9.8f;
    const float ks = 100f;                  // 스프링 계수
    const float kd = 3f;                    // 댐퍼 계수 (1 -> 3으로 올림)
    const float e = 0.7f;                   // 탄성 계수 (0.9 -> 0.7로 낮춤)
    float dt = 0.02f;

    // 바람
    const float windMax = 20f;              // 최대 바람 세기
    const float windMin = 8f;               // 출렁일 때 최소 세기 (중력 9.8보다 약함)
    const float rampTime = 4f;              // 최대 세기까지 걸리는 시간(초)
    bool blowing = false;
    float windTimer = 0f;

    // 공
    List<LottoBall> balls = new List<LottoBall>();    // 통 안에서 움직이는 공
    List<LottoBall> all = new List<LottoBall>();      // 전체 공 (글자 위치 갱신용)
    List<int> pickedNumbers = new List<int>();        // 뽑힌 번호

    // 추첨
    const int pickCount = 6;
    const float pickInterval = 4f;          // 몇 초마다 한 개 뽑을지
    float pickTimer = 0f;

    bool zeroGravity = false;

    void OnGUI()
    {
        zeroGravity = GUI.Toggle(new Rect(10, 10, 150, 25), zeroGravity, "zero-gravity");

        if (GUI.Button(new Rect(10, 40, 100, 30), "추첨 시작"))
        {
            if (!blowing && pickedNumbers.Count < pickCount)
            {
                blowing = true;
                pickTimer = 0f;
                windTimer = 0f;             // 매번 약한 바람부터 시작
            }
        }

        // 뽑힌 번호 표시 (작은 순으로 정렬해서)
        List<int> sorted = new List<int>(pickedNumbers);
        sorted.Sort();
        GUI.Label(new Rect(10, 80, 400, 25), "뽑힌 번호: " + string.Join(", ", sorted));
    }

    void Start()
    {
        Camera.main.transform.position = new Vector3(0, 0, 35);
        Camera.main.transform.LookAt(Vector3.zero);

        makeBucket();

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

            // 통 안 아무 곳에서 시작 (z 고정 없음)
            b.obj.transform.position = Random.insideUnitSphere * (radius - 1.5f);

            // 속도: 모든 방향 랜덤 (5 -> 2로 낮춤)
            b.v = 2f * RandomVec();

            // 번호 글자 (LateUpdate에서 위치를 맞춤)
            b.label = new GameObject("Label" + b.number);
            TextMesh tm = b.label.AddComponent<TextMesh>();
            tm.text = b.number.ToString();
            tm.fontSize = 100;
            tm.characterSize = 0.1f;        // 글자 크기: 안 맞으면 이 값 조절
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.black;

            balls.Add(b);
            all.Add(b);
        }
    }

    void Update()
    {
        // 0. 바람 세기 계산: 서서히 세지다가(ramp) 출렁임(wave)
        float wind = 0f;
        if (blowing)
        {
            windTimer += dt;
            float ramp = Mathf.Clamp01(windTimer / rampTime);               // 0 -> 1
            float wave = (Mathf.Sin(windTimer * 2f) + 1f) * 0.5f;           // 0 ~ 1 출렁
            wind = ramp * Mathf.Lerp(windMin, windMax, wave);
        }

        // 1. 힘 계산
        for (int i = 0; i < balls.Count; i++)
        {
            LottoBall bi = balls[i];

            // 중력
            bi.f = zeroGravity ? Vector3.zero : bi.m * new Vector3(0, -g, 0);

            // 바람: 통 아래쪽에서만 위로 불어서 공을 솟구치게 함
            if (blowing && bi.obj.transform.position.y < -radius * 0.3f)
            {
                bi.f += new Vector3(Random.Range(-5f, 5f), wind, Random.Range(-5f, 5f));
            }

            // 공끼리 충돌 (스프링 + 댐퍼)
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

        // 2. 속도, 위치 갱신
        for (int i = 0; i < balls.Count; i++)
        {
            balls[i].v += balls[i].f / balls[i].m * dt;
            balls[i].obj.transform.position += balls[i].v * dt;
        }

        // 3. 통 벽 충돌 (이동 다음에 검사)
        for (int i = 0; i < balls.Count; i++)
        {
            LottoBall b = balls[i];
            float limit = radius - b.radius;
            Vector3 pos = b.obj.transform.position;

            if (pos.magnitude > limit)
            {
                Vector3 n = pos.normalized;              // 벽의 법선 (원점 -> 공)
                b.obj.transform.position = n * limit;    // 한계선 안으로 되돌림

                float vn = Vector3.Dot(b.v, n);
                if (vn > 0)                              // 벽 쪽으로 나가는 중일 때만 반사
                    b.v -= (1 + e) * vn * n;
            }
        }

        // 4. 추첨: 바람이 켜진 동안 일정 간격마다 가장 위에 있는 공 하나 뽑기
        if (blowing)
        {
            pickTimer += dt;
            if (pickTimer >= pickInterval)
            {
                pickTimer = 0f;
                pickTopBall();

                if (pickedNumbers.Count >= pickCount)
                    blowing = false;                     // 6개 다 뽑으면 바람 끔
            }
        }
    }

    // 번호 글자를 항상 카메라 쪽을 보게, 공 표면 앞에 붙임
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

    void pickTopBall()
    {
        if (balls.Count == 0) return;

        // y가 가장 큰 공 찾기
        LottoBall top = balls[0];
        foreach (LottoBall b in balls)
        {
            if (b.obj.transform.position.y > top.obj.transform.position.y)
                top = b;
        }

        // 물리 계산 대상에서 제외하고 통 아래쪽 일렬 위치에 배치
        balls.Remove(top);
        top.v = Vector3.zero;

        int idx = pickedNumbers.Count;
        top.obj.transform.position = new Vector3(-12.5f + idx * 5f, -14f, 0);
        pickedNumbers.Add(top.number);
    }

    void makeBucket()
    {
        bucket = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bucket.transform.localScale = Vector3.one * radius * 2f;
        bucket.transform.position = Vector3.zero;

        if (glassMat != null)
            bucket.GetComponent<Renderer>().material = glassMat;

        Destroy(bucket.GetComponent<Collider>());   // 통 콜라이더 제거 (충돌은 코드로 처리)
    }

    Vector3 RandomVec()
    {
        return new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f));
    }
}