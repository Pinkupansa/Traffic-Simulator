using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleToDestroy : MonoBehaviour
{
    [SerializeField] private Vector2 targetScale;
    [SerializeField] private Vector2 baseScale;
    [SerializeField] private float scaleTime;
    private void Start()
    {
        transform.localScale = baseScale;
        LeanTween.scale(gameObject, targetScale, scaleTime).setEase(LeanTweenType.easeInBounce).setOnComplete(() => Destroy(gameObject));
    }
}
