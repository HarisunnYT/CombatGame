using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoveCell : MonoBehaviour
{
    [SerializeField]
    private Image moveImage;

    [SerializeField]
    private TMP_Text cooldownText;

    [SerializeField]
    private Image attackType;

    [Space()]
    [SerializeField]
    private Sprite offensiveSprite;

    [SerializeField]
    private Sprite defensiveSprite;

    public MoveData MoveData { get; private set; }

    protected CharacterPurchasePanel purchasePanel;

    private void Start()
    {
        purchasePanel = GetComponentInParent<CharacterPurchasePanel>();
    }

    public virtual void Configure(MoveData moveData)
    {
        this.MoveData = moveData;

        moveImage.sprite = moveData.Icon;
        cooldownText.text = moveData.Cooldown.ToString();
        attackType.sprite = moveData.AttackType == AttackType.Offensive ? offensiveSprite : defensiveSprite;
    }
}
