using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using KModkit;

public class LEDMathScript : MonoBehaviour
{
    #region Buttons and Numbers

    public KMBombInfo Bomb;
    public KMBombModule BombModule;
    public KMAudio Audio;

    public KMSelectable ClearButton;
    public KMSelectable SubmitButton;
    public KMSelectable P1Button;
    public KMSelectable P10Button;
    public KMSelectable P100Button;
    public KMSelectable S1Button;
    public KMSelectable S10Button;
    public KMSelectable S100Button;

    public Material[] ledOptions;

    public Renderer ledA;
    public Renderer ledB;
    public Renderer ledOp;

    static int moduleIdCounter = 1;

    int moduleId;

    private bool moduleSolved;

    private int ledAIndex = 0;
    private int ledBIndex = 0;
    private int ledOpIndex = 0;

    private int ledANumber = 0;
    private int ledBNumber = 0;

    int AnswerNumber = 0;

    int EquationAnswer = 0;

    public TextMesh AnswerInputDisplay;
    #endregion

    void Awake()
    {
        moduleId = moduleIdCounter++;
    }

    protected void Start()
    {
        string SubmitDisplayNumber = Convert.ToString(AnswerNumber);
        AnswerInputDisplay.text = SubmitDisplayNumber;

        ClearButton.OnInteract += delegate () { ClearAnswer(); return false; };
        SubmitButton.OnInteract += delegate () { SubmitAttempt(); return false; };
        P1Button.OnInteract += delegate () { Plus1(); return false; };
        P10Button.OnInteract += delegate () { Plus10(); return false; };
        P100Button.OnInteract += delegate () { Plus100(); return false; };
        S1Button.OnInteract += delegate () { Subtract1(); return false; };
        S10Button.OnInteract += delegate () { Subtract10(); return false; };
        S100Button.OnInteract += delegate () { Subtract100(); return false; };

        CalculateAnswer();
    }
    #region LED Calculation
    void CalculateAnswer()
    {
        //ledA
        ledAIndex = UnityEngine.Random.Range(0, 4);
        ledA.material = ledOptions[ledAIndex];

        //ledB
        ledBIndex = UnityEngine.Random.Range(0, 4);
        ledB.material = ledOptions[ledBIndex];

        //ledOp
        ledOpIndex = UnityEngine.Random.Range(0, 4);
        ledOp.material = ledOptions[ledOpIndex];

        //LED A
        //If A is red
        if(ledAIndex == 0)
        {
            ledANumber = (Bomb.GetBatteryCount() + Bomb.GetIndicators().Count()) * 2;
        }
        //If A is blue
        else if (ledAIndex == 1)
        {
            ledANumber = (Bomb.GetSerialNumberNumbers().Last() * 3) + Bomb.GetIndicators().Count();
        }
        //If A is green
        else if (ledAIndex == 2)
        {
            ledANumber = Bomb.GetBatteryCount() - Bomb.GetSerialNumberNumbers().Last() - 7;
        }
        //If A is yellow
        else
        {
            ledANumber = (Bomb.GetBatteryCount() * Bomb.GetBatteryHolderCount()) + 4;
        }
        Debug.LogFormat("[LED Math #{0}] Value of LED 'A' is {1}.", moduleId, ledANumber);

        //LED B
        //If B has the same colour as A
        if(ledBIndex == ledAIndex)
        {
            ledBNumber = (8 - Bomb.GetBatteryHolderCount()) + Bomb.GetBatteryCount();
        }
        //If B has the same colour as the operator
        else if(ledBIndex == ledOpIndex)
        {
            ledBNumber = Bomb.GetIndicators().Count() + Bomb.GetBatteryHolderCount() + 1;
        }
        //If B is blue or yellow
        else if(ledBIndex == 1 || ledBIndex == 3)
        {
            ledBNumber = (Bomb.GetSerialNumberNumbers().Last() + Bomb.GetBatteryHolderCount()) * 5;
        }
        //If B is red or green
        else
        {
            ledBNumber = (Bomb.GetSerialNumberNumbers().Last() - Bomb.GetBatteryCount()) * 6;
        }
        Debug.LogFormat("[LED Math #{0}] Value of LED 'B' is {1}.", moduleId, ledBNumber);

        //CALCULATING LED A AND B (the operator)
        //If the operator is red
        if(ledOpIndex == 0)
        {
            EquationAnswer = ledANumber + ledBNumber;
            if(EquationAnswer >= 1000)
            {

            }
            Debug.LogFormat("[LED Math #{0}] The operator is +.", moduleId);
            Debug.LogFormat("[LED Math #{0}] The correct answer is {1} ({2} + {3}).", moduleId, EquationAnswer, ledANumber, ledBNumber);
        }
        //If the operator is blue
        else if(ledOpIndex == 1)
        {
            EquationAnswer = ledANumber - ledBNumber;
            Debug.LogFormat("[LED Math #{0}] The operator is -.", moduleId);
            Debug.LogFormat("[LED Math #{0}] The correct answer is {1} ({2} - {3}).", moduleId, EquationAnswer, ledANumber, ledBNumber);
        }
        //If the operator is green or yellow
        else
        {
            EquationAnswer = ledANumber * ledBNumber;
            Debug.LogFormat("[LED Math #{0}] The operator is ×.", moduleId);
            Debug.LogFormat("[LED Math #{0}] The correct answer is {1} ({2} × {3}).", moduleId, EquationAnswer, ledANumber, ledBNumber);
        }
    }
    #endregion
    #region Button Mechanics
    void ClearAnswer()
    {
        if (!moduleSolved)
        {
            AnswerNumber = 0;
            string SubmitDisplayNumber = Convert.ToString(AnswerNumber);
            AnswerInputDisplay.text = SubmitDisplayNumber;
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            ClearButton.AddInteractionPunch();
        }
    }

    void SubmitAttempt()
    {
        if (!moduleSolved)
        {
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            SubmitButton.AddInteractionPunch();
            if (EquationAnswer == AnswerNumber)
            {
                moduleSolved = true;
                Debug.LogFormat("[LED Math #{0}] You submitted {1}. Expected {2}. Correct! Module solved!", moduleId, AnswerNumber, EquationAnswer);
                GetComponent<KMBombModule>().HandlePass();
                ledA.material = ledOptions[4];
                ledB.material = ledOptions[4];
                ledOp.material = ledOptions[4];
                AnswerInputDisplay.text = ":)";
                Audio.PlaySoundAtTransform("solvesound", transform);
            }

            else
            {
                Debug.LogFormat("[LED Math #{0}] You submitted {1}. Expected {2}. Incorrect! Strike!", moduleId, AnswerNumber, EquationAnswer);
                GetComponent<KMBombModule>().HandleStrike();
                AnswerInputDisplay.text = ":(";
            }
        }
    }

    void Plus1()
    {
        if (!moduleSolved)
        {
            AnswerNumber = AnswerNumber + 1;
            string SubmitDisplayNumber = Convert.ToString(AnswerNumber);
            AnswerInputDisplay.text = SubmitDisplayNumber;
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            P1Button.AddInteractionPunch();
        }
    }

    void Plus10()
    {
        if (!moduleSolved)
        {
            AnswerNumber = AnswerNumber + 10;
            string SubmitDisplayNumber = Convert.ToString(AnswerNumber);
            AnswerInputDisplay.text = SubmitDisplayNumber;
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            P10Button.AddInteractionPunch();
        }
    }

    void Plus100()
    {
        if (!moduleSolved)
        {
            AnswerNumber = AnswerNumber + 100;
            string SubmitDisplayNumber = Convert.ToString(AnswerNumber);
            AnswerInputDisplay.text = SubmitDisplayNumber;
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            P100Button.AddInteractionPunch();
        }
    }

    void Subtract1()
    {
        if (!moduleSolved)
        {
            AnswerNumber = AnswerNumber - 1;
            string SubmitDisplayNumber = Convert.ToString(AnswerNumber);
            AnswerInputDisplay.text = SubmitDisplayNumber;
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            S1Button.AddInteractionPunch();
        }
    }

    void Subtract10()
    {
        if (!moduleSolved)
        {
            AnswerNumber = AnswerNumber - 10;
            string SubmitDisplayNumber = Convert.ToString(AnswerNumber);
            AnswerInputDisplay.text = SubmitDisplayNumber;
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            S10Button.AddInteractionPunch();
        }
    }

    void Subtract100()
    {
        if (!moduleSolved)
        {
            AnswerNumber = AnswerNumber - 100;
            string SubmitDisplayNumber = Convert.ToString(AnswerNumber);
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            S100Button.AddInteractionPunch();
            AnswerInputDisplay.text = SubmitDisplayNumber;
        }
    }
#endregion
}
