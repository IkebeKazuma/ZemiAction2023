using UnityEngine;

namespace PMP.BetterButton {
    [RequireComponent(typeof(BetterButton))]
    public class BetterButtonAssistantBehaviour : MonoBehaviour {

        [SerializeField]
        private BetterButton _betterButton;
        public BetterButton betterButton {
            get {
                if (_betterButton == null) {
                    var c = GetComponent<BetterButton>();
                    if (!c) {
                        Debug.LogError("BetterButtonコンポーネントの自動取得に失敗しました。\nインスペクタから手動で設定してください。");
                    } else
                        _betterButton = c;
                }
                return _betterButton;
            }
        }

        public BetterButton overrideBetterButton {
            set {
                _betterButton = value;
            }
        }

        private RectTransform _rTrns;
        public RectTransform rectTransform {
            get {
                if (_rTrns == null) {
                    var rt = betterButton.GetComponent<RectTransform>();
                    _rTrns = rt;
                }
                return _rTrns;
            }
        }
    }
}