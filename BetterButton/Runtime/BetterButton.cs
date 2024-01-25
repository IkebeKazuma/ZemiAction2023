using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace PMP.BetterButton {
    [RequireComponent(typeof(Image))]
    public class BetterButton :
        Selectable,
        IPointerClickHandler,
        ISubmitHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler {

        RectTransform rt;
        public RectTransform rectTransform {
            get {
                if (rt == null)
                    rt = GetComponent<RectTransform>();
                return rt;
            }
        }

        public bool allowOnlyOnceInput = false;
        bool interacted = false;
        public bool ignoreExceptTargetInput = false;   // ターゲット入力以外は無視するか
        public PointerEventData.InputButton targetInputBtn;
        public void ResetInteractOnlyOnceFlag() { interacted = false; }

        // ラベル
        public TextMeshProUGUI uiLabel;

        [System.Serializable]
        public class PointerHandler {
            public PointerHandlerUnit left;
            public PointerHandlerUnit right;
            public PointerHandlerUnit middle;

            public PointerHandler(BetterButton _base) {
                left = new(_base);
                right = new(_base);
                middle = new(_base);
            }
        }

        [SerializeField] PointerHandler _pointerHandler;
        public PointerHandler pointerHandler {
            get {
                if (_pointerHandler == null) {
                    _pointerHandler = new PointerHandler(this);
                }
                return _pointerHandler;
            }
        }

        [System.Serializable]
        public class PointerHandlerUnit {

            private BetterButton _base;

            private int _clickCount = 0;
            public int clickCount { get => _clickCount; }
            public void AddClickCount(int amount = 1) => _clickCount += amount;
            public void ResetClickCount() => _clickCount = 0;

            public bool isPointerDown;
            public bool isDragging;

            /// <summary>
            /// ボタンが押されているかどうか
            /// </summary>
            public bool IsPressed() {
                if (_base) {
                    if (!_base.IsActive() || !_base.IsInteractable())
                        return false;
                    return isPointerDown;
                } else
                    return false;
            }

            // Click
            public UnityAction onClick;
            // Down & Up & Hold
            public UnityAction onPointerDown;
            public UnityAction onPointerUp;
            // Drag
            public UnityAction onBeginDrag;
            public UnityAction onDrag;
            public UnityAction onEndDrag;

            // 押された瞬間のTime.timeを格納
            private float _pressedTime = -1f;
            public float pressedTime { get => _pressedTime; }

            // 離された瞬間のTime.timeを格納
            private float _releasedTime = -1f;
            public float releasedTime { get => _releasedTime; }

            // ホールドの時間
            public float holdTime {
                get {
                    if (_pressedTime > 0) {
                        if (_releasedTime > 0)
                            return _releasedTime - _pressedTime;
                        else
                            return Time.time - _pressedTime;
                    } else
                        return 0;
                }
            }

            /// <summary>
            /// 押された瞬間のTime.timeを記録
            /// </summary>
            public void RecordPressedTime() { _pressedTime = Time.time; }
            /// <summary>
            /// 離された瞬間のTime.timeを記録
            /// </summary>
            public void RecordReleasedTime() { _releasedTime = Time.time; }

            public void ResetPressedReleasedTimer() { _pressedTime = _releasedTime = -1; }

            public PointerHandlerUnit(BetterButton _base) {
                this._base = _base;
                Init();
            }

            public void Init() {
                {   // コールバック群初期化
                    onClick = null;
                    onPointerDown = null;
                    onPointerUp = null;
                    onBeginDrag = null;
                    onDrag = null;
                    onEndDrag = null;
                }

                ResetPressedReleasedTimer();

                isPointerDown = false;
                isDragging = false;
                ResetClickCount();
            }
        }

        public PointerHandlerUnit GetPointerStateHandler(PointerEventData.InputButton inputButton) {
            switch (inputButton) {
                case PointerEventData.InputButton.Left:
                    return pointerHandler.left;
                case PointerEventData.InputButton.Right:
                    return pointerHandler.right;
                case PointerEventData.InputButton.Middle:
                    return pointerHandler.middle;
                default: return null;
            }
        }

        [SerializeField] private bool _isPointerInside;
        public bool isPointerInside { get => _isPointerInside; }

        // Enter & Exit
        public UnityAction onPointerEnter;
        public UnityAction onPointerExit;
        // Select
        public UnityAction onSelected;
        public UnityAction onDeselected;

        // 左クリックのイベント
        public UnityAction onClick {
            get => pointerHandler.left.onClick;
            set => pointerHandler.left.onClick = value;
        }

        protected override void OnEnable() {
            transition = Transition.SpriteSwap;
            if (_pointerHandler == null) _pointerHandler = new PointerHandler(this);
            base.OnEnable();

            //Debug.Log("OnEnable");
        }

        protected override void OnDisable() => base.OnDisable();

        /// <summary>
        /// 全てのマウスクリック操作がされた瞬間にコールされる
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData) {
            OnClick(eventData);

            // Debug.Log("OnPointerClick");
        }

        /// <summary>
        /// 全てのマウスクリックダウン操作がされた瞬間にコールされる
        /// </summary>
        /// <param name="eventData"></param>        
        public override void OnPointerDown(PointerEventData eventData) {
            base.OnPointerDown(eventData);

            if (!IsActive() || !IsInteractable())
                return;

            PointerHandlerUnit tHandler = GetPointerStateHandler(eventData.button);
            if (tHandler != null) {
                tHandler.isPointerDown = true;
                tHandler.ResetPressedReleasedTimer();
                tHandler.RecordPressedTime();
                tHandler.onPointerDown?.Invoke();
            }

            //DoSpriteSwap(spriteDefault);

            // Debug.Log("Down");
        }

        /// <summary>
        /// 全てのマウスクリックアップ操作がされた瞬間にコールされる
        /// </summary>
        /// <param name="eventData"></param>        
        public override void OnPointerUp(PointerEventData eventData) {
            base.OnPointerUp(eventData);

            if (!IsActive() || !IsInteractable())
                return;

            //state.isPointerDown = false;

            PointerHandlerUnit tHandler = GetPointerStateHandler(eventData.button);
            if (tHandler != null) {
                tHandler.isPointerDown = false;
                tHandler.RecordReleasedTime();
                tHandler.onPointerUp?.Invoke();
            }

            // Debug.Log("Up");
        }

        /// <summary>
        /// カーソルが検知範囲に入った時にコールされる
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnPointerEnter(PointerEventData eventData) {
            if (!IsActive() || !IsInteractable())
                return;

            base.OnPointerEnter(eventData);

            _isPointerInside = true;
            onPointerEnter?.Invoke();

            // Debug.Log("Enter");
        }

        public override void OnPointerExit(PointerEventData eventData) {
            if (!IsActive() || !IsInteractable())
                return;

            base.OnPointerExit(eventData);

            _isPointerInside = false;
            onPointerExit?.Invoke();

            // Debug.Log("Exit");
        }

        public void OnBeginDrag(PointerEventData eventData) {
            if (!IsActive() || !IsInteractable())
                return;

            PointerHandlerUnit tHandler = GetPointerStateHandler(eventData.button);
            if (tHandler != null) {
                tHandler.isDragging = true;
                tHandler.onBeginDrag?.Invoke();
            }

            // Debug.Log("Begin drag");
        }

        public void OnDrag(PointerEventData eventData) {
            if (!IsActive() || !IsInteractable())
                return;

            PointerHandlerUnit tHandler = GetPointerStateHandler(eventData.button);
            if (tHandler != null) {
                tHandler.onDrag?.Invoke();
            }

            // Debug.Log("Is dragging");
        }

        public void OnEndDrag(PointerEventData eventData) {
            if (!IsActive() || !IsInteractable())
                return;

            PointerHandlerUnit tHandler = GetPointerStateHandler(eventData.button);
            if (tHandler != null) {
                tHandler.isDragging = false;
                tHandler.onEndDrag?.Invoke();
            }

            // Debug.Log("End drag");
        }

        public override void OnSelect(BaseEventData eventData) {
            base.OnSelect(eventData);
            onSelected?.Invoke();

            // Debug.Log("Selected");
        }

        public override void OnDeselect(BaseEventData eventData) {
            base.OnDeselect(eventData);
            onDeselected?.Invoke();

            // Debug.Log("Deselected");
        }

        public void OnSubmit(BaseEventData eventData) {
            OnClick(eventData as PointerEventData);

            if (!IsActive() || !IsInteractable())
                return;

            DoStateTransition(SelectionState.Pressed, false);
            StartCoroutine(OnFinishSubmit());

            // Debug.Log("Submit");
        }

        private IEnumerator OnFinishSubmit() {
            var fadeTime = colors.fadeDuration;
            var elapsedTime = 0f;

            while (elapsedTime < fadeTime) {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            DoStateTransition(currentSelectionState, false);
        }

        void OnClick(PointerEventData eventData) {

            if (!IsActive() || !IsInteractable())
                return;

            if (allowOnlyOnceInput && interacted)
                return;

            // PointerEventData が null の場合は左クリックとして処理する
            if (eventData == null) {
                eventData = new PointerEventData(EventSystem.current);
                eventData.button = PointerEventData.InputButton.Left;
            }

            // ターゲット入力以外は無視するか
            if (CheckIgnoreInput(eventData.button))
                return;

            // ハンドラー取得
            PointerHandlerUnit tHandler = GetPointerStateHandler(eventData.button);

            // ハンドラーがない場合はreturn
            if (tHandler == null) {
                Debug.LogError("[PM Presents/Better Button] ステートハンドラーの取得に失敗しました。");
                return;
            }

            if (allowOnlyOnceInput) interacted = true;

            tHandler.isPointerDown = false;
            tHandler.AddClickCount();
            tHandler.onClick?.Invoke();

            UISystemProfilerApi.AddMarker("BetterButton.onClick", this);

            // Debug.Log("Click");
        }

        bool CheckIgnoreInput(PointerEventData.InputButton inputButton) {
            // ターゲット以外は無視する && ボタンが合わない 場合は true
            return ignoreExceptTargetInput && (inputButton != targetInputBtn);
        }

        /// <summary>
        /// ボタンを有効化します。
        /// （interactable = true）
        /// </summary>
        public void Activate() {
            interactable = true;
        }

        /// <summary>
        /// ボタンを無効化します。
        /// （interactable = false）
        /// </summary>
        public void Deactivate() {
            interactable = false;
        }

        /// <summary>
        /// テキストを変更します。
        /// </summary>
        public void ChangeLabelText(string text) {
            if (!uiLabel) return;
            uiLabel.text = text;
        }
    }
}