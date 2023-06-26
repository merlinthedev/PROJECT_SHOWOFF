using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
 
public class InteractableAutoSelect : MonoBehaviour {
    [SerializeField]
    private InputActionReference navigate;

    [SerializeField]
    private List<Selectable> selectables;

    private void OnEnable() {
        navigate.action.performed += OnPerformed;
    }

    private void OnDisable() {
        navigate.action.performed -= OnPerformed;
    }

    public void OnPerformed(InputAction.CallbackContext ctx = default) {
        if (EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.activeInHierarchy || !EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().IsInteractable()) {
            foreach (Selectable selectable in selectables) {
                if (selectable.IsInteractable() && selectable.isActiveAndEnabled) {
                    EventSystem.current.SetSelectedGameObject(selectable.gameObject);
                }
            }
        }
    }
}