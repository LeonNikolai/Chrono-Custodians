using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using static UnityEngine.UI.Button;
using UnityEngine.Events;

// Authors : Leon
// A scripts that remimplements the unity button class.
// More events and properties are added to the button class.
// Refferences to the image and text are added to the button class.
public class BetterButton : Selectable, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISubmitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Image _iconRefference;
    [SerializeField] TMP_Text _textRefference;
    [SerializeField] ButtonClickedEvent _OnClick = new ButtonClickedEvent();
    [SerializeField] UnityEvent _onHoverEnter = new UnityEvent();
    [SerializeField] UnityEvent _onHoverExit = new UnityEvent();
    [SerializeField] UnityEvent _onClickDown = new UnityEvent();
    [SerializeField] UnityEvent _onClickUp = new UnityEvent();
    // [SerializeField] AudioSource ClickSound;
    // [SerializeField] AudioSource HoverEnterSound;
    // [SerializeField] AudioSource HoverExitSound;

    public Sprite Image
    {
        get { return _iconRefference?.sprite; }
        set
        {
            if (_iconRefference) _iconRefference.sprite = value;
        }
    }
    public string Text
    {
        get { return _textRefference?.text; }
        set
        {
            if (_textRefference) _textRefference.text = value;
        }
    }
    public ButtonClickedEvent onClick
    {
        get { return _OnClick; }
        set { _OnClick = value; }
    }
    public UnityEvent onEnter
    {
        get { return _onHoverEnter; }
        set { _onHoverEnter = value; }
    }
    public UnityEvent onExit
    {
        get { return _onHoverExit; }
        set { _onHoverExit = value; }
    }
    public UnityEvent onClickDown
    {
        get { return _onClickDown; }
        set { _onClickDown = value; }
    }
    public UnityEvent onClickUp
    {
        get { return _onClickUp; }
        set { _onClickUp = value; }
    }

    private void Press()
    {
        if (!IsActive() || !IsInteractable())
            return;
        UISystemProfilerApi.AddMarker("Button.onClick", this);
        onClick.Invoke();
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        Press();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        onEnter?.Invoke();
        base.OnPointerExit(eventData);
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {

        onExit?.Invoke();
        base.OnPointerEnter(eventData);
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        onClickDown?.Invoke();
        base.OnPointerDown(eventData);
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        onClickUp?.Invoke();
        base.OnPointerUp(eventData);
    }

    public virtual void OnSubmit(BaseEventData eventData)
    {
        Press();
        if (!IsActive() || !IsInteractable())
            return;

        DoStateTransition(SelectionState.Pressed, false);
        StartCoroutine(OnFinishSubmit());
    }

    private IEnumerator OnFinishSubmit()
    {
        var fadeTime = colors.fadeDuration;
        var elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        DoStateTransition(currentSelectionState, false);
    }
}