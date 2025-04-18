using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Common")]
    [SerializeField] int maxHP = 100;
    [SerializeField] int currentHearts = 3;
    [SerializeField] Card[] allCards;
    int CurrentHP;

    [Header("Panels")]
    [SerializeField] GameObject[] boardDisplay;
    [SerializeField] GameObject[] cardGameDisplay;

    [Header("Card Game Win Screen")]
    [SerializeField] GameObject winScreen;
    [SerializeField] Image loserProfile;
    [SerializeField] TextMeshProUGUI snakeLoserText;

    [Header("Card Game Lose Screen")]
    [SerializeField] GameObject loseScreen;
    [SerializeField] TextMeshProUGUI snakeWinnerText;

    [Header("Board")]
    [SerializeField] Button diceRollButton;
    [SerializeField] Transform diceRollDisplay;
    [SerializeField] Transform grid;
    [SerializeField] float numberChangeInterval;
    [SerializeField] int numberChangeTimes;
    [SerializeField] TextMeshProUGUI hpTextDisplay;
    [SerializeField] Image hpFillImage;
    [SerializeField] Transform heartsHolder;
    [SerializeField] int healAmount;
    [SerializeField] int damageAmount;
    [SerializeField] int bananaGoBackSpaces = 3;
    [SerializeField] float bananaSlipDuration = 1;
    [SerializeField] GameObject healDisplay;
    [SerializeField] GameObject damageDisplay;


    [Header("Board character display")]
    [SerializeField] Transform characterDisplay;
    [SerializeField] AnimationCurve characterMoveCurve;
    [SerializeField] AnimationCurve characterMoveScaleCurve;
    [SerializeField] float boardJumpTime = 0.25f;

    int characterPosition = 0;

    [Header("Card Fight UI")]
    [SerializeField] List<Card> collectedCards;
    [SerializeField] Transform enemyCardDisplay;
    [SerializeField] Transform playerCardDisplay;
    [SerializeField] TextMeshProUGUI snakeHPText;
    [SerializeField] Image snakeHPFillImage;
    [SerializeField] TextMeshProUGUI playerHPText;
    [SerializeField] Image playerHPFillImage;
    [SerializeField] float cardFlipDuration = 0.5f;
    [SerializeField] Image countDownTimerFill;
    [SerializeField] TextMeshProUGUI countDownTimerText;
    [SerializeField] int fillTimer = 5;
    [SerializeField] float snakeTraversalDuration = 0.5f;
    [SerializeField] Image playerDisplayImage;
    [SerializeField] Sprite playerNormal;
    [SerializeField] Sprite playerDamage;
    [SerializeField] Image snakeDisplayImage;
    [SerializeField] TextMeshProUGUI dialogueText;

    [Header("Mystery box")]
    [SerializeField] GameObject mysteryBoxPanel;
    [SerializeField] TextMeshProUGUI mysteryBoxRewardText;

    [Header("VSScreen")]
    [SerializeField] GameObject vsScreen;
    [SerializeField] Image vsScreenSnakePotrait;
    [SerializeField] TextMeshProUGUI vsScreenSnakeSnakeName;

    void Start()
    {
        vsScreen.SetActive(false);
        dialogueText.transform.parent.gameObject.SetActive(false);
        mysteryBoxPanel.SetActive(false);
        winScreen.SetActive(false);
        loseScreen.SetActive(false);

        SetDiceFace(1);
        healDisplay.SetActive(false);
        damageDisplay.SetActive(false);
        CurrentHP = maxHP;
        UpdateHPDisplay();
        characterDisplay.position = grid.GetChild(characterPosition).position;
        ToggleBoard(true);
        ToggleCardGame(false);
        AudioManager.PlayClip(AudioManager.BoardBGMClip);
    }

    public void RollDice()
    {
        StartCoroutine(DiceTurn());
    }

    void SetDiceFace(int face)
    {
        //face--;
        for (int i = 1; i < diceRollDisplay.childCount; i++)
        {
            diceRollDisplay.GetChild(i).gameObject.SetActive(i == face);
        }
    }

    IEnumerator DiceTurn()
    {
        int maxRoll = 6;
        AudioManager.PlayClip(AudioManager.DiceRollClip);
        diceRollButton.interactable = false;
        int diceRoll = Random.Range(1, maxRoll + 1);
        if (characterPosition + diceRoll >= grid.childCount)
        {
            diceRoll = grid.childCount - 1 - characterPosition;
        }

        for (int i = 0; i < numberChangeTimes; i++)
        {
            SetDiceFace(Random.Range(1, maxRoll + 1));
            yield return new WaitForSeconds(numberChangeInterval);
        }

        SetDiceFace(diceRoll);
        int targetPosition = characterPosition + diceRoll;
        Vector3 originalScale = characterDisplay.localScale;
        for (; characterPosition < targetPosition; characterPosition++)
        {
            AudioManager.PlayClip(AudioManager.HoppingClip);
            Vector3 startPos = grid.GetChild(characterPosition).position;
            Vector3 endPos = grid.GetChild(characterPosition + 1).position;
            float f = 0;

            while (f < 1)
            {
                f += Time.deltaTime / boardJumpTime;
                characterDisplay.position = Vector3.Lerp(startPos, endPos, characterMoveCurve.Evaluate(f));
                characterDisplay.localScale = originalScale * characterMoveScaleCurve.Evaluate(f);
                yield return null;
            }

            f = 1;
            characterDisplay.position = Vector3.Lerp(startPos, endPos, f);
            characterDisplay.localScale = originalScale * characterMoveScaleCurve.Evaluate(f);
        }

        characterDisplay.localScale = originalScale;

        yield return StartCoroutine(CheckCurrentTile());


        diceRollButton.interactable = true;
    }

    IEnumerator CheckCurrentTile()
    {
        TileScript tile = grid.GetChild(characterPosition).GetComponent<TileScript>();
        if (tile != null)
        {
            switch (tile.tileType)
            {
                case TileTypes.Heal:
                    Heal(healAmount);
                    break;
                case TileTypes.Snake:
                    yield return StartCoroutine(CardFight(tile));
                    break;
                case TileTypes.Damage:
                    Damage(damageAmount);
                    break;
                case TileTypes.Reward:
                    StartCoroutine(MysteryBox());
                    break;
                case TileTypes.Banana:
                    yield return StartCoroutine(BananaSlip());
                    break;
                case TileTypes.Goal:
                    SceneManager.LoadScene(3);
                    break;
            }
        }

        //yield return StartCoroutine(CardFight(tile));
    }

    IEnumerator ShowDialogue(string text)
    {
        dialogueText.transform.parent.gameObject.SetActive(true);
        dialogueText.text = text;
        yield return new WaitForSeconds(2);
        dialogueText.transform.parent.gameObject.SetActive(false);
    }

    IEnumerator BananaSlip()
    {
        AudioManager.PlayClip(AudioManager.BananaSlipClip);
        int targetPosition = characterPosition - bananaGoBackSpaces;
        float finalAngle = 0;
        for (; characterPosition > targetPosition; characterPosition--)
        {
            Vector3 startPos = grid.GetChild(characterPosition).position;
            Vector3 endPos = grid.GetChild(characterPosition - 1).position;
            float f = 0;
            float delta = (endPos - startPos).x;
            if (delta > 0)
            {
                finalAngle = -360;
            }
            else if (delta < 0)
            {
                finalAngle = 360;
            }
            while (f < 1)
            {
                f += Time.deltaTime / (bananaSlipDuration / bananaGoBackSpaces);
                characterDisplay.position = Vector3.Lerp(startPos, endPos, f);
                characterDisplay.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, finalAngle, f));
                yield return null;
            }

            f = 1;
            characterDisplay.position = Vector3.Lerp(startPos, endPos, f);
            characterDisplay.rotation = Quaternion.identity;
        }

        yield return StartCoroutine(CheckCurrentTile());
    }
    public void Heal(int amount)
    {
        AudioManager.PlayClip(AudioManager.HeadChompClip);
        healDisplay.SetActive(true);
        CurrentHP = Mathf.Min(CurrentHP + amount, maxHP);
        UpdateHPDisplay();
    }

    public void Damage(int damage)
    {
        AudioManager.PlayClip(AudioManager.OuchClip);
        damageDisplay.SetActive(true);
        CurrentHP -= damage;
        UpdateHPDisplay();
    }

    public void UpdateHPDisplay()
    {
        hpFillImage.fillAmount = (float)CurrentHP / maxHP;
        hpTextDisplay.text = $"{CurrentHP}/{maxHP}";

        int i = 0;
        for (; i < currentHearts; i++)
        {
            Transform t = i < heartsHolder.childCount ? heartsHolder.GetChild(i) : Instantiate(heartsHolder.GetChild(0), heartsHolder);
            t.gameObject.SetActive(true);
        }

        for (; i < heartsHolder.childCount; i++)
        {
            heartsHolder.GetChild(i).gameObject.SetActive(false);
        }
    }

    public bool IsPlayerTurn { get; private set; }
    List<SnakeData> encounteredSnakes = new List<SnakeData>();
    CardDisplay selectedCard;
    IEnumerator CardFight(TileScript tile)
    {
        countDownTimerFill.transform.parent.gameObject.SetActive(false);
        SnakeData snake = tile.snake;
        vsScreenSnakePotrait.sprite = snake.snakeNormal;
        vsScreenSnakeSnakeName.text = snake.snakeName;
        playerDisplayImage.sprite = playerNormal;
        snakeDisplayImage.sprite = snake.snakeNormal;
        ToggleBoard(false);
        AudioManager.FadeOutClip(AudioManager.BoardBGMClip, 0.5f);
        vsScreen.SetActive(true);
        yield return new WaitWhile(() => vsScreen.activeSelf);
        ToggleCardGame(true);
        AudioManager.FadeInClip(AudioManager.BattleBGMClip, 0.5f);
        int snakeHP = snake.maxHP;
        UpdateSnakeHPDisplay(snakeHP, snake.maxHP);
        UpdateCardPlayerHPDisplay();
        if (encounteredSnakes.Contains(snake))
        {
            StartCoroutine(ShowDialogue(snake.reEncounterDialogue.GetRandom()));
        }
        else
        {
            StartCoroutine(ShowDialogue(snake.encounterDialogue.GetRandom()));
            encounteredSnakes.Add(snake);
        }
        System.Random rng = new System.Random();
        Queue<Card> playerCardQueue = new Queue<Card>(collectedCards.OrderBy((c) => rng.Next()));
        Queue<Card> enemyCardQueue = new Queue<Card>(snake.snakeCards.OrderBy((c) => rng.Next()));

        List<CardDisplay> playerCards = new List<CardDisplay>();
        List<CardDisplay> enemyCards = new List<CardDisplay>();

        SetUpDeck(enemyCardQueue, enemyCardDisplay, enemyCards);
        SetUpDeck(playerCardQueue, playerCardDisplay, playerCards);

        foreach (var display in playerCards)
        {
            StartCoroutine(display.FlipCard(cardFlipDuration, true));
            display.CanInteract = true;
        }
        foreach (var display in enemyCards)
        {
            display.IsCardActive = true;
            display.CanInteract = false;
        }
        yield return new WaitForSeconds(cardFlipDuration);

        while (snakeHP > 0 && CurrentHP > 0)
        {
            //Player turn
            IsPlayerTurn = true;
            foreach (CardDisplay card in playerCards)
            {
                card.IsCardActive = !IsCounterAction(card.Card.cardType);
            }
            selectedCard = null;
            yield return new WaitUntil(() => selectedCard != null);

            foreach (CardDisplay card in playerCards)
            {
                card.IsCardActive = card == selectedCard;
            }

            yield return new WaitForSeconds(1);

            CardDisplay enemyCard = null;
            for (int i = 0; i < enemyCards.Count; i++)
            {
                if (IsCounterAction(enemyCards[i].Card.cardType))
                {
                    if (Random.Range(0, 100) < 50)
                    {
                        enemyCard = enemyCards[i];
                        yield return StartCoroutine(enemyCards[i].FlipCard(cardFlipDuration, true));
                        break;
                    }
                }
            }


            if (enemyCard == null)
            {
                switch (selectedCard.Card.cardType)
                {
                    case CardTypesEnum.Damage:
                        StartCoroutine(FlashProfile(snakeDisplayImage, snake.snakeDamage));
                        snakeHP -= selectedCard.Card.value;
                        break;
                    case CardTypesEnum.Heal:
                        Heal(selectedCard.Card.value);
                        break;
                    case CardTypesEnum.InstantDeath:
                        StartCoroutine(FlashProfile(snakeDisplayImage, snake.snakeDamage));
                        snakeHP -= snake.maxHP;
                        break;
                    case CardTypesEnum.Reveal:
                        foreach (var card in enemyCards)
                        {
                            StartCoroutine(card.FlipCard(cardFlipDuration, true));
                        }
                        yield return new WaitForSeconds(cardFlipDuration);
                        yield return StartCoroutine(ShowFillTimer(fillTimer, null));
                        foreach (var card in enemyCards)
                        {
                            StartCoroutine(card.FlipCard(cardFlipDuration, false));
                        }
                        yield return new WaitForSeconds(cardFlipDuration);
                        break;
                }
            }
            else
            {
                switch (enemyCard.Card.cardType)
                {
                    case CardTypesEnum.Reflect:
                        switch (selectedCard.Card.cardType)
                        {
                            case CardTypesEnum.Damage:
                                StartCoroutine(FlashProfile(playerDisplayImage, playerDamage));
                                Damage(selectedCard.Card.value);
                                break;
                            case CardTypesEnum.Heal:
                                snakeHP = Mathf.Min(snakeHP + selectedCard.Card.value, snake.maxHP);
                                break;
                            case CardTypesEnum.InstantDeath:
                                StartCoroutine(FlashProfile(playerDisplayImage, playerDamage));
                                Damage(maxHP);
                                break;
                            case CardTypesEnum.Reveal:
                                foreach (var card in enemyCards)
                                {
                                    StartCoroutine(card.FlipCard(cardFlipDuration, false));
                                }
                                yield return new WaitForSeconds(cardFlipDuration);
                                yield return StartCoroutine(ShowFillTimer(fillTimer, null));
                                foreach (var card in enemyCards)
                                {
                                    StartCoroutine(card.FlipCard(cardFlipDuration, true));
                                }
                                yield return new WaitForSeconds(cardFlipDuration);
                                break;
                        }
                        break;
                }
            }

            UpdateCardPlayerHPDisplay();
            UpdateSnakeHPDisplay(snakeHP, snake.maxHP);

            if (CurrentHP <= 0 || snakeHP <= 0)
            {
                break;
            }

            if (Random.Range(0, 100) < 50)
            {
                StartCoroutine(ShowDialogue(snake.battleDialogue.GetRandom()));
            }

            yield return new WaitForSeconds(0.5f);
            StartCoroutine(selectedCard.FlipCard(cardFlipDuration, false));
            if (enemyCard != null)
            {
                StartCoroutine(enemyCard.FlipCard(cardFlipDuration, false));
            }
            yield return new WaitForSeconds(cardFlipDuration);

            if (enemyCard != null)
            {
                enemyCardQueue.Enqueue(enemyCard.Card);
                enemyCard.Initialize(enemyCardQueue.Dequeue());
            }

            playerCardQueue.Enqueue(selectedCard.Card);
            selectedCard.Initialize(playerCardQueue.Dequeue());

            yield return new WaitForSeconds(1);
            foreach (CardDisplay card in playerCards)
            {
                card.IsCardActive = false;
            }
            StartCoroutine(selectedCard.FlipCard(cardFlipDuration, true));
            yield return new WaitForSeconds(cardFlipDuration);

            yield return new WaitForSeconds(1);

            //Snakes turn
            enemyCard = enemyCards.Where(e => !IsCounterAction(e.Card.cardType)).OrderBy(e => rng.Next()).FirstOrDefault();
            if (enemyCard != null)
            {
                yield return StartCoroutine(enemyCard.FlipCard(cardFlipDuration, true));
                yield return new WaitForSeconds(0.5f);
                int count = 0;
                foreach (CardDisplay card in playerCards)
                {
                    card.IsCardActive = IsCounterAction(card.Card.cardType);
                    if (card.IsCardActive) count++;
                }

                selectedCard = null;
                if (count > 0)
                {
                    yield return StartCoroutine(ShowFillTimer(fillTimer, () => selectedCard != null));
                }
            }

            foreach (CardDisplay card in playerCards)
            {
                card.IsCardActive = card == selectedCard;
            }

            if (selectedCard == null)
            {
                switch (enemyCard.Card.cardType)
                {
                    case CardTypesEnum.Damage:
                        StartCoroutine(FlashProfile(playerDisplayImage, playerDamage));
                        Damage(enemyCard.Card.value);
                        break;
                    case CardTypesEnum.Heal:
                        snakeHP = Mathf.Min(snakeHP + enemyCard.Card.value, snake.maxHP);
                        break;
                    case CardTypesEnum.InstantDeath:
                        StartCoroutine(FlashProfile(playerDisplayImage, playerDamage));
                        Damage(maxHP);
                        break;
                    case CardTypesEnum.Reveal:
                        foreach (var card in enemyCards)
                        {
                            StartCoroutine(card.FlipCard(cardFlipDuration, false));
                        }
                        yield return new WaitForSeconds(cardFlipDuration);
                        yield return StartCoroutine(ShowFillTimer(fillTimer, null));
                        foreach (var card in enemyCards)
                        {
                            StartCoroutine(card.FlipCard(cardFlipDuration, true));
                        }
                        yield return new WaitForSeconds(cardFlipDuration);
                        break;
                }
            }
            else
            {
                switch (selectedCard.Card.cardType)
                {
                    case CardTypesEnum.Reflect:
                        switch (enemyCard.Card.cardType)
                        {
                            case CardTypesEnum.Damage:
                                StartCoroutine(FlashProfile(snakeDisplayImage, snake.snakeDamage));
                                snakeHP -= enemyCard.Card.value;
                                break;
                            case CardTypesEnum.Heal:
                                Heal(enemyCard.Card.value);
                                break;
                            case CardTypesEnum.InstantDeath:
                                StartCoroutine(FlashProfile(snakeDisplayImage, snake.snakeDamage));
                                snakeHP -= snake.maxHP;
                                break;
                            case CardTypesEnum.Reveal:
                                foreach (var card in enemyCards)
                                {
                                    StartCoroutine(card.FlipCard(cardFlipDuration, true));
                                }
                                yield return new WaitForSeconds(cardFlipDuration);
                                yield return StartCoroutine(ShowFillTimer(fillTimer, null));
                                foreach (var card in enemyCards)
                                {
                                    StartCoroutine(card.FlipCard(cardFlipDuration, false));
                                }
                                yield return new WaitForSeconds(cardFlipDuration);
                                break;
                        }
                        break;
                }
            }

            UpdateCardPlayerHPDisplay();
            UpdateSnakeHPDisplay(snakeHP, snake.maxHP);

            if (CurrentHP <= 0 || snakeHP <= 0)
            {
                break;
            }

            if (Random.Range(0, 100) < 50)
            {
                StartCoroutine(ShowDialogue(snake.battleDialogue.GetRandom()));
            }
            //yield return new WaitForSeconds(2);
            if (selectedCard != null)
            {
                StartCoroutine(selectedCard.FlipCard(cardFlipDuration, false));
            }

            if (enemyCard != null)
            {
                StartCoroutine(enemyCard.FlipCard(cardFlipDuration, false));
            }
            yield return new WaitForSeconds(cardFlipDuration);

            if (enemyCard != null)
            {
                enemyCardQueue.Enqueue(enemyCard.Card);
                enemyCard.Initialize(enemyCardQueue.Dequeue());
            }
            if (selectedCard != null)
            {
                playerCardQueue.Enqueue(selectedCard.Card);
                selectedCard.Initialize(playerCardQueue.Dequeue());
            }

            yield return new WaitForSeconds(1);

            if (selectedCard != null)
            {
                StartCoroutine(selectedCard.FlipCard(cardFlipDuration, true));
                yield return new WaitForSeconds(cardFlipDuration);
            }

            yield return new WaitForSeconds(1);
        }

        AudioManager.FadeInClip(AudioManager.BoardBGMClip, 0.5f);
        AudioManager.FadeOutClip(AudioManager.BattleBGMClip, 0.5f);

        if (snakeHP <= 0)
        {
            winScreen.SetActive(true);
            ToggleCardGame(false);
            loserProfile.sprite = snake.snakeDead;
            snakeLoserText.text = snake.deathDialogue.GetRandom();
            yield return new WaitWhile(() => winScreen.activeSelf);
            ToggleBoard(true);

            mysteryBoxPanel.SetActive(true);
            string text = string.Empty;


            Card card = allCards[Random.Range(0, allCards.Length)];
            collectedCards.Add(card);
            //New card
            text = $"The Card {card.title}!";

            mysteryBoxRewardText.text = text;
            yield return new WaitWhile(() => mysteryBoxPanel.activeSelf);
        }
        else
        {
            currentHearts--;
            if (currentHearts <= 0)
            {
                SceneManager.LoadScene(2);
            }
            else
            {
                snakeWinnerText.text = snake.winDialogue.GetRandom();
                CurrentHP = maxHP;
                UpdateHPDisplay();
                loseScreen.SetActive(true);
                ToggleCardGame(false);
                yield return new WaitWhile(() => loseScreen.activeSelf);
                ToggleBoard(true);

                int targetPosition = snake.snakeEndPosition;

                Vector3 startPos = grid.GetChild(characterPosition).position;
                Vector3 endPos = grid.GetChild(targetPosition).position;

                float f = 0;
                while (f < 1)
                {
                    f += Time.deltaTime / snakeTraversalDuration;
                    characterDisplay.position = Vector3.Lerp(startPos, endPos, f);
                    yield return null;
                }

                f = 1;
                characterDisplay.position = Vector3.Lerp(startPos, endPos, f);
                characterPosition = targetPosition;
                yield return StartCoroutine(CheckCurrentTile());
            }
        }
    }

    IEnumerator ShowFillTimer(float maxTime, System.Func<bool> extraCondition)
    {
        float timeStarted = Time.time;
        countDownTimerFill.transform.parent.gameObject.SetActive(true);
        yield return new WaitUntil(() =>
        {
            float timeRemaining = timeStarted + maxTime - Time.time;
            UpdateFillTimer(timeRemaining, maxTime);
            if (extraCondition == null)
            {
                return timeRemaining < 0;
            }
            return extraCondition() || timeRemaining < 0;
        });
        countDownTimerFill.transform.parent.gameObject.SetActive(false);
    }

    IEnumerator FlashProfile(Image source, Sprite changeTo)
    {
        Sprite current = source.sprite;
        source.sprite = changeTo;
        yield return new WaitForSeconds(1);
        source.sprite = current;
    }

    void UpdateFillTimer(float timeRemaining, float maxTime)
    {
        countDownTimerFill.fillAmount = timeRemaining / maxTime;
        countDownTimerText.text = $"{Mathf.CeilToInt(timeRemaining)}";
    }

    void SetUpDeck(Queue<Card> queue, Transform holder, List<CardDisplay> displayList)
    {
        for (int i = 0; i < holder.childCount; i++)
        {
            var display = holder.GetChild(i).GetComponent<CardDisplay>();
            if (display != null)
            {
                displayList.Add(display);
                display.Initialize(queue.Dequeue());
                display.ShowBack(true);
            }
        }
    }
    bool IsCounterAction(CardTypesEnum cardType) => cardType switch
    {
        CardTypesEnum.Block => true,
        CardTypesEnum.Reflect => true,
        _ => false
    };

    public void PlayerSelectedCard(CardDisplay cardDisplay)
    {
        AudioManager.PlayClip(AudioManager.ButtonClickClip);
        selectedCard = cardDisplay;
    }

    void UpdateSnakeHPDisplay(int snakeHP, int snakeMaxHP)
    {
        snakeHPText.text = $"{snakeHP} / {snakeMaxHP}";
        snakeHPFillImage.fillAmount = (float)snakeHP / snakeMaxHP;
    }
    void UpdateCardPlayerHPDisplay()
    {
        playerHPText.text = $"{CurrentHP} / {maxHP}";
        playerHPFillImage.fillAmount = (float)CurrentHP / maxHP;
    }
    void ToggleBoard(bool toggle)
    {
        foreach (var g in boardDisplay)
        {
            g.SetActive(toggle);
        }
    }

    void ToggleCardGame(bool toggle)
    {
        foreach (var g in cardGameDisplay)
        {
            g.SetActive(toggle);
        }
    }

    IEnumerator MysteryBox()
    {
        mysteryBoxPanel.SetActive(true);
        int random = Random.Range(0, 100);
        string text = string.Empty;

        if (random < 25)
        {
            //Full heal
            text = "A Full Heal!";
        }
        else if (random < 75)
        {
            Card card = allCards[Random.Range(0, allCards.Length)];
            collectedCards.Add(card);
            //New card
            text = $"The Card {card.title}!";
        }
        else if (random < 80)
        {
            //Extra heart
            text = "Another Heart!";
            currentHearts++;
        }
        else
        {
            //Deal damage
            text = "-10 Health!";
        }

        mysteryBoxRewardText.text = text;
        yield return new WaitWhile(() => mysteryBoxPanel.activeSelf);
        if (random < 25)
        {
            //Full heal
            Heal(maxHP);
        }
        else if (random < 75)
        {
            //New card

        }
        else if (random < 80)
        {
            //Extra heart
            UpdateHPDisplay();
        }
        else
        {
            //Deal damage
            Damage(10);
        }
    }
}

public static class Extensions
{
    public static T GetRandom<T>(this T[] array)
    {
        if (array == null) return default;
        if (array.Length == 0) return default;
        return array[Random.Range(0, array.Length)];
    }
}