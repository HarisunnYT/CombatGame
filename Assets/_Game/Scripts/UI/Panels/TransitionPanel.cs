using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using CharTween;

public class TransitionPanel : Panel
{
    [SerializeField]
    private TMP_Text loadingText;

    private Sequence loadingTextSequence;

    protected override void OnShow()
    {
        PlayLoadingTween();
        Time.timeScale = 1;
    }

    protected override void OnClose()
    {
        loadingTextSequence.Kill();
    }

    private void PlayLoadingTween()
    {
        var tweener = loadingText.GetCharTweener();

        var lineInfo = loadingText.textInfo.lineInfo[0];
        int start = lineInfo.firstCharacterIndex;
        int end = lineInfo.lastCharacterIndex;

        loadingTextSequence = DOTween.Sequence();

        for (var i = start; i <= end; ++i)
        {
            tweener.SetLocalScale(i, Vector3.one);
            tweener.SetAlpha(i, 1);
        }

        for (var i = start; i <= end; ++i)
        {
            var timeOffset = Mathf.Lerp(0, 1, (i - start) / (float)(end - start + 1));
            var charSequence = DOTween.Sequence();

            charSequence.Append(tweener.DOLocalMoveY(i, 0.5f, 0.5f).SetEase(Ease.InOutCubic))
                .Join(tweener.DOFade(i, 0, 0.5f).From())
                .Join(tweener.DOScale(i, 0, 0.5f).From().SetEase(Ease.OutBack, 5))
                .Append(tweener.DOLocalMoveY(i, 0, 0.5f).SetEase(Ease.OutBounce))
                .Append(tweener.DOScale(i, 0f, 0.5f).SetEase(Ease.InOutCubic));

            loadingTextSequence.Insert(timeOffset, charSequence);
        }

        loadingTextSequence.SetLoops(-1, LoopType.Restart);
    }
}
