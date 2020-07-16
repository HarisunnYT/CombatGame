using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using CharTween;

public class FancyText : MonoBehaviour
{
    [System.Serializable]
    enum TextType
    {
        BounceIn
    }

    [SerializeField]
    private TextType textType;

    [SerializeField]
    private bool loop = false;

    private TMP_Text text;

    private Sequence textSequence;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        textSequence.Kill();

        if (textType == TextType.BounceIn)
            Tween3();
    }

    private void OnDisable()
    {
        textSequence.Kill();
    }

    private void Tween3()
    {
        var tweener = text.GetCharTweener();

        var lineInfo = text.textInfo.lineInfo[0];
        int start = lineInfo.firstCharacterIndex;
        int end = lineInfo.lastCharacterIndex;

        textSequence = DOTween.Sequence();

        for (var i = start; i <= end; ++i)
        {
            var timeOffset = Mathf.Lerp(0, 1, (i - start) / (float)(end - start + 1));
            var charSequence = DOTween.Sequence();
            charSequence.Append(tweener.DOLocalMoveY(i, 0.5f, 0.5f).SetEase(Ease.InOutCubic))
                .Join(tweener.DOFade(i, 0, 0.5f).From())
                .Join(tweener.DOScale(i, 0, 0.5f).From().SetEase(Ease.OutBack, 5))
                .Append(tweener.DOLocalMoveY(i, 0, 0.5f).SetEase(Ease.OutBounce));
            textSequence.Insert(timeOffset, charSequence);
        }

        if (loop)
            textSequence.SetLoops(-1, LoopType.Yoyo);
    }
}
