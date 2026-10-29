using UnityEngine;

public class MoonRotation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public float revolutionSpeed = 15f;

    void Update()
    {
        transform.Rotate(Vector3.up * revolutionSpeed * Time.deltaTime);
    }
}
