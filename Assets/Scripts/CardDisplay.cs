using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI subTitleText;
    [SerializeField] TextMeshProUGUI descriptionText;

    [SerializeField] GameObject cardBack;

    [SerializeField] GameObject valueCircle;
    [SerializeField] GameObject healCircleBG;
    [SerializeField] GameObject damageCircleBG;

    [SerializeField] TextMeshProUGUI valueText;
    [SerializeField] GameObject[] blockerOverlay;
    bool isBlockerActive;

    [SerializeField] Image cardBG;
    [SerializeField] Sprite blockBG;
    [SerializeField] Sprite reverseBG;
    [SerializeField] Sprite revealBG;
    [SerializeField] Sprite instantDeathBG;
    [SerializeField] Sprite damageBG;
    [SerializeField] Sprite healBG;

    public bool CanInteract { get; set; }

    public bool IsCardActive
    {
        get => !isBlockerActive;
        set
        {
            isBlockerActive = !value;
            foreach (var item in blockerOverlay)
            {
                item.SetActive(isBlockerActive);
            }
        }
    }

    public Card Card { get; private set; }

    bool CanShowValueCircle => !cardBack.activeSelf && Card.cardType switch
    {
        CardTypesEnum.Damage => true,
        CardTypesEnum.Heal => true,
        _ => false
    };
    public void Initialize(Card cardData)
    {
        Card = cardData;
        titleText.text = Card.title;
        subTitleText.text = Card.subTitle;
        descriptionText.text = Card.description;
        valueText.text = GetValueText(Card.cardType, Card.value);
        healCircleBG.SetActive(Card.cardType == CardTypesEnum.Heal);
        damageCircleBG.SetActive(Card.cardType == CardTypesEnum.Damage);
        valueCircle.SetActive(CanShowValueCircle);
        cardBG.sprite = GetCardBG(Card.cardType);
    }

    public void ShowBack(bool showBack)
    {
        cardBack.SetActive(showBack);
        valueCircle.SetActive(CanShowValueCircle);
    }

    public IEnumerator FlipCard(float duration, bool finalState)
    {
        AudioManager.PlayClip(AudioManager.CardSoundClip);

        duration /= 2;

        ShowBack(finalState);

        float f = 0;

        while (f < 1)
        {
            f += Time.deltaTime / duration;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.up, f);
            yield return null;
        }

        ShowBack(!finalState);

        while (f > 0)
        {
            f -= Time.deltaTime / duration;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.up, f);
            yield return null;
        }
    }

    private Sprite GetCardBG(CardTypesEnum cardType)
    {
        return cardType switch
        {
            CardTypesEnum.Damage => damageBG,
            CardTypesEnum.Heal => healBG,
            CardTypesEnum.Block => blockBG,
            CardTypesEnum.Reflect => reverseBG,
            CardTypesEnum.Reveal => revealBG,
            _ => instantDeathBG,
        };
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsCardActive && CanInteract)
        {
            FindObjectOfType<GameManager>().PlayerSelectedCard(this);
        }
    }

    string GetValueText(CardTypesEnum cardType, int value)
    {
        return cardType switch
        {
            CardTypesEnum.Damage => $"-{value}",
            CardTypesEnum.Heal => $"+{value}",
            CardTypesEnum.InstantDeath => $"-\u221E",
            _ => string.Empty,
        };
    }
}
