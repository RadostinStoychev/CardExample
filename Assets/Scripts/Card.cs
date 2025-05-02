using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [Header("Card Components")]
    [SerializeField] private Button cardButton;
    [SerializeField] private Image cardImage;
    [SerializeField] private Image cardBack;
    [SerializeField] private Transform cardFace;
    
    [Header("Animation Settings")]
    [SerializeField] private float flipDuration = 0.3f;
    [SerializeField] private AnimationCurve flipCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float matchScaleDuration = 0.2f;
    [SerializeField] private float matchFadeDuration = 0.5f;
    
    private int cardValue;
    private Sprite frontSprite;
    private Sprite backSprite;
    private bool isFlipped = false;
    private bool isMatched = false;
    private Coroutine flipCoroutine;
    private Coroutine matchCoroutine;
    
    // Cached components for optimization
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    
    public int CardValue => cardValue;
    public bool IsFlipped => isFlipped;
    public bool IsMatched => isMatched;
    
    private void Awake()
    {
        if (cardButton == null)
        {
            cardButton = GetComponent<Button>();
        }

        rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        cardButton.onClick.AddListener(OnCardClicked);
    }
    
    public void Setup(int value, Sprite front, Sprite back)
    {
        StopAllCoroutines();
        
        cardValue = value;
        frontSprite = front;
        backSprite = back;
        
        // Apply sprites
        cardImage.sprite = frontSprite;
        cardBack.sprite = backSprite;
        
        // Reset state
        isFlipped = false;
        isMatched = false;
        
        // Reset visuals
        cardFace.localRotation = Quaternion.Euler(0f, 180f, 0f);
        cardButton.interactable = true;
        cardImage.color = Color.white;
        cardBack.gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        rectTransform.localScale = Vector3.one;
    }
    
    public void Flip()
    {
        isFlipped = !isFlipped;
        
        // Cancel any existing flip animation
        if (flipCoroutine != null)
        {
            StopCoroutine(flipCoroutine);
        }
        
        // Start new flip animation
        flipCoroutine = StartCoroutine(FlipAnimation());
    }
    
    private IEnumerator FlipAnimation()
    {
        float elapsedTime = 0f;
        Quaternion startRotation = cardFace.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0f, isFlipped ? 0f : 180f, 0f);
        
        while (elapsedTime < flipDuration)
        {
            float t = flipCurve.Evaluate(elapsedTime / flipDuration);
            cardFace.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        cardBack.gameObject.SetActive(!isFlipped);
        cardImage.gameObject.SetActive(isFlipped);
        
        // Ensure the final rotation is exact
        cardFace.localRotation = targetRotation;
        flipCoroutine = null;
    }

    // Initiate matching routine with an animation
    public void SetMatched()
    {
        isMatched = true;
        isFlipped = true;
        
        // Disable button
        cardButton.interactable = false;
        
        // Cancel any existing match animation
        if (matchCoroutine != null)
        {
            StopCoroutine(matchCoroutine);
        }
        
        // Start match animation
        matchCoroutine = StartCoroutine(MatchAnimation());
    }

    // Initiate matching routine with no animation
    public void SetMatchedOnStart()
    {
        isMatched = true;
        isFlipped = true;
        
        // Disable button
        cardButton.interactable = false;
        
        cardBack.gameObject.SetActive(false);
        cardImage.gameObject.SetActive(true);
        cardImage.color = new Color(cardImage.color.r, cardImage.color.g, cardImage.color.b, 0.5f);;
        cardFace.localRotation = Quaternion.Euler(0f, isFlipped ? 0f : 180f, 0f);
    }
    
    private IEnumerator MatchAnimation()
    {
        // Scale up animation
        float elapsedTime = 0f;
        Vector3 startScale = rectTransform.localScale;
        Vector3 targetScale = new Vector3(1.1f, 1.1f, 1.1f);
        
        while (elapsedTime < matchScaleDuration)
        {
            float t = elapsedTime / matchScaleDuration;
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Scale down animation
        elapsedTime = 0f;
        startScale = rectTransform.localScale;
        targetScale = Vector3.one;
        
        while (elapsedTime < matchScaleDuration)
        {
            float t = elapsedTime / matchScaleDuration;
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Fade animation
        elapsedTime = 0f;
        Color startColor = cardImage.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0.5f);
        
        while (elapsedTime < matchFadeDuration)
        {
            float t = elapsedTime / matchFadeDuration;
            cardImage.color = Color.Lerp(startColor, targetColor, t);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure the final values are exact
        rectTransform.localScale = Vector3.one;
        cardImage.color = targetColor;
        matchCoroutine = null;
    }
    
    private void OnCardClicked()
    {
        if (!isMatched && !isFlipped)
        {
            GameManager.Instance.SelectCard(this);
        }
    }
    
    private void OnDestroy()
    {
        // Stop all coroutines to prevent leaks
        StopAllCoroutines();
        
        // Remove button listener
        cardButton.onClick.RemoveListener(OnCardClicked);
    }
    
    public void Reset()
    {
        StopAllCoroutines();
        flipCoroutine = null;
        matchCoroutine = null;
        cardButton.interactable = true;
        cardImage.color = Color.white;
        canvasGroup.alpha = 1f;
        rectTransform.localScale = Vector3.one;
        cardFace.localRotation = Quaternion.Euler(0f, 180f, 0f);
        isFlipped = false;
        isMatched = false;
    }
}