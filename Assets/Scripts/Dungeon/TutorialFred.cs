using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialFred : MonoBehaviour
{
    [SerializeField] private TMP_Text textField;
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private GameObject textBox;
    
    [SerializeField] private InputAction interactAction;
    
    [SerializeField] private List<string> privateDialoguesList;

    [SerializeField] private string exhaustedDialogueText;

    private int lineCount;
    private int currentLine;
    private bool isTalking;
    private bool held;

    private void Start()
    {
        lineCount = privateDialoguesList.Count;
        textField.text = privateDialoguesList[0];
    }

    private void OnEnable() => interactAction.Enable();
    private void OnDisable() => interactAction.Disable();
    private void OnTriggerEnter2D(Collider2D other)
    {
        interactPrompt.SetActive(true);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (interactAction.IsPressed())
        {
            if (isTalking && !held)
            {
                held = true;
                if (currentLine == lineCount)
                {
                    textField.text = exhaustedDialogueText;
                }
                else
                {
                    textField.text = privateDialoguesList[currentLine];
                    currentLine++;
                }
            }
            else
            {
                interactPrompt.SetActive(false);
                textBox.SetActive(true);
                isTalking = true;
            }
        }
        else
        {
            held = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        HideAllUI();
        isTalking = false;
    }

    void HideAllUI()
    {
        interactPrompt.SetActive(false);
        textBox.SetActive(false);
    }
}
