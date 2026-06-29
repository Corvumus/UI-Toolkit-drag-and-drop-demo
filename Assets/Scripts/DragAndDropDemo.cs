using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class DragAndDropDemo : MonoBehaviour
{
    PanelRenderer panelRenderer;
    private int lastVersion;

    private void Awake()
    {
        panelRenderer = GetComponent<PanelRenderer>();
    }

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReloadedCallback);
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReloadedCallback);
    }

    private void OnUIReloadedCallback(PanelRenderer panelRenderer, VisualElement rootElement, int version)
    {
        if (lastVersion == version)
            return;

        lastVersion = version;

        //Находим элементы с тэгом "draggable"
        List<VisualElement> draggables = rootElement.Query<VisualElement>(className: "draggable").ToList();

        //Добавляем манипулятор к элементам
        for (int i = 0; i < draggables.Count; i++)
            draggables[i].AddManipulator(new DragAndDropManipulator(draggables[i]));
    }
}
