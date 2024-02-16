using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class RectTransformParentFitter : MonoBehaviour
{

    private RectTransform parentRectTransform;
    private RectTransform rectTransform;

    private void Update()
    {
        FitRectTransformToParent();
    }

    private void FitRectTransformToParent()
    {
        if (transform.parent == null)
        {
            Debug.LogError("The UI object has no parent.");
            return;
        }

        parentRectTransform = transform.parent.GetComponent<RectTransform>();
        rectTransform = GetComponent<RectTransform>();

        if (parentRectTransform == null || rectTransform == null)
        {
            Debug.LogError("GameObject or parent does not have a RectTransform component.");
            return;
        }

        float scale = rectTransform.localScale.x;
        float scaleParent = parentRectTransform.localScale.x;
        float scaleRatio = scale / scaleParent;
        
        if(scaleRatio != 0) rectTransform.sizeDelta = new Vector2(parentRectTransform.rect.width / scaleRatio, parentRectTransform.rect.height / scaleRatio);
        rectTransform.localPosition = Vector2.zero;
    }

}
