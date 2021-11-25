
using UnityEngine;
using UnityEngine.EventSystems;
public class UnfoldingUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Vector2 unfoldedScale;
    [SerializeField] private Vector2 foldedScale;
    [SerializeField] private GameObject[] subcomponents;
    [SerializeField] private float unfoldTime;
    [SerializeField] private float foldTime;
    [SerializeField] private bool useRectTransform;
    private void Start()
    {
        if(useRectTransform)
        {
            SetWidth(foldedScale.x);
            SetHeight(foldedScale.y);
        }
        else
        {
            transform.localScale = foldedScale;
        }
        HideUnhideSubcomponents(false);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        LeanTween.value(foldedScale.x, unfoldedScale.x, unfoldTime).setOnUpdate(SetWidth).setEase(LeanTweenType.easeInSine);
        LeanTween.value(foldedScale.y, unfoldedScale.y, unfoldTime).setOnUpdate(SetHeight).setEase(LeanTweenType.easeInSine).setOnComplete(() => HideUnhideSubcomponents(true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        LeanTween.value(unfoldedScale.x, foldedScale.x, foldTime).setOnUpdate(SetWidth).setEase(LeanTweenType.easeInSine);
        LeanTween.value(unfoldedScale.y, foldedScale.y, foldTime).setOnUpdate(SetHeight).setEase(LeanTweenType.easeInSine).setOnComplete(() => HideUnhideSubcomponents(false)); 
    }

    private void HideUnhideSubcomponents(bool show)
    {
        foreach(GameObject g in subcomponents)
        {
            g.SetActive(show);
        }
    }
    private void SetWidth(float value)
    {
        if(useRectTransform)
        {
            RectTransform rect = GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(value, rect.sizeDelta.y);
        }
        else
        {
            transform.localScale = new Vector2(value, transform.localScale.y);
        }
        
    }
    private void SetHeight(float value)
    {
        if (useRectTransform)
        {
            RectTransform rect = GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, value);
        }
        else
        {
            transform.localScale = new Vector2(transform.localScale.x, value);
        }
    }
    
    
}
