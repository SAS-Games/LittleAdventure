using UnityEngine;
using Unity.Mathematics;

public class FlowFieldAgent : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        if (FlowFieldTestBootstrap.Sampler == null)
            return;

        float2 dir = FlowFieldTestBootstrap.Sampler.SampleDirection(transform.position);

        Vector3 move = new Vector3(dir.x, 0f, dir.y) * speed * Time.deltaTime;

        transform.position += move;
    }
}