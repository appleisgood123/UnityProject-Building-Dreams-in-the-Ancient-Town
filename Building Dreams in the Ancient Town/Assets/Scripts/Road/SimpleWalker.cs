using UnityEngine;

public class SimpleWalker : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public float moveSpeed = 1f;
    public float arriveDistance = 0.5f;

    private bool movingToEnd = true;

    private void Start()
    {
        if (startPoint != null)
            transform.position = startPoint.position;
        if (endPoint != null)
            transform.forward = (endPoint.position - startPoint.position).normalized;
    }

    private void Update()
    {
        if (startPoint == null || endPoint == null) return;

        Transform target = movingToEnd ? endPoint : startPoint;
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        Vector3 dir = target.position - transform.position;
        if (dir != Vector3.zero)
            transform.forward = dir.normalized;

        if (Vector3.Distance(transform.position, target.position) <= arriveDistance)
        {
            if (movingToEnd)
            {
                transform.position = startPoint.position;
                transform.forward = (endPoint.position - startPoint.position).normalized;
                // �������յ��ƶ�
            }
            // ���ϣ�������ߣ����ڴ˴��л� movingToEnd�������ﰴ����ѭ��ʵ��
        }
    }
}