using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleUITips
{
    #region Public Models

    public enum FixedUIPosType
    {
        Left,
        Right,
        Top,
        Bottom,
        Center
    }

    public enum CircleProgressType
    {
        Auto,
        Manual,
    }

    /// <summary>
    /// 氣泡信息
    /// </summary>
    public class BubbleInfo : IComparable<BubbleInfo>
    {
        public List<string> actionKeys;     // 交互键位显示（如："E"、"F"、"Space"）
        public GameObject creator;          // 谁创建了气泡（世界坐标跟随）
        public GameObject player;           // 用于排序（距离 player 最近者优先）
        public string content;              // 气泡内容
        public string itemName;             // 物体名

        public BubbleInfo(List<string> actionKeys, GameObject creator, GameObject player, string content, string itemName)
        {
            this.actionKeys = actionKeys;
            this.creator = creator;
            this.player = player;
            this.content = content;
            this.itemName = itemName;
        }

        public int CompareTo(BubbleInfo other)
        {
            if (creator == null || player == null) return 1;
            if (other == null || other.creator == null || other.player == null) return -1;
            float myDist = Vector3.Distance(creator.transform.position, player.transform.position);
            float otherDist = Vector3.Distance(other.creator.transform.position, other.player.transform.position);
            return myDist <= otherDist ? -1 : 1;
        }
    }

    /// <summary>
    /// 飘字提示信息
    /// </summary>
    public class TipInfo
    {
        public string content;
        public Vector3 worldPos;
        public float showTime; // 停留时长

        public TipInfo(string content, Vector3 worldPos, float showTime = 0.75f)
        {
            this.content = content;
            this.worldPos = worldPos;
            this.showTime = showTime;
        }
    }

    /// <summary>
    /// 图标提示信息
    /// </summary>
    public class SignInfo
    {
        public string signIconPath; // Resources 下的路径
        public GameObject creator;  // 跟随对象
        public float showTime;

        public SignInfo(string signIconPath, GameObject creator, float showTime = 0.75f)
        {
            this.signIconPath = signIconPath;
            this.creator = creator;
            this.showTime = showTime;
        }
    }

    #endregion

    #region UI Items (no 3rd-party deps)

    internal static class UIConvert
    {
        public static Vector2 ScreenToUIPoint(RectTransform anyChildRt, Vector2 screenPos)
        {
            if (anyChildRt == null) return Vector2.zero;
            var parent = anyChildRt.parent as RectTransform;
            if (parent == null) return Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPos, GetUICamera(parent), out var lp);
            return lp;
        }

        public static Camera GetUICamera(Transform t)
        {
            var canvas = t.GetComponentInParent<Canvas>();
            if (canvas == null) return Camera.main;
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
                return canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
            return null; // ScreenSpaceOverlay
        }
    }

    /// <summary>
    /// 气泡条目
    /// </summary>
    public class UIBubbleItem : MonoBehaviour
    {
        public Text ContentText;
        public Image ContentImage; // 可选
        public Text KeyText;
        public Text ItemNameText;

        private GameObject _creator;
        private GameObject _player;
        private RectTransform _rt;

        private void Awake() => _rt = GetComponent<RectTransform>();

        public void Init(BubbleInfo info, string keys)
        {
            _creator = info.creator;
            _player = info.player;
            if (ContentText) ContentText.text = info.content;
            if (ItemNameText) ItemNameText.text = info.itemName;
            if (KeyText) KeyText.text = keys;

            transform.localScale = Vector3.zero;
            StartCoroutine(ScaleOverTime(Vector3.zero, Vector3.one, 0.2f));
        }

        public void UpdateContent(string content)
        {
            if (ContentText) ContentText.text = content;
        }

        private void LateUpdate()
        {
            if (_creator == null || _rt == null) return;
            var sp = Camera.main != null ? Camera.main.WorldToScreenPoint(_creator.transform.position) : Vector3.zero;
            _rt.localPosition = UIConvert.ScreenToUIPoint(_rt, sp);
        }

        public void DestroyBubble()
        {
            StartCoroutine(ScaleOverTime(Vector3.one, Vector3.zero, 0.2f, () => Destroy(gameObject)));
        }

        private IEnumerator ScaleOverTime(Vector3 from, Vector3 to, float duration, Action onComplete = null)
        {
            float t = 0f;
            while (t < duration)
            {
                transform.localScale = Vector3.Lerp(from, to, t / duration);
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            transform.localScale = to;
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// 通用圆形进度条条目（UGUI Image.fillAmount）
    /// </summary>
    public class UICommonProgressItem : MonoBehaviour
    {
        public Image ProgressBarImage;

        private RectTransform _rt;
        private GameObject _creator;
        public float OffsetX = 30f;
        public float OffsetY = 30f;

        private float _curProgress;
        private float _maxProgress;

        private string _key;
        private CircleProgressType _type;
        private Action _onComplete;

        private void Awake() => _rt = GetComponent<RectTransform>();

        public void Init(string key, CircleProgressType type, GameObject creator, float duration, Action onComplete)
        {
            _key = key;
            _type = type;
            _creator = creator;
            _maxProgress = Mathf.Max(0.0001f, duration);
            _curProgress = 0f;
            _onComplete = onComplete;

            if (ProgressBarImage) ProgressBarImage.fillAmount = 0f;
            RefreshPosition();
        }

        private void Update()
        {
            if (_type == CircleProgressType.Auto)
            {
                _curProgress += Time.deltaTime;
                if (ProgressBarImage)
                    ProgressBarImage.fillAmount = Mathf.Clamp01(_curProgress / _maxProgress);
                if (_curProgress >= _maxProgress)
                {
                    _onComplete?.Invoke();
                    Destroy(gameObject);
                    return;
                }
            }
            RefreshPosition();
        }

        private void RefreshPosition()
        {
            if (_rt == null || _creator == null) return;
            var sp = Camera.main != null ? Camera.main.WorldToScreenPoint(_creator.transform.position) : Vector3.zero;
            var lp = UIConvert.ScreenToUIPoint(_rt, sp);
            _rt.localPosition = new Vector2(lp.x + OffsetX, lp.y + OffsetY);
        }

        public void SetManualProgress01(float value01)
        {
            _type = CircleProgressType.Manual;
            if (ProgressBarImage) ProgressBarImage.fillAmount = Mathf.Clamp01(value01);
        }
    }

    /// <summary>
    /// 固定位置文本条目（可用于分数、状态）
    /// </summary>
    public class UIFixedPosTextItem : MonoBehaviour
    {
        public Text ContentText;

        private RectTransform _rt;
        private float _startY;

        private void Awake() => _rt = GetComponent<RectTransform>();

        public void Init(FixedUIPosType posType, string content)
        {
            if (ContentText) ContentText.text = content;
            transform.localScale = Vector3.zero;

            if (_rt != null)
            {
                var parent = _rt.parent as RectTransform;
                Vector2 target = Vector2.zero;
                if (parent != null)
                {
                    var half = parent.rect.size / 2f;
                    switch (posType)
                    {
                        case FixedUIPosType.Left:   target = new Vector2(-half.x + 120f, 0f); break;
                        case FixedUIPosType.Right:  target = new Vector2( half.x - 120f, 0f); break;
                        case FixedUIPosType.Top:    target = new Vector2(0f,  half.y - 80f); break;
                        case FixedUIPosType.Bottom: target = new Vector2(0f, -half.y + 80f); break;
                        case FixedUIPosType.Center: target = Vector2.zero; break;
                    }
                }
                _rt.localPosition = target;
                _startY = _rt.localPosition.y;
            }

            StartCoroutine(ScaleOverTime(Vector3.zero, Vector3.one, 0.3f));
            StartCoroutine(MoveYOverTime(_startY + 50f, 0.5f));
        }

        private IEnumerator ScaleOverTime(Vector3 from, Vector3 to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                transform.localScale = Vector3.Lerp(from, to, t / duration);
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            transform.localScale = to;
        }

        private IEnumerator MoveYOverTime(float targetY, float duration)
        {
            float t = 0f;
            Vector3 start = _rt.localPosition;
            while (t < duration)
            {
                _rt.localPosition = new Vector3(start.x, Mathf.Lerp(start.y, targetY, t / duration), start.z);
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            _rt.localPosition = new Vector3(start.x, targetY, start.z);
        }
    }

    /// <summary>
    /// 图标提示条目（跟随世界坐标）
    /// </summary>
    public class UISignItem : MonoBehaviour
    {
        public Image IconImg;
        public float OffsetY = 150f;

        private RectTransform _rt;
        private GameObject _creator;

        private void Awake() => _rt = GetComponent<RectTransform>();

        public void Init(GameObject creator, string iconPath)
        {
            _creator = creator;
            if (IconImg)
            {
                var sp = string.IsNullOrEmpty(iconPath) ? null : Resources.Load<Sprite>(iconPath);
                if (sp != null) IconImg.sprite = sp;
            }
            RefreshPosition();
        }

        private void LateUpdate() => RefreshPosition();

        private void RefreshPosition()
        {
            if (_rt == null || _creator == null) return;
            var sp = Camera.main != null ? Camera.main.WorldToScreenPoint(_creator.transform.position) : Vector3.zero;
            var lp = UIConvert.ScreenToUIPoint(_rt, sp);
            _rt.localPosition = new Vector2(lp.x, lp.y + OffsetY);
        }
    }

    /// <summary>
    /// 飘字提示条目（缩放+上移，定时销毁）
    /// </summary>
    public class UITipItem : MonoBehaviour
    {
        public Text ContentText;
        public float MoveUp = 50f;
        public float ScaleInTime = 0.3f;
        public float MoveTime = 0.5f;

        private RectTransform _rt;
        private float _startY;
        private float _deadTime;
        private Coroutine _lifeCoro;

        private void Awake() => _rt = GetComponent<RectTransform>();

        public void Init(Vector3 worldPos, string content, float deadTime)
        {
            if (ContentText) ContentText.text = content;
            _deadTime = Mathf.Max(0f, deadTime);

            transform.localScale = Vector3.zero;

            var sp = Camera.main != null ? Camera.main.WorldToScreenPoint(worldPos) : Vector3.zero;
            _rt.localPosition = UIConvert.ScreenToUIPoint(_rt, sp);
            _startY = _rt.localPosition.y;

            StartCoroutine(ScaleOverTime(Vector3.zero, Vector3.one, ScaleInTime));
            StartCoroutine(MoveYOverTime(_startY + MoveUp, MoveTime));
            if (_lifeCoro != null) StopCoroutine(_lifeCoro);
            _lifeCoro = StartCoroutine(LifeTimer(_deadTime));
        }

        public void ResetTip(Vector3 worldPos, string content)
        {
            if (ContentText) ContentText.text = content;
            StopAllCoroutines();
            transform.localScale = Vector3.zero;

            var sp = Camera.main != null ? Camera.main.WorldToScreenPoint(worldPos) : Vector3.zero;
            _rt.localPosition = UIConvert.ScreenToUIPoint(_rt, sp);
            _startY = _rt.localPosition.y;

            StartCoroutine(ScaleOverTime(Vector3.zero, Vector3.one, ScaleInTime));
            StartCoroutine(MoveYOverTime(_startY + MoveUp, MoveTime));
            if (_lifeCoro != null) StopCoroutine(_lifeCoro);
            _lifeCoro = StartCoroutine(LifeTimer(_deadTime));
        }

        private IEnumerator LifeTimer(float t)
        {
            yield return new WaitForSeconds(t);
            Destroy(gameObject);
        }

        private IEnumerator ScaleOverTime(Vector3 from, Vector3 to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                transform.localScale = Vector3.Lerp(from, to, t / duration);
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            transform.localScale = to;
        }

        private IEnumerator MoveYOverTime(float targetY, float duration)
        {
            float t = 0f;
            Vector3 start = _rt.localPosition;
            while (t < duration)
            {
                _rt.localPosition = new Vector3(start.x, Mathf.Lerp(start.y, targetY, t / duration), start.z);
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            _rt.localPosition = new Vector3(start.x, targetY, start.z);
        }
    }

    #endregion

    /// <summary>
    /// 独立的 UI 助手（单例），无第三方依赖。将本脚本挂到一个 Canvas 下。
    /// </summary>
    public class UIHelperStandalone : Singleton<UIHelperStandalone>
    {
        [Header("Bubble")]
        public GameObject BubblePrefab;

        private GameObject _currentBubble;
        private BubbleInfo _currentBubbleInfo;
        public readonly List<BubbleInfo> BubbleList = new List<BubbleInfo>();

        [Header("Tip")]
        public GameObject TipPrefab;
        public float ShowTipInterval = 0.15f;
        public float QuickReplaceThreshold = 0.3f;
        private readonly Queue<TipInfo> _tipQueue = new Queue<TipInfo>();
        private GameObject _currentTipGO;
        private float _lastTipShowTime;

        [Header("Sign")]
        public GameObject SignPrefab;

        [Header("FixedPos")]
        public GameObject FixedPosTextPrefab;

        [Header("Progress")]
        public GameObject CircleProgressPrefab;
        private readonly Dictionary<string, GameObject> _circleProgressDict = new Dictionary<string, GameObject>();

        #region Bubble API

        private void RefreshBubble()
        {
            if (BubbleList == null || BubbleList.Count == 0) return;

            var tmp = BubbleList[0];
            if (_currentBubbleInfo != null && tmp.creator == _currentBubbleInfo.creator) return;

            if (_currentBubble != null) DestroyBubble();

            _currentBubbleInfo = tmp;
            _currentBubble = Instantiate(BubblePrefab, transform, false);

            var bubbleItem = _currentBubble.GetComponent<UIBubbleItem>();
            string keyStr = string.Empty;
            if (tmp.actionKeys != null && tmp.actionKeys.Count > 0)
                keyStr = string.Join("/", tmp.actionKeys);
            if (bubbleItem != null)
                bubbleItem.Init(tmp, keyStr);
        }

        public void DestroyBubble()
        {
            if (_currentBubble)
            {
                var it = _currentBubble.GetComponent<UIBubbleItem>();
                if (it != null) it.DestroyBubble();
                else Destroy(_currentBubble);
                _currentBubble = null;
                _currentBubbleInfo = null;
            }
        }

        public void AddBubble(BubbleInfo info)
        {
            if (info == null || info.creator == null) return;
            if (BubbleList.Exists(x => x.creator == info.creator)) return;
            BubbleList.Add(info);
            BubbleList.Sort();
            RefreshBubble();
        }

        public void RemoveBubble(GameObject creator)
        {
            if (creator == null) return;
            var idx = BubbleList.FindIndex(x => x.creator == creator);
            if (idx >= 0) BubbleList.RemoveAt(idx);
            if (BubbleList.Count == 0) DestroyBubble();
            else RefreshBubble();
        }

        public void UpdateBubbleContent(GameObject creator, string content)
        {
            if (creator == null || string.IsNullOrEmpty(content)) return;
            var info = BubbleList.Find(x => x.creator == creator);
            if (info == null) return;
            info.content = content;
            if (_currentBubble != null)
            {
                var it = _currentBubble.GetComponent<UIBubbleItem>();
                if (it != null) it.UpdateContent(content);
            }
        }

        public void SortBubbleByDistance()
        {
            if (BubbleList == null || BubbleList.Count == 0) return;
            var oldNearest = BubbleList[0];
            BubbleList.Sort();
            if (oldNearest != BubbleList[0]) RefreshBubble();
        }

        #endregion

        #region Tip API

        public void ShowTip(TipInfo tip)
        {
            float now = Time.time;
            float delta = now - _lastTipShowTime;

            if (delta <= QuickReplaceThreshold && _currentTipGO != null)
            {
                var item = _currentTipGO.GetComponent<UITipItem>();
                if (item != null)
                {
                    item.ResetTip(tip.worldPos, tip.content);
                    _lastTipShowTime = now;
                    return;
                }
            }

            _tipQueue.Enqueue(tip);
            StopCoroutine(nameof(ShowTipCoroutine));
            StartCoroutine(nameof(ShowTipCoroutine));
        }

        private IEnumerator ShowTipCoroutine()
        {
            while (_tipQueue.Count > 0)
            {
                var info = _tipQueue.Dequeue();
                _lastTipShowTime = Time.time;
                _currentTipGO = Instantiate(TipPrefab, transform, false);
                var it = _currentTipGO.GetComponent<UITipItem>();
                if (it != null) it.Init(info.worldPos, info.content, info.showTime);
                yield return new WaitForSeconds(ShowTipInterval);
            }
        }

        #endregion

        #region Sign API

        public void ShowSign(SignInfo sign)
        {
            var go = Instantiate(SignPrefab, transform, false);
            var it = go.GetComponent<UISignItem>();
            if (it != null) it.Init(sign.creator, sign.signIconPath);
            Destroy(go, sign.showTime);
        }

        #endregion

        #region FixedPos API

        public void ShowFixedText(FixedUIPosType pos, string content, float lifeTime)
        {
            var go = Instantiate(FixedPosTextPrefab, transform, false);
            var it = go.GetComponent<UIFixedPosTextItem>();
            if (it != null) it.Init(pos, content);
            Destroy(go, lifeTime);
        }

        #endregion

        #region Progress API

        public void ShowCircleProgress(string key, CircleProgressType type, GameObject creator, float duration = 0f)
        {
            if (string.IsNullOrEmpty(key) || creator == null) return;
            if (_circleProgressDict.ContainsKey(key)) return;
            var go = Instantiate(CircleProgressPrefab, transform, false);
            var it = go.GetComponent<UICommonProgressItem>();
            if (it != null)
            {
                it.Init(key, type, creator, duration, () => { _circleProgressDict.Remove(key); });
                _circleProgressDict[key] = go;
            }
            else
            {
                Destroy(go);
            }
        }

        public void SetCircleProgress01(string key, float value01)
        {
            if (!_circleProgressDict.TryGetValue(key, out var go) || go == null) return;
            var it = go.GetComponent<UICommonProgressItem>();
            if (it != null) it.SetManualProgress01(value01);
        }

        public void DestroyCircleProgress(string key)
        {
            if (!_circleProgressDict.ContainsKey(key)) return;
            if (_circleProgressDict[key] != null) Destroy(_circleProgressDict[key]);
            _circleProgressDict.Remove(key);
        }

        #endregion
    }
}

/*
======================== 使用说明 ========================
1) 准备：
   - 在场景中创建一个 Canvas（建议 Screen Space - Overlay）。
   - 将 UIHelperStandalone 脚本挂到 Canvas 上（或它的子物体上）。
   - 提供 5 个 Prefab（可复用）：
       - BubblePrefab：包含 UIBubbleItem 与 Text 组件（ContentText、KeyText、ItemNameText）
       - TipPrefab：包含 UITipItem 与 Text 组件（ContentText）
       - SignPrefab：包含 UISignItem 与 Image 组件（IconImg）
       - CircleProgressPrefab：包含 UICommonProgressItem 与 Image 组件（ProgressBarImage，Image.type=Filled, FillAmount=0）
       - FixedPosTextPrefab：包含 UIFixedPosTextItem 与 Text 组件（ContentText）

2) 调用示例：
   SimpleUITips.UIHelperStandalone.Instance.AddBubble(
       new SimpleUITips.BubbleInfo(new List<string>{"E"}, obj, player, "交互：打开", "门"));

   SimpleUITips.UIHelperStandalone.Instance.ShowTip(
       new SimpleUITips.TipInfo("+10", someWorldPos, 0.75f));

   SimpleUITips.UIHelperStandalone.Instance.ShowSign(
       new SimpleUITips.SignInfo("Icons/Warning", obj, 1.0f));

   SimpleUITips.UIHelperStandalone.Instance.ShowFixedText(
       SimpleUITips.FixedUIPosType.Top, "获得成就", 1.5f);

   SimpleUITips.UIHelperStandalone.Instance.ShowCircleProgress(
       "craft-1", SimpleUITips.CircleProgressType.Auto, obj, 2.0f);
   // 手动模式：
   SimpleUITips.UIHelperStandalone.Instance.ShowCircleProgress(
       "charge", SimpleUITips.CircleProgressType.Manual, obj);
   SimpleUITips.UIHelperStandalone.Instance.SetCircleProgress01("charge", 0.5f);
   SimpleUITips.UIHelperStandalone.Instance.DestroyCircleProgress("charge");

3) 与原工程差异：
   - 移除了 DOTween / TMPro / InputSystem / 其他外部框架依赖。
   - 动画一律改为原生协程（缩放/位移动画）。
   - 统一了坐标换算，避免分辨率硬编码与乱码注释。
   - 修复命名错误（如 Destory -> Destroy）。
   - Bubble 键位改为字符串列表，使用 \"/\" 连接显示。
=========================================================
*/
