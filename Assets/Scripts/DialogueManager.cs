using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using Febucci.UI;
using JSAM;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Linq;
using Sirenix.OdinInspector;
using System.Text.RegularExpressions;
using System;
using UnityEngine.UI;
using Animator = UnityEngine.Animator;
using Image = UnityEngine.UI.Image;
using UnityEngine.Pool;
using jcdr;
using System.Globalization;

// Took straight from Occult a la Carte
// Even with edits, it may be quite jank
// edit: its 2025 now, ive taken this straight from the game that i put it into from occult a la carte lmao

public enum DialogueType
{
    Speaker,
    Statement,
    //Title,
    MonologueStart,
    MonologueEnd,
    Transition,
    Level,
    Music,
    Sound,
    Character,
    Pause,
    DefinitionStart,
    DefinitionEnd,
    Macro,
    StopMusic,
    StopSounds,
    ChangeSpeakerIndicator,
    HideTextbox,
    ShowTextbox,
    ShowProp,
    HideProp,
    HideAllProps,
    DisableSelf,
    DisableUntilNextTurn,
    FakeSceneChange,
    TitleScreen
}

public struct BoolString
{
    public bool update;
    public string value;

    public BoolString(bool update, string value)
    {
        this.update = update;
        this.value = value;
    }
}

public struct DialogueToken
{
    public string value;
    public int index;
    public DialogueType type;
    public int offset; // from index

    public DialogueToken(string value, int index, DialogueType type, int offset = 0)
    {
        this.value = value;
        this.index = index;
        this.type = type;
        this.offset = offset;
    }

    public DialogueToken ShallowCopy()
    {
        DialogueToken copy = new DialogueToken();
        copy.value = value;
        copy.index = index;
        copy.type = type;
        copy.offset = offset;
        return copy;
    }
}
public class DialogueManager : SerializedMonoBehaviour
{
    public List<PuppetData> characters = new List<PuppetData>();

    public List<PuppetData> onstageCharacters = new List<PuppetData>();


    private static Dictionary<string, DialogueType> keywords = new Dictionary<string, DialogueType>()
    {
        // http://regexstorm.net/tester
        {@"(.+:)", DialogueType.Speaker },
        {@"(.+:)(.|\n\S)+", DialogueType.Statement },
        //{@"<title>.+</title>", DialogueType.Title },
        {@"<monologue>(.+:)(.|\n\S)+", DialogueType.MonologueStart }, // what the fuck does a monologue do - 2025
        {@"(.+:)(.|\n\S)+</monologue>", DialogueType.MonologueEnd },
        {@"(\nEND SCENE).*$", DialogueType.Transition },
        {@"<level>.+</level>", DialogueType.Level },
        {@"<music>.+</music>", DialogueType.Music },
        {@"<sound>.+</sound>", DialogueType.Sound },
        {@"<character>.+</character>", DialogueType.Character },
        {@"<pause>.+</pause>", DialogueType.Pause },
        {@"<define.+>.+", DialogueType.DefinitionStart },
        {@"</define>", DialogueType.DefinitionEnd },
        {@"<run .+>", DialogueType.Macro },
        {@"<music stop>", DialogueType.StopMusic },
        {@"<sound stop>", DialogueType.StopSounds },
        {@"<indicator .+>", DialogueType.ChangeSpeakerIndicator },
        {@"<hide textbox>", DialogueType.HideTextbox },
        {@"<show textbox>", DialogueType.ShowTextbox },
        {@"<show prop .+>", DialogueType.ShowProp },
        {@"<hide prop .+>", DialogueType.HideProp },
        {@"<hide all>", DialogueType.HideAllProps },
        {@"<disable self>", DialogueType.DisableSelf },
        {@"<disable until next turn>", DialogueType.DisableUntilNextTurn },
        {@"<transition>", DialogueType.FakeSceneChange },
        {@"<title screen>", DialogueType.TitleScreen }

    }; // i aint even gonna bother with puppets imma just made it toggle on or off preset parents with the chars in em since they dont need to move - 2025

    bool dialoguePaused = false;
    int lengthOfConvo;
    string[] temp;
    string[] line = new string[2];
    [SerializeField]
    List<DialogueToken> convoData;
    public TextMeshProUGUI nameBox;
    public TextMeshProUGUI textBox;
    public CanvasGroup textBoxCanvasGroup;
    TextAnimator_TMP textBoxAnimator;
    int currentLine;
    string text;
    string tempString;
    public SoundPlayer dialogueClick;

    public List<IsTouchingMouse> blockers;

    public bool inGameplayScene = false;
    public LevelDatabase database;
    public TextAsset tutorialScript;
    public CanvasGroup dialogueCanvasGroup;

    public LevelDatabase level;

    void Start()
    {
        if (level == null) // what the fuck is going on edit: oh singletons fuck
            level = FindObjectOfType<LevelDatabase>();

        if(!level.GetCurrentLevel().notALevel)
            mainStory = level.GetCurrentLevel().winDialogue;

        if (inGameplayScene)
        {
            if (!database.GetCurrentLevel().playTutorial)
            {
                Destroy(gameObject);
                return;
            }

            mainStory = tutorialScript;

            Debug.Log(mainStory.text); // one of those "i swear to god it doesnt work without this" sigh, idk anymore
        }

        textBoxAnimator = textBox.GetComponent<TextAnimator_TMP>();

        sceneTransition = transition.GetComponent<SceneTransition>();

        //levels = GameObject.Find("LevelDatabase").GetComponent<LevelDatabase>();

        textBox.text = "";
        nameBox.text = "";

        StartConversation();
    }

    public void Toggle()
    {
        dialoguePaused = !dialoguePaused;
    }
    public void Unpause()
    {
        // hack way to do this but oh well
        Invoke("ContinueDialogue", 1f);
    }
    public void ContinueDialogue()
    {
        dialoguePaused = false;
        dialogueCanvasGroup.alpha = 1.0f;
        dialogueCanvasGroup.blocksRaycasts = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!dialoguePaused)
        {
            if (nextLineContinue > 0f)
            {
                nextLineContinue -= Time.deltaTime;
                if (nextLineContinue <= 0f)
                {
                    currentLine += 1;
                    SetDialogue(currentLine);
                }
            }

            if (blockers.All(x => !x.IsTouching))
            {
                if (Input.GetKeyDown(KeyCode.Z) || (Input.GetMouseButtonDown(0)))
                {
                    if (textBoxAnimator.allLettersShown && currentLine != -1)
                    {
                        currentLine += 1;
                        SetDialogue(currentLine);
                        dialogueClick.PlaySound();
                    }
                    else
                    {
                        textBoxAnimator.ShowAllCharacters(true);
                        dialogueClick.PlaySound();
                    }
                }
            }
            
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                skipCount += 400f * Time.deltaTime;
                if(skipCount > skipLimit)
                {
                    skipCount = skipLimit;
                    Skip();
                }
                else
                {
                    holdSkipText.text = "Hold CTRL to skip";
                }
            }
            else
            {
                holdSkipText.text = "";
            }
        }


        if (skipCount > 0)
            skipCount -= 3 * Time.deltaTime;

        holdSkipCanvasGroup.alpha = (float)skipCount / (float)skipLimit;

        //else
        //{
        //    if (Input.GetKeyDown(KeyCode.Escape))
        //    {
        //        ToggleSettings();
        //    }
        //}
    }

    float skipCount = 0;
    public int skipLimit = 90;
    void Skip()
    {
        if (textBoxAnimator.allLettersShown && currentLine != -1)
        {
            currentLine += 1;
            SetDialogue(currentLine);
            //JSAM.AudioManager.PlaySound("DramaticHit");
        }
        else
        {
            textBoxAnimator.ShowAllCharacters(true);
            //JSAM.AudioManager.PlaySound("DramaticHit");
        }

        skipCount = Mathf.RoundToInt(skipLimit*0.75f);
    }
    public TextMeshProUGUI holdSkipText;
    public CanvasGroup holdSkipCanvasGroup;
    List<DialogueToken> ParseFile(TextAsset file)
    {
        text = RemoveAllComments(file.text).Replace("\t", "");

        List<DialogueToken> tokens = Tokenize(text);

        return tokens;
    }

    string RemoveAllComments(string text)
    {
        string[] lines = text.Split('\n');
        string output = "";
        for(int i = 0; i < lines.Length; i++)
        {
            output += RemoveCommentsFromLine(lines[i]) + "\n";
        }
        return output;
    }

    string RemoveCommentsFromLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return line;
        string trimmed = line.Trim();
        if (trimmed.StartsWith("//") || trimmed.StartsWith("#"))
            return string.Empty;
        return line;
    }
    List<DialogueToken> Tokenize(string text)
    {
        var tokens = new List<DialogueToken>();

        foreach (var keyword in keywords)
        {
            foreach (Match match in Regex.Matches(text, keyword.Key, RegexOptions.Multiline))
            {
                string matchedString = match.Value;

                if (matchedString.Length > 1000)
                {
                    Debug.LogWarning("Match of " + keyword.Value + " is in excess of 1000 characters! Discarding result...");
                    continue;
                }

                if (keyword.Value == DialogueType.Statement || keyword.Value == DialogueType.MonologueStart || keyword.Value == DialogueType.MonologueEnd)
                {
                    matchedString = Regex.Replace(match.Value, @"(.+:)", "");
                }

                string[] tokenValues = matchedString.Split('\n');

                if (tokenValues.Length > 1)
                {
                    for (int i = 0; i < tokenValues.Length; i++)
                    {
                        tokens.Add(new DialogueToken(tokenValues[i], matchedString.IndexOf(tokenValues[i]) + match.Index, keyword.Value, 0)); // TODO: We really assume the text doesn't repeat ever in indexof...
                    }
                }
                else
                {
                    tokens.Add(new DialogueToken(matchedString, match.Index, keyword.Value, match.Value.Length - matchedString.Length));
                }
            }
        }

        // https://stackoverflow.com/questions/5461479/sort-list-by-field-c
        tokens.Sort((x,y) => x.index.CompareTo(y.index));

        // TODO: remove all this since we're not using offsets anymore
        for (int i = 0; i < tokens.Count; i++) {
            for (int j = 0; j < tokens.Count; j++) {
                // Check if they overlap - https://stackoverflow.com/questions/13513932/algorithm-to-detect-overlapping-periods
                if (tokens[i].type == DialogueType.Speaker && tokens[j].type == DialogueType.Statement)
                {
                    bool overlap = tokens[i].index + tokens[i].offset < (tokens[j].index + tokens[j].offset + tokens[j].value.Length) && tokens[j].index + tokens[j].offset < (tokens[i].index + tokens[i].offset + tokens[i].value.Length);
                    if (overlap)
                    {
                        Debug.LogWarning("Overlapping:\n" + tokens[i].value + "\n\n" + tokens[j].value + "\n\nAdjusting offsets accordingly...");
                        tokens[i] = new DialogueToken(tokens[i].value, tokens[i].index, tokens[i].type, tokens[j].offset);
                    }
                }
            }
        }

        tokens.RemoveAll(x => x.value.Trim() == "");

        return tokens;
    }

    [SerializeField]
    bool monologue = false;
    [SerializeField]
    bool monologueEnd = false;
    bool CompareTokens(DialogueToken x, int line)
    {
        // TODO: Change comparison based on token type
        return x.index == convoLineToIndex[line];
    }

    public Animator transition;
    SceneTransition sceneTransition;
    bool transitioned = false;
    public LevelDatabase levels;

    [ReadOnly]
    public List<Tuple<string, List<DialogueToken>>> definitions = new List<Tuple<string, List<DialogueToken>>>();
    Tuple<string, List<DialogueToken>> currentDefinition;
    [SerializeField, ReadOnly]
    bool creatingDefinition;

    [HideInInspector] public bool unpauseOnNextTurn;
    public void SetDialogue(int line, bool firstTimeLoading = false)
    {
        if (line > lengthOfConvo)
        {
            currentLine = -1;
            EndConversation();
        }
        else
        {
            BoolString newSpeaker = new BoolString(false, "");
            BoolString newStatement = new BoolString(false, "");
            bool newTitle = false;
            // We can have new statements with old speakers but never new speakers with old statements.
            //var lineData = convoData.Where(x => x.index + x.offset == convoLineToIndex[line]).ToArray();

            if (transitioned)
            {
                transition.Play("start");
                transitioned = false;
            }

            nextLineContinue = 0f;

            if (firstTimeLoading && line != 1) // loading a save
            {
                // still need to preserve history to have the correct backgrounds, etc.
                for(int i = 1; i < line; i++)
                {
                    GetLineData(i, false, true, true);
                }
            }

            GetLineData(line);

            void GetLineData(int lineNumber, bool screenUpdate = true, bool skipLevels = false, bool ignoreDelayedContinues = false)
            {
                var lineData = convoData.Where(x => CompareTokens(x, lineNumber)).ToArray();
                for (int i = 0; i < lineData.Length; i++)
                {
                    if (creatingDefinition && lineData[i].type != DialogueType.DefinitionEnd)
                    {
                        currentDefinition.Item2.Add(lineData[i]);
                        if(!ignoreDelayedContinues) DelayedContinue(-1);
                        Debug.Log(lineData[i].value + " | Macro | " + lineData[i].type.ToString());
                        continue;
                    }

                    Debug.Log(lineData[i].value + " | " + lineData[i].index + " | " + lineData[i].type.ToString());

                    switch (lineData[i].type)
                    {
                        case DialogueType.Speaker:
                            //if(screenUpdate) 
                                newSpeaker = new BoolString(true, lineData[i].value.Replace(": ", "").Replace(":", ""));
                            break;
                        case DialogueType.Statement:
                            //if (screenUpdate) 
                                newStatement = new BoolString(true, lineData[i].value.Replace(": ", "").Replace(":", ""));
                            break;
                        //case DialogueType.Title:
                        //    if (!screenUpdate) continue;

                        //    if (!ignoreDelayedContinues) DelayedContinue(0);
                        //    DisplayBackground(lineData[i].value);
                        //    newTitle = true;
                        //    break;
                        case DialogueType.MonologueStart:
                            monologue = true;
                            break;
                        case DialogueType.MonologueEnd:
                            monologueEnd = true;
                            break;
                        case DialogueType.Transition:
                            if (!screenUpdate) continue;

                            if (lineData[i].value.ToUpper().Contains("END SCENE"))
                            {
                                transition.Play("ScreenTransitionExit");
                                transitioned = true;
                                if(!ignoreDelayedContinues) DelayedContinue(1f);
                            }
                            else
                            {
                                Debug.LogError(lineData[i].value + " is not a valid transition!");
                            }
                            break;
                        case DialogueType.Level:
                            // todo: read level data

                            //PlayerPrefs.SetString("levelScene", level); // for the UI
                            //PlayerPrefs.SetString("LevelData", LevelData.ToString());
                            PlayerPrefs.SetString("CurrentLevel", lineData[0].value.Replace("<level>", "").Replace("</level>", "").Trim());
                            StartLevel();
                            break;
                        case DialogueType.Music:
                            if(!ignoreDelayedContinues) DelayedContinue(0);
                            string music = lineData[0].value.Replace("<music>", "").Replace("</music>", "").Trim();
                            JSAM.AudioManager.PlayMusic(Extensions.ReplaceWhitespace(music, "").ToEnum<LibraryMusic>());
                            break;
                        case DialogueType.Sound:
                            if (!ignoreDelayedContinues) DelayedContinue(0);
                            string sound = lineData[0].value.Replace("<sound>", "").Replace("</sound>", "").Trim();
                            JSAM.AudioManager.PlaySound(Extensions.ReplaceWhitespace(sound, "").ToEnum<LibrarySounds>());
                            break;
                        case DialogueType.Character:
                            if(!ignoreDelayedContinues) DelayedContinue(0);
                            string character = lineData[0].value.Replace("<character>", "").Replace("</character>", "").Trim();
                            UpdateStage(character);
                            break;
                        case DialogueType.Pause:
                            float pauseLength = float.Parse(lineData[0].value.Replace("<pause>", "").Replace("</pause>", "").Trim(), CultureInfo.InvariantCulture);
                            if(!ignoreDelayedContinues) DelayedContinue(pauseLength);
                            break;
                        case DialogueType.DefinitionStart:
                            string definition = lineData[0].value.Replace("<define", "").Replace(">", "").Trim();
                            creatingDefinition = true;
                            currentDefinition = Tuple.Create(definition, new List<DialogueToken>());
                            if(!ignoreDelayedContinues) DelayedContinue(0f);
                            break;
                        case DialogueType.DefinitionEnd:

                            if (currentDefinition != null)
                            {
                                if (definitions.Select(x => x.Item1).Contains(currentDefinition.Item1))
                                    definitions.Remove(definitions.Find(x => x.Item1 == currentDefinition.Item1));
                            }

                            definitions.Add(currentDefinition);
                            creatingDefinition = false;
                            currentDefinition = default;
                            if(!ignoreDelayedContinues) DelayedContinue(0f);
                            break;
                        case DialogueType.Macro:
                            // check definitions
                            string macro = lineData[i].value.Replace("<run", "").Replace(">", "").Trim();
                            Debug.Log("Running: " + macro);
                            if (definitions.Select(x => x.Item1).Contains(macro))
                            {
                                List<DialogueToken> data = definitions.Find(x => x.Item1 == macro).Item2;

                                for (int j = 0; j < data.Count; j++)
                                {
                                    var line = data[j].ShallowCopy();
                                    line.index = convoLineToIndex[lineNumber] + j;
                                    data[j] = line;
                                }

                                for (int j = 0; j < convoData.Count; j++)
                                {
                                    DialogueToken line = convoData[j];
                                    if (line.index > convoLineToIndex[lineNumber])
                                    {
                                        line.index += data.Count + convoLineToIndex[lineNumber];
                                    }
                                    convoData[j] = line;
                                }

                                List<DialogueToken> modifiedConvoData = new List<DialogueToken>(convoData);
                                modifiedConvoData.RemoveAt(lineNumber);
                                modifiedConvoData.InsertRange(lineNumber, data);
                                convoData = modifiedConvoData;

                                UpdateConvoLineToIndex();
                            }
                            else
                            {
                                Debug.LogError(macro + " does not exist!");
                            }

                            if(!ignoreDelayedContinues) DelayedContinue(0);
                            break;
                        case DialogueType.StopMusic:
                            JSAM.AudioManager.StopAllMusic();
                            if(!ignoreDelayedContinues) DelayedContinue(0);
                            break;
                        case DialogueType.StopSounds:
                            JSAM.AudioManager.StopAllSounds();
                            if (!ignoreDelayedContinues) DelayedContinue(0);
                            break;
                        case DialogueType.ChangeSpeakerIndicator:
                            ChangeSpeakerIndicator(lineData[i].value.Replace("<indicator", "").Replace(">", "").Trim());
                            if (!ignoreDelayedContinues) DelayedContinue(0);
                            break;
                        case DialogueType.HideTextbox:
                            HideTextbox();
                            if (!ignoreDelayedContinues) DelayedContinue(0);
                            break;
                        case DialogueType.ShowTextbox:
                            ShowTextbox();
                            if (!ignoreDelayedContinues) DelayedContinue(0);
                            break;
                        case DialogueType.ShowProp:
                            ShowProp(lineData[i].value.Replace("<show prop", "").Replace(">", "").Trim());
                            if (!ignoreDelayedContinues) DelayedContinue(0);
                            break;
                        case DialogueType.HideProp:
                            HideProp(lineData[i].value.Replace("<hide prop", "").Replace(">", "").Trim());
                            if (!ignoreDelayedContinues) DelayedContinue(0);
                            break;
                        case DialogueType.HideAllProps:
                            HideAllProps();
                            if (!ignoreDelayedContinues) DelayedContinue(0);
                            break;
                        case DialogueType.DisableSelf:
                            Destroy(gameObject); // todo: do this in a smarter way
                            break;
                        case DialogueType.DisableUntilNextTurn:
                            dialoguePaused = true;
                            unpauseOnNextTurn = true;
                            dialogueCanvasGroup.alpha = 0f;
                            dialogueCanvasGroup.blocksRaycasts = false;
                            if (!ignoreDelayedContinues) DelayedContinue(0);
                            break;
                        case DialogueType.FakeSceneChange:
                            transition.Play("ScreenFakeTransition");
                            dialoguePaused = true;
                            Invoke("ContinueDialogue", 3.5f); // unhardcode
                            if (!ignoreDelayedContinues) DelayedContinue(0);
                            break;
                        case DialogueType.TitleScreen:
                            sceneTransition.SetNextScene("TitleScreen");
                            sceneTransition.NextScene();
                            break;
                    }
                }
            }

            if (newStatement.update && newSpeaker.update)
            {
                textBox.text = newStatement.value.TrimStart();
                nameBox.text = newSpeaker.value.ToTitleCase();
            }
            else if (newStatement.update && !newSpeaker.update)
            {
                textBox.text = newStatement.value;
            }
            else if (!newStatement.update && newSpeaker.update)
            {
                // Ignore and continue on
                currentLine++;
                SetDialogue(currentLine);
            }

            // TODO: This code assumes all monologues span multiple lines and that monologues are never mixed with regular text. Revise if and when necessary
            if (monologue && !monologueEnd)
            {
                textBox.text = "<i>" + textBox.text.Replace("<monologue>", "").Replace("</monologue>", "") + "</i>";
            }
            else if (monologueEnd)
            {
                monologue = monologueEnd = false;
                textBox.text = "<i>" + textBox.text.Replace("<monologue>", "").Replace("</monologue>", "") + "</i>";
            }
        }
    }

    public Dictionary<string, CanvasGroup> props;

    void HideAllProps()
    {
        foreach (var prop in props)
        {
            prop.Value.alpha = 0f;
        }
    }
    void HideProp(string name)
    {
        LeanTween.alphaCanvas(props[name], 0, 1f); // todo: add option into parsing on if to fade out or not
    }

    void ShowProp(string name)
    {
        props[name].alpha = 1;
    }
    void HideTextbox()
    {
        textBoxCanvasGroup.alpha = 0;
    }

    void ShowTextbox()
    {
        textBoxCanvasGroup.alpha = 1;
    }

    [Space]

    public CanvasGroup leftSpeakerIndicator;
    public CanvasGroup rightSpeakerIndicator;
    public CanvasGroup speakerBackground;
    void ChangeSpeakerIndicator(string side)
    {
        if(side == "right")
        {
            rightSpeakerIndicator.alpha = 1;
            leftSpeakerIndicator.alpha = 0;
            speakerBackground.alpha = 1;
        }
        else if (side == "left")
        {
            leftSpeakerIndicator.alpha = 1;
            rightSpeakerIndicator.alpha = 0;
            speakerBackground.alpha = 1;
        }
        else
        {
            rightSpeakerIndicator.alpha = 0;
            leftSpeakerIndicator.alpha = 0;
            speakerBackground.alpha = 0;
        }
    }

    public Transform stage;

    void UpdateStage(string character)
    {
        // e.g. <character>Remilia / Mad / 0, 0</character>
        //      <character>Remilia / 500, 0 / 0.5</character>
        //      <character>Remilia / Disappear</character>

        string[] data = character.Split('/').Select(x => x.Trim()).ToArray();

        bool alreadyOnstage = false;
        PuppetData puppet;

        // oh my god
        if (onstageCharacters.Any(x => x.name == data[0]))
        {
            puppet = onstageCharacters.Find(x => x.name == data[0]);
            alreadyOnstage = true;
        }
        else
        {
            puppet = characters.Find(x => x.name == data[0]);
        }

        if (puppet.expressions.ContainsKey(data[1]))
        {
            PuppetData obj = null;

            if (!alreadyOnstage)
            {
                obj = Instantiate(puppet, stage);
                obj.name = puppet.name;
                puppet = obj.GetComponent<PuppetData>();
            }

            puppet.currentExpression = data[1];
            puppet.currentLocation = ToVector2(data[2].Replace("x", puppet.currentLocation.x.ToString()).Replace("y", puppet.currentLocation.y.ToString()));
            //Debug.Log(puppet.currentLocation);

            if (data.Length > 3)
                puppet.UpdateSprite(float.Parse(data[3], CultureInfo.InvariantCulture));
            else
                puppet.UpdateSprite(0);

            if(!alreadyOnstage)
                onstageCharacters.Add(obj);
        }
        else if (data[1] == "Disappear")
        {
            // disappear
            onstageCharacters.Remove(puppet);
            Destroy(puppet.gameObject);
        }
        else
        {
            puppet.currentLocation = ToVector2(data[1].Replace("x", puppet.currentLocation.x.ToString()).Replace("y", puppet.currentLocation.y.ToString()));

            if (data.Length > 2)
                puppet.UpdateSprite(float.Parse(data[2], CultureInfo.InvariantCulture));
            else
                puppet.UpdateSprite(0);
        }
    }

    Vector2 ToVector2(string encoded)
    {
        float[] values = encoded.Split(',').Select(x => (float)jcdr.SimpleExtensions.Evaluate(x.Trim())).ToArray();
        return new Vector2(values[0], values[1]);
    }

    public void StartLevel()
    {
        PlayerPrefs.SetInt("CurrentLine", currentLine - 1);
        PlayerPrefs.SetString("NextScene", "SampleScene");
        sceneTransition.NextScene();
        Debug.Log("StartLevel");
    }

    public void RefuseLevel()
    {
        dialoguePaused = false;
        Debug.Log("RefuseLevel");
    }

    public void SkipLevel()
    {
        dialoguePaused = false;
        Debug.Log("SkipLevel");
    }

    public Dictionary<string, Sprite> backgrounds = new Dictionary<string, Sprite>();
    //public Image bg;
    //void DisplayBackground(string background)
    //{
    //    var bgData = background.Split("-"); // TODO: post-jam, do this more robustly
    //    bgData[0] = bgData[0].Replace("<title>", "").Replace("</title>", "").Trim().ToUpper();
    //    bg.sprite = backgrounds[bgData[0]];
    //}

    [SerializeField]
    float nextLineContinue = 0f;
    void DelayedContinue(float delay)
    {
        if (delay == -1)
        {
            currentLine += 1;
            SetDialogue(currentLine);
        }
        else if (delay == 0)
        {
            delay = 0.001f;
            nextLineContinue = delay;
        }
        else
        {
            nextLineContinue = delay;
        }
    }

    [SerializeField]
    Dictionary<int, int> convoLineToIndex = new Dictionary<int, int>();

    void UpdateConvoLineToIndex()
    {
        convoLineToIndex = new Dictionary<int, int>();

        int i = 1; // start at 1 because written format
        foreach (var x in convoData.GroupBy(x => x.index))
        {
            convoLineToIndex.Add(i, x.Key);
            i++;
        }
    }

    [Button]
    public void StartConversation(int startingLine = 1)
    {
        convoData = ParseFile(mainStory);

        // https://stackoverflow.com/questions/1300088/distinct-with-lambda
        // https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.groupby?view=net-7.0

        lengthOfConvo = convoData.GroupBy(x => x.index).Count();

        convoLineToIndex = new Dictionary<int, int>();

        UpdateConvoLineToIndex();

        SetDialogue(startingLine, true);
        currentLine = startingLine;

        foreach(CanvasGroup child in GetComponentsInChildren<CanvasGroup>())
        {
            child.alpha = 1f;
            child.interactable = true;
            child.blocksRaycasts = true;
        }
    }

    void EndConversation()
    {
        PlayerPrefs.SetInt("CurrentLine", 0);
        foreach (CanvasGroup child in GetComponentsInChildren<CanvasGroup>())
        {
            child.alpha = 0f;
            child.interactable = false;
            child.blocksRaycasts = false;
        }
    }
    [BoxGroup("Convert")]
    public TextAsset mainStory;
}
