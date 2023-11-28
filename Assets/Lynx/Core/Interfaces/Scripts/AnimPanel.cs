using System.Collections;
using Lynx;
using UnityEngine;

public class AnimPanel : MonoBehaviour
{
    [SerializeField] private Vector2 targetSize;
    [SerializeField] private RectTransform panelBG;
    [SerializeField] private float timeOfSlide = 1.0f;
    [SerializeField] private LynxMath.easingType easing;
    [SerializeField] private GameObject colorButtons;

    private Vector2 baseSize;

    
    // Start is called before the first frame update
    void Start()
    {
        baseSize = panelBG.sizeDelta;
    }

    public void OpenPanel()
    {
        colorButtons.SetActive(true);
        StartCoroutine(ResizePanel(targetSize,true));
    }
    
    public void ClosePanel()
    {
        StartCoroutine(ResizePanel(baseSize, false));
    }

    private IEnumerator ResizePanel(Vector2 endSize, bool isButtonPanelActive)
    {
        Vector2 startSize = panelBG.sizeDelta;

        for(float t = 0; t < 1; t += Time.deltaTime/timeOfSlide)
        {
            panelBG.sizeDelta = Vector2.Lerp(startSize, endSize, LynxMath.Ease(t, easing));
            yield return new WaitForEndOfFrame();
        }
        colorButtons.SetActive(isButtonPanelActive);
    }

}
