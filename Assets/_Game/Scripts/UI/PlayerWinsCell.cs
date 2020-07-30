using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWinsCell : PlayerCell
{
    [SerializeField]
    private GameObject winTokenPrefab;

    [SerializeField]
    private Transform winsParents;

    private int previousWinsAmount;

    private void OnEnable()
    {
        if (previousWinsAmount < MatchManager.Instance.GetWins(PlayerController))
        {
            StartCoroutine(DelayedTokenCreation());
        }
    }

    private void CreateToken()
    {
        Instantiate(winTokenPrefab, winsParents);
        previousWinsAmount = MatchManager.Instance.GetWins(PlayerController);
    }

    private IEnumerator DelayedTokenCreation()
    {
        yield return new WaitForSecondsRealtime(1.75f);

        CreateToken();
    }
}
