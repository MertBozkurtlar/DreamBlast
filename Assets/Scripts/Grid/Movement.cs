using System.Collections;
using UnityEngine;

/*
 * This script will allow any game object to be moved smoothly
 */
public class Movement : MonoBehaviour
{
    private Vector3 from;
    private Vector3 to;
    private float currentDistance;

    protected bool idle = true;

    public bool Idle
    {
        get
        {
            return idle;
        }
    }

    [SerializeField]
    private float   speed = 1;

    public float Speed
    {
        get
        {
            return speed;
        }
    }
    public IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        if(speed <= 0)
            Debug.LogWarning("Speed must be a positive number.");

        from = transform.position;
        to = targetPosition;
        currentDistance = 0;
        idle = false;

        do
        {
            currentDistance += speed * Time.deltaTime;

            if(currentDistance > 1)
                currentDistance = 1;

            transform.position = Vector3.LerpUnclamped(from, to, Easing(currentDistance));

            yield return null;
        }
        while(currentDistance != 1);

        idle = true;
    }
    public IEnumerator MoveToTransform(Transform target)
    {
        if(speed <= 0)
            Debug.LogWarning("Speed must be a positive number.");

        from = transform.position;
        to = target.position;
        currentDistance = 0;
        idle = false;

        do
        {
            currentDistance += speed * Time.deltaTime;

            if(currentDistance > 1)
                currentDistance = 1;

            to = target.position;
            transform.position = Vector3.LerpUnclamped(from, to, Easing(currentDistance));

            yield return null;
        }
        while(currentDistance != 1);

        idle = true;
    }

    //  Easing function to alter the speed of the animation over time
    //  Can replace with dotween
    private float Easing(float t)
    {
        float c1 = 1.70158f;
        float c2 = c1 * 1.525f;

        return t < 0.5f
            ? (Mathf.Pow(t * 2, 2) * ((c2 + 1) * 2 * t - c2)) / 2
            : (Mathf.Pow(t * 2 - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2
            ;
    }
}