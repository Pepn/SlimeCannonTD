using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public class UIToolkitButton : MonoBehaviour
{
    [SerializeField, Required] private UIDocument UIDocument;
    [SerializeField, Required] private string buttonName;
    [SerializeField] UnityEvent OnButtonClick;
    Button Button;

    private void OnAwake()
    {
        Button = UIDocument.rootVisualElement.Q(buttonName) as Button;

        if (Button == null)
        {
            Debug.LogWarning($"Button {name} not found.");
            return;
        }

        Button.RegisterCallback<ClickEvent>((ClickEvent evt) => OnButtonClick?.Invoke());
    }
}
