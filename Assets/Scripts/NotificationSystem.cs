using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class NotificationSystem : MonoBehaviour
{
    public static NotificationSystem Instance { get; private set; }
    
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private Transform notificationContainer;
    [SerializeField] private float displayTime = 3f;
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private Vector2 startPosition;
    [SerializeField] private Vector2 endPosition;
    [SerializeField] private float notificationSpacing = 60f;
    [SerializeField] private int maxNotifications = 5;
    
    private List<GameObject> activeNotifications = new List<GameObject>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void ShowNotification(string message, Color color)
    {
        activeNotifications.RemoveAll(item => item == null);
        
        if (activeNotifications.Count >= maxNotifications)
        {
            if (activeNotifications.Count > 0)
            {
                Destroy(activeNotifications[0]);
                activeNotifications.RemoveAt(0);
            }
        }
        
        GameObject notification = Instantiate(notificationPrefab, notificationContainer);
        activeNotifications.Add(notification);
        
        RectTransform rectTransform = notification.GetComponent<RectTransform>();
        TextMeshProUGUI textComponent = notification.GetComponentInChildren<TextMeshProUGUI>();
        
        textComponent.text = message;
        textComponent.color = color;
        
        Vector2 position = startPosition;
        position.y += (activeNotifications.Count - 1) * notificationSpacing;
        
        Sequence sequence = DOTween.Sequence();
        
        rectTransform.anchoredPosition = position;
        CanvasGroup canvasGroup = notification.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        
        Vector2 finalPosition = endPosition;
        finalPosition.y += (activeNotifications.Count - 1) * notificationSpacing;
        sequence.Append(rectTransform.DOAnchorPos(finalPosition, fadeTime).SetEase(Ease.OutBack));
        sequence.Join(canvasGroup.DOFade(1, fadeTime));
        
        sequence.AppendInterval(displayTime);
        
        sequence.Append(canvasGroup.DOFade(0, fadeTime));
        
        sequence.OnComplete(() => {
            int index = activeNotifications.IndexOf(notification);
            activeNotifications.Remove(notification);
            Destroy(notification);
            
            AdjustNotificationsPositions(index);
        });
        
        sequence.Play();
    }
    
    public void ShowNotification(string message)
    {
        ShowNotification(message, Color.white);
    }
    
    private void AdjustNotificationsPositions(int removedIndex)
    {
        for (int i = removedIndex; i < activeNotifications.Count; i++)
        {
            if (activeNotifications[i] != null)
            {
                RectTransform rt = activeNotifications[i].GetComponent<RectTransform>();
                Vector2 newPos = rt.anchoredPosition;
                newPos.y -= notificationSpacing;
                rt.DOAnchorPos(newPos, 0.3f).SetEase(Ease.OutBack);
            }
        }
    }
} 