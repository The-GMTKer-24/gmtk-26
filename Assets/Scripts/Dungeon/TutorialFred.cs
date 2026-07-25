using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialFred : MonoBehaviour
{
    [SerializeField] private TMP_Text textField;
    [SerializeField] private GameObject interactPrompt;
    
    [SerializeField] private InputAction interactAction;

    private bool isTalking;

    private void OnEnable() => interactAction.Enable();
    private void OnDisable() => interactAction.Disable();
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isTalking)
        {
            interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (interactAction.IsPressed())
        {
            interactPrompt.SetActive(false);
            textField.gameObject.SetActive(true);
            
            // Starting point for the rest of the stuff where you show the text and iterate through it
        }
    }
}
