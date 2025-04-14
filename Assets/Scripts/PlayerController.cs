using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveDistance = 1f;
    [SerializeField] private float moveDuration = 0.3f;
    
    private bool isMoving = false;
    
    private void Update()
    {
        if (GameManager.Instance.IsBreedingMode)
            return;
            
        if (isMoving)
            return;
            
        if (Input.GetKeyDown(KeyCode.W))
        {
            MoveForward();
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            UIManager.Instance.ShowPlantingMenu();
            NotificationSystem.Instance.ShowNotification("Choose a crop to plant", new Color(0.4f, 0.6f, 1f));
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            if (GameManager.Instance.Inventory.Count >= 2)
            {
                if (GameManager.Instance.Money >= 50)
                {
                    UIManager.Instance.EnterBreedingMode();
                }
                else
                {
                    NotificationSystem.Instance.ShowNotification("Breeding costs 50 money!", Color.red);
                }
            }
            else
            {
                NotificationSystem.Instance.ShowNotification("Need at least 2 crops to breed!", Color.red);
            }
        }
    }
    
    private void MoveForward()
    {
        isMoving = true;
        
        transform.DOLocalMoveZ(transform.localPosition.z + moveDistance, moveDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                transform.DOLocalMoveZ(transform.localPosition.z - moveDistance, moveDuration * 0.5f)
                    .SetEase(Ease.InOutQuad)
                    .OnComplete(() => {
                        isMoving = false;
                        GameManager.Instance.NextDay();
                        NotificationSystem.Instance.ShowNotification("A new day has begun!", new Color(1f, 0.7f, 0.2f));
                    });
            });
    }
} 