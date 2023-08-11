using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PMP.BetterButton {
    public class BetterButton :
        Selectable,
        IPointerClickHandler,
        ISubmitHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler {

        RectTransform rt;
        public RectTransform rectTransform {
            get {
                if (rt == null) rt = GetComponent<RectTransform>();
                return rt;
            }
        }

        public bool interactOnlyOnce = false;
        bool interacted = false;
        public bool ignoreExceptTarget = false;   // ターゲット以外は無視するか　例：左クリックにしか反応しない
        public PointerEventData.InputButton targetInputBtn;
        public void ResetInteractOnlyOnceFlag() { interacted = false; }

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
            /// ボタンが押されているか
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
            public UnityAction onPointerClick;
            // Down & Up
            public UnityAction onPointerDown;
            public UnityAction onPointerUp;
            // Drag
            public UnityAction onBeginDrag;
            public UnityAction onDrag;
            public UnityAction onEndDrag;

            private float _downTime = -1f;
            public float downTime { get => _downTime; }   // 押された瞬間のTime.timeを格納
            private float _upTime = -1f;
            public float upTime { get => _upTime; }   // 離された瞬間のTime.timeを格納
            public float holdTime {
                get {
                    if (_downTime > 0) {
                        if (_upTime > 0) {
                            return _upTime - _downTime;
                        } else {
                            return Time.time - _downTime;
                        }
                    } else
                        return 0;
                }
            }

            public void SetDownTime() => _downTime = Time.time;
            public void SetUpTime() => _upTime = Time.time;
            public void ResetTimer() {
                _downTime = _upTime = -1;
            }

            public PointerHandlerUnit(BetterButton _base) {
                this._base = _base;
                Init();
            }

            public void Init() {
                {   // コールバック群初期化
                    onPointerClick = null;
                    onPointerDown = null;
                    onPointerUp = null;
                    onBeginDrag = null;
                    onDrag = null;
                    onEndDrag = null;
                }

                ResetTimer();

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
            get {
                return pointerHandler.left.onPointerClick;
            }
            set {
                pointerHandler.left.onPointerClick = value;
            }
        }

        protected override void OnEnable() {
            transition = Transition.SpriteSwap;
            if (_pointerHandler == null) _pointerHandler = new PointerHandler(this);
            base.OnEnable();
            //Debug.Log("OnEnable");
        }

        protected override void OnDisable() => base.OnDisable();

        /// <summary>
        /// 全てのマウスクリックダウン操作がされた瞬間にコールされる
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData) {
            if (interactOnlyOnce && interacted) return;
            OnClick(eventData);
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
                tHandler.ResetTimer();
                tHandler.SetDownTime();
                tHandler.onPointerDown?.Invoke();
            }

            //DoSpriteSwap(spriteDefault);

            //Debug.Log("Down");
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
                tHandler.SetUpTime();
                tHandler.onPointerUp?.Invoke();
            }

            //Debug.Log("Up");
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
            //Debug.Log("Enter");
        }

        public override void OnPointerExit(PointerEventData eventData) {
            if (!IsActive() || !IsInteractable())
                return;

            base.OnPointerExit(eventData);

            _isPointerInside = false;
            onPointerExit?.Invoke();
            //Debug.Log("Exit");
        }

        public void OnBeginDrag(PointerEventData eventData) {
            if (!IsActive() || !IsInteractable())
                return;

            PointerHandlerUnit tHandler = GetPointerStateHandler(eventData.button);
            if (tHandler != null) {
                tHandler.isDragging = true;
                tHandler.onBeginDrag?.Invoke();
            }
            //Debug.Log("Begin drag");
        }

        public void OnDrag(PointerEventData eventData) {
            if (!IsActive() || !IsInteractable())
                return;

            PointerHandlerUnit tHandler = GetPointerStateHandler(eventData.button);
            if (tHandler != null) {
                tHandler.onDrag?.Invoke();
            }
            //Debug.Log("Is dragging");
        }

        public void OnEndDrag(PointerEventData eventData) {
            if (!IsActive() || !IsInteractable())
                return;

            PointerHandlerUnit tHandler = GetPointerStateHandler(eventData.button);
            if (tHandler != null) {
                tHandler.isDragging = false;
                tHandler.onEndDrag?.Invoke();
            }
            //Debug.Log("End drag");
        }

        public override void OnSelect(BaseEventData eventData) {
            base.OnSelect(eventData);
            onSelected?.Invoke();
            //state.hasSelection = true;
            //Debug.Log("Selected");
        }

        public override void OnDeselect(BaseEventData eventData) {
            base.OnDeselect(eventData);
            onDeselected?.Invoke();
            //state.hasSelection = false;
            //Debug.Log("Deselected");
        }

        public void OnSubmit(BaseEventData eventData) {
            OnClick(null);
        }

        void OnClick(PointerEventData eventData) {
            if (!IsActive() || !IsInteractable())
                return;

            if (eventData != null) {
                PointerHandlerUnit tHandler = GetPointerStateHandler(eventData.button);
                if (tHandler != null) {
                    tHandler.isPointerDown = false;
                    tHandler.onPointerClick?.Invoke();

                    if (interactOnlyOnce) {
                        if (ignoreExceptTarget) {
                            if (eventData.button == targetInputBtn) interacted = true;
                        } else
                            interacted = true;
                    }
                }
            } else {
                if (interactOnlyOnce) interacted = true;
                pointerHandler.left.AddClickCount();
                pointerHandler.left.onPointerClick?.Invoke();
            }
            //Debug.Log("Click");
        }
    }
}