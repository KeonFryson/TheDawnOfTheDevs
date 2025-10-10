using UnityEngine;
using UnityEngine.EventSystems;

public class CreditsStoper : MonoBehaviour, IPointerClickHandler
{

    [SerializeField] private GameObject OptionsMenu;
    [SerializeField] private GameObject CreditsMenu;
    [SerializeField] private GameObject MainMenu;
    [SerializeField] Animator animator;



    void Update()
    {
        
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
           
            
            if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
            {

                if (OptionsMenu != null)
                {
                    OptionsMenu.SetActive(true);
                }

                if (CreditsMenu != null)
                {
                    CreditsMenu.SetActive(false);
                }

            }
        }

        if (animator != null)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f && !animator.IsInTransition(0))
            {
                // Animation has finished
                if (MainMenu != null)
                {
                    MainMenu.SetActive(true);
                }
                if (CreditsMenu != null)
                {
                    CreditsMenu.SetActive(false);
                }
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {

        if (OptionsMenu != null)
        {
            OptionsMenu.SetActive(true);
        }

        if (CreditsMenu != null)
        {
            CreditsMenu.SetActive(false);
        }
    }

    
}
