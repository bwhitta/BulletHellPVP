using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ControlsManager;

public class RebindButtonLogic : MonoBehaviour
{
        [Header("Rebind options")]
    private GameControls controls;
    [SerializeField] private bool excludeMouse;
    [SerializeField] private string actionName;
    [SerializeField] private int bindingIndex;
    
        [Header("Read only (auto select on validate): ")]
    [SerializeField] private Button selfButton;
    [SerializeField] private TMP_Text selfText;

    private void OnValidate() => SelectObjects();
    
    private void Awake()
    {
        SelectObjects();
        selfButton.onClick.AddListener(ButtonClicked);
        controls = GameControlsMaster.GameControls;
        selfText.SetText(controls.FindAction(actionName, true).bindings[bindingIndex].ToDisplayString());
    }

    private void SelectObjects()
    {
        selfButton = this.GetComponent<Button>();
        selfText = this.GetComponentInChildren<TMP_Text>();
    }

    private void ButtonClicked()
    {
        if (RebindsMaster.currentlyRebinding == true)
        {
            Debug.LogWarning("Binding denied - binding already in progress.");
            return;
        }
        InputAction foundAction = controls.FindAction(actionName);
        Debug.Log(actionName + " clicked!");
        InteractiveRebind(foundAction, bindingIndex);
    }

    private void SetText(string textToSet) => selfText.text = textToSet;

    private InputActionRebindingExtensions.RebindingOperation rebindOperation;
    private void InteractiveRebind(InputAction actionWithRebind, int bindingTargetIndex)
    {
        if (actionWithRebind == null)
        {
            Debug.LogError("Rebind target is null.");
            return;
        } else if (actionWithRebind.bindings.Count <= bindingTargetIndex)
        {
            Debug.LogError("Invalid binding index.");
            return;
        }

        if (actionWithRebind.bindings[bindingTargetIndex].isComposite)
        {
            Debug.Log("isComposite");
            var firstPartIndex = bindingTargetIndex + 1;
            if (firstPartIndex < actionWithRebind.bindings.Count && actionWithRebind.bindings[firstPartIndex].isComposite)
            {
                Debug.Log("composite rebind started");
                StartRebind(actionWithRebind, bindingTargetIndex, true);
            }
        }
        else
        {
            StartRebind(actionWithRebind, bindingTargetIndex, false);
        }
    }
    private void StartRebind(InputAction actionWithRebind, int bindingTargetIndex, bool allCompositeParts)
    {
        RebindsMaster.currentlyRebinding = true;
        actionWithRebind.Disable();
        InputBinding targetedBinding = actionWithRebind.bindings[bindingTargetIndex];

        Debug.Log("IsComposite: " + allCompositeParts);

        SetText("Input for: " + targetedBinding.ToDisplayString());

        rebindOperation = actionWithRebind.PerformInteractiveRebinding(bindingTargetIndex)
            .WithControlsExcluding("Mouse")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.2f)
            .OnCancel(op =>
            {
                SetText(actionWithRebind.GetBindingDisplayString(bindingTargetIndex));
                actionWithRebind.Enable();
                rebindOperation.Dispose();
                RebindsMaster.currentlyRebinding = false;
            })
            .OnComplete(op =>
            {
                SetText(actionWithRebind.GetBindingDisplayString(bindingTargetIndex));
                actionWithRebind.Enable();
                rebindOperation.Dispose();
                if (allCompositeParts)
                {
                    int nextBindingIndex = bindingTargetIndex + 1;
                    if (nextBindingIndex < actionWithRebind.bindings.Count && actionWithRebind.bindings[nextBindingIndex].isComposite)
                    {
                        StartRebind(actionWithRebind, bindingTargetIndex, allCompositeParts);
                    }
                }
                RebindsMaster.currentlyRebinding = false;

            })
            .Start();
    }
}
