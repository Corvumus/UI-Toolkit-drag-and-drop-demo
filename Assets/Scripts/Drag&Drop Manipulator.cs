using UnityEngine;
using UnityEngine.UIElements;

public class DragAndDropManipulator : PointerManipulator
{
    private bool isDragging;

    //Позиция указателя при начале перетаскивания
    private Vector2 pointerStartPos;
    //Позиция элемента при начале перетаскивания
    private Vector2 elemStartPos;

    public DragAndDropManipulator(VisualElement target)
    { 
        this.target = target;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        //Реагируем только на ЛКМ
        if (evt.button != 0)
            return;

        target.style.position = Position.Absolute;

        isDragging = true;

        pointerStartPos = evt.position;
        elemStartPos = target.worldBound.position;

        //Переносим элемент на передний план
        target.BringToFront();

        //Захватываем указатель, чтобы события move и up работали при выходе за границы элемента
        target.CapturePointer(evt.pointerId);

        //Не допускаем вызов метода на родительском элементе
        evt.StopPropagation();
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        //Если не перетаскивается или захвачен другим курсором, то выходим
        if (!isDragging || !target.HasPointerCapture(evt.pointerId))
            return;

        VisualElement parent = target.parent;
        if (parent == null)
            return;

        //Текущая позиция курсора
        Vector2 pointerPos = evt.position;
        //Смещение курсора относительно начальной позиции
        Vector2 pointerDelta = pointerPos - pointerStartPos;

        Vector2 newWorld = elemStartPos + pointerDelta;
        Vector2 newLocal = parent.WorldToLocal(newWorld);

        //Ограничиваем позицию элемента в пределах экрана
        Vector2 clampedLocal = ClampPosition(newLocal);

        target.style.left = clampedLocal.x;
        target.style.top = clampedLocal.y;

        //Не допускаем вызов метода на родительском элементе
        evt.StopPropagation();
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        //Если не перетаскивается или захвачен другим курсором, то выходим
        if (!isDragging || !target.HasPointerCapture(evt.pointerId) || evt.button != 0)
            return;

        //Освобождаем указатель
        target.ReleasePointer(evt.pointerId);
        
        //Не допускаем вызов метода на родительском элементе
        evt.StopPropagation();
    }

    private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
    {
        isDragging = false;
    }

    //Ограничиваем положение элемента в пределах экрана
    private Vector2 ClampPosition(Vector2 position)
    {
        // Получаем размеры родителя и элемента
        float parentWidth = target.parent.layout.width;
        float parentHeight = target.parent.layout.height;

        float elementWidth = target.layout.width;
        float elementHeight = target.layout.height;

        // Если родитель или элемент еще не имеют размера, возвращаем позицию без изменений
        if (parentWidth <= 0 || parentHeight <= 0 || elementWidth <= 0 || elementHeight <= 0)
            return position;

        float minX, maxX, minY, maxY;

        // Объект может выходить за границы не более чем на половину
        minX = -elementWidth / 2;
        maxX = parentWidth - elementWidth / 2;
        minY = -elementHeight / 2;
        maxY = parentHeight - elementHeight / 2;

        // Применяем ограничения
        float clampedX = Mathf.Clamp(position.x, minX, maxX);
        float clampedY = Mathf.Clamp(position.y, minY, maxY);

        return new Vector2(clampedX, clampedY);
    }
}
