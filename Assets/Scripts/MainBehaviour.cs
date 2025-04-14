using UnityEngine;

public class MainBehaviour : MonoBehaviour
{
    [SerializeField] private GameManager gameManagerPrefab;
    [SerializeField] private UIManager uiManagerPrefab;
    
    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            Instantiate(gameManagerPrefab);
        }
        
        if (UIManager.Instance == null)
        {
            Instantiate(uiManagerPrefab);
        }
    }
}
