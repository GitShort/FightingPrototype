using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class Coins
    {
        public int loseRewardMin;
        public int loseRewardMax;

        public int winRewardMin;
        public int winRewardMax;
    }

    [Header("Balancing")]
    [SerializeField] int _coins = 0;
    [SerializeField] int currentLevel = 0;
    [SerializeField] float _fightDuration = 30f;
    [SerializeField] int[] SwordPrice;
    [SerializeField] Coins[] _CoinsReward;

    [Header("General")]
    [SerializeField] Animator _playerAnim;
    [SerializeField] Animator _enemyAnim;

    [SerializeField] GameObject _menuGO;
    [SerializeField] GameObject _winGO;
    [SerializeField] GameObject _fightTimerGO;
    [SerializeField] TextMeshProUGUI _swordPriceText;
    [SerializeField] TextMeshProUGUI _fightTimerText;
    [SerializeField] TextMeshProUGUI _coinsText;
    [SerializeField] TextMeshProUGUI _swordText;
    [SerializeField] TextMeshProUGUI _levelText;
    [SerializeField] TextMeshProUGUI _bossWeaponText;
    [SerializeField] TextMeshProUGUI _gainedCoinsText;
    [SerializeField] GameObject _gainedCoinsGO;
    [SerializeField] TextMeshProUGUI _messageText;
    [SerializeField] string loseMessage = "You lost!";
    [SerializeField] string winMessage = "You won!";
    [SerializeField] string bossWinMessage = "You defeated BOSS";
    [SerializeField] TextMeshProUGUI _fightTypeText;
    [SerializeField] TextMeshProUGUI _fightsWonText;
    [SerializeField] TextMeshProUGUI _fightsLostText;
    [SerializeField] ParticleSystem _fightSmokeVFX;
    [SerializeField] TextMeshProUGUI _SpeedUpText;

    int _winCount;
    int _lossCount;
    bool _fightStarted = false;
    float t;
    string currentEnemy;
    int _coinsReward;
    int _currentSword;



    void Start()
    {
        t = _fightDuration;
        _menuGO.SetActive(true);
        _fightTimerGO.SetActive(false);
        UpdateUI();
    }

    void Update()
    {
        if (_fightStarted)
        {
            if (!_fightTimerGO.activeInHierarchy)
                _fightTimerGO.SetActive(true);

            t -= Time.deltaTime;
            _fightTimerText.text = Mathf.RoundToInt(t+1).ToString();

            if (t <= 0)
            {
                FightEnd(currentEnemy);
                t = _fightDuration;
                _fightStarted = false;
                _fightTimerGO.SetActive(false);
            }    
        }
    }

    void UpdateUI()
    {
        _coinsText.text = _coins.ToString();
        _swordText.text = _currentSword.ToString();
        _levelText.text = "LEVEL " + (currentLevel +1).ToString();
        _bossWeaponText.text = "Holds sword " + (currentLevel + 1).ToString();
        _fightsWonText.text = "W: " + _winCount;
        _fightsLostText.text = "L: " + _lossCount; 
        if (_currentSword < 11)
            _swordPriceText.text = "Sword " + (_currentSword + 1) + " costs " + SwordPrice[_currentSword] + " coins!";
        else
            _swordPriceText.text = "Sold out!";
    }

    public void PurchaseSword()
    {
        if (_currentSword < 11)
        {
            if (_coins >= SwordPrice[_currentSword])
            {
                _coins -= SwordPrice[_currentSword];
                _currentSword++;
            }
            UpdateUI();
        }

    }

    IEnumerator FightResult(string message, bool giveReward)
    {
        if (currentLevel < 10)
        {
            if (message.Equals(loseMessage))
            {
                if (giveReward)
                    _lossCount++;
                Result(message, giveReward, _CoinsReward[currentLevel].loseRewardMin, _CoinsReward[currentLevel].loseRewardMax);
            }
            else if (message.Equals(winMessage))
            {
                _winCount++;
                Result(message, giveReward, _CoinsReward[currentLevel].winRewardMin, _CoinsReward[currentLevel].winRewardMax);
            }
            else if (message.Equals(bossWinMessage))
            {
                _messageText.text = message + " " + (currentLevel).ToString() + "!";
            }

            yield return new WaitForSeconds(1f);
            _messageText.text = "";
            _gainedCoinsGO.SetActive(false);

            _menuGO.SetActive(true);
        }
        else
        {
            _messageText.text = "Bosses defeated!";
            _winGO.SetActive(true);
        }
        _fightTypeText.gameObject.SetActive(false);
        UpdateUI();
    }

    private void Result(string message, bool giveReward, int coinsRewardMin, int coinsRewardMax)
    {
        _messageText.text = message;
        if (giveReward)
        {
            _coinsReward = Random.Range(coinsRewardMin, coinsRewardMax);
            _coins += _coinsReward;
            _gainedCoinsGO.SetActive(true);
            _gainedCoinsText.text = _coinsReward.ToString();
        }

    }

    public void StartFight(string enemyType)
    {
        _fightSmokeVFX.Play();
        _playerAnim.SetBool("isFighting", true);
        _enemyAnim.SetBool("isFighting", true);
        _menuGO.SetActive(false);
        _fightTypeText.gameObject.SetActive(true);
        _fightStarted = true;
        currentEnemy = enemyType;
        _fightTypeText.text = enemyType.ToUpper() + " battle";
    }

    void FightEnd(string enemyType)
    {
        if (enemyType.Equals("Regular"))
        {
            if (_currentSword > 0)
            {
                int chanceToWin = Random.Range(0, 100);
                if (chanceToWin > 30)
                {
                    StartCoroutine(FightResult(winMessage, true));
                }
                else
                {
                    StartCoroutine(FightResult(loseMessage, true));
                }
            }
            else
            {
                StartCoroutine(FightResult(loseMessage, true));
            }
        }
        else if (enemyType.Equals("Boss"))
        {
            if (_currentSword > currentLevel + 1)
            {
                currentLevel++;
                StartCoroutine(FightResult(bossWinMessage, false));
            }
            else
            {
                StartCoroutine(FightResult(loseMessage, false));
            }
        }
        _playerAnim.SetBool("isFighting", false);
        _enemyAnim.SetBool("isFighting", false);
        _fightSmokeVFX.Stop();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    public void SpeedUp()
    {
        if (_fightDuration > 0)
        {
            _fightDuration = 0;
            _SpeedUpText.gameObject.SetActive(true);
            t = _fightDuration;
        }    
        else
        {
            _SpeedUpText.gameObject.SetActive(false);
            _fightDuration = 30;
            t = _fightDuration;
        }
    }

    public void ResetScore()
    {
        _winCount = 0;
        _lossCount = 0;
        UpdateUI();
    }
}
