using UnityEngine;
using UnityEngine.UI;

public class ChartLine : MonoBehaviour
{
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public ChartLine Initialize(Vector2 posA, Vector2 posB)
    {
        Vector2 dir = (posB - posA).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float distance = Vector2.Distance(posA, posB);
        _rectTransform.sizeDelta = new Vector2(distance, 3.0f);
        _rectTransform.anchoredPosition = posA + dir * distance * 0.5f;
        _rectTransform.localEulerAngles = new Vector3(0, 0, angle);

        return this;
    }
}
