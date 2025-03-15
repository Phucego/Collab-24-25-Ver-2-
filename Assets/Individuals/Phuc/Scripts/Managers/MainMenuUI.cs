using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Menu")]
    Animator anim;

    public GameObject levelSelectionCanvas;
    public GameObject mainMenuCanvas;

    public TextMeshProUGUI gameName;

    [Header("Buttons")]
    public Button startButton;
    public Button settingsButton;
    public Button quitButton;
    public Button backButton;
    public Button Quit_Yes;
    public Button Quit_No;
    public Button tutLevel;

    [Header("Navigation Indicator")]
    public RectTransform indicator;
    public float indicatorOffset = -3.4f;

    private int selectedIndex = 0;

    public GameObject confirmMenuCanvas;

    public Camera cam;
    public Transform camTarget;

    [Header("Scenes To Load")]
    [SerializeField] private SceneField _tutorialScene;
    [SerializeField] private SceneField _mainMenuScene;

    private List<AsyncOperation> _scenesToLoad = new List<AsyncOperation>();

    [Header("Camera Original Position")]
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;

    [Header("Gate Related")]
    [SerializeField] private GameObject gate;
    public GameObject targetGate;
    private Vector3 closedPosition;


    public List<Button> menuButtons = new List<Button>();
    private void Awake()
    {
        Time.timeScale = 1f;
        anim = GetComponent<Animator>();

        confirmMenuCanvas.SetActive(false);
        levelSelectionCanvas.SetActive(false);

        originalCamPos = cam.transform.position;
        originalCamRot = cam.transform.rotation;

        closedPosition = gate.transform.position;

  
        //TODO: Add buttons from the main menu
        menuButtons.Add(startButton);
        menuButtons.Add(settingsButton);
        menuButtons.Add(quitButton);
        


        AssignButtonListeners();
        AssignHoverListeners();

        MoveIndicator(menuButtons[selectedIndex]);
    }

    private void Update()
    {
        HandleMenuNavigation();

        if (indicator != null && menuButtons.Count > 0)
        {
            Vector3 targetPosition = new Vector3(indicator.position.x, menuButtons[selectedIndex].transform.position.y - indicatorOffset, indicator.position.z);
            indicator.position = Vector3.Lerp(indicator.position, targetPosition, Time.deltaTime * 10f);
        }
    }

    public void OnLevelSelectionMenu()
    {
        menuButtons.Clear();        //clear the previous list

        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        anim.SetBool("fromMenu", true);
        levelSelectionCanvas.SetActive(true);

        StartCoroutine(MoveCamera(camTarget.position, camTarget.rotation));
        StartCoroutine(OpenGate(true));

        menuButtons.Add(backButton);
        menuButtons.Add(tutLevel);
    }

    public void OnReturnToMainMenu()
    {
        menuButtons.Clear();

        anim.SetBool("fromMenu", false);
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        mainMenuCanvas.SetActive(true);
        levelSelectionCanvas.SetActive(false);

        StartCoroutine(MoveCamera(originalCamPos, originalCamRot));
        StartCoroutine(OpenGate(false));

        //TODO: Add buttons from the main menu
        menuButtons.Add(startButton);
        menuButtons.Add(settingsButton);
        menuButtons.Add(quitButton);
    }

    IEnumerator MoveCamera(Vector3 targetPos, Quaternion targetRot)
    {
        float duration = 1f;
        float elapsed = 0f;

        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            cam.transform.position = Vector3.Lerp(startPos, targetPos, t);
            cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        cam.transform.position = targetPos;
        cam.transform.rotation = targetRot;
    }

    public void OnStartLevel()
    {
        levelSelectionCanvas.SetActive(false);
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        StartCoroutine(StartLevelTransition());
    }

    #region Confirmation Menu
    private void OnConfirmQuit()
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        Debug.Log("Quit");
        Application.Quit();
    }

    private void OnConfirmBack()
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        anim.SetBool("isConfirmationMenu", false);
    }
    #endregion

    public void OnQuitGame()
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        anim.SetBool("isConfirmationMenu", true);
    }

    IEnumerator StartLevelTransition()
    {
        anim.SetTrigger("isStart");
        AudioManager.Instance.PlaySoundEffect("Swoosh_SFX");
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(_tutorialScene);
    }

    IEnumerator OpenGate(bool openGate)
    {
        Debug.Log(openGate);
        float duration = 2.5f;
        Vector3 targetPos = openGate ? targetGate.transform.position : closedPosition;
        Vector3 startPos = gate.transform.position;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            gate.transform.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        gate.transform.position = targetPos;
    }

    #region Button Indicator
    private void AssignButtonListeners()
    {
        //MAIN MENU
        startButton.onClick.AddListener(() => { MoveIndicator(startButton); OnLevelSelectionMenu(); });
        settingsButton.onClick.AddListener(() => MoveIndicator(settingsButton));
        quitButton.onClick.AddListener(() => { MoveIndicator(quitButton); OnQuitGame(); });

        //LEVEL SELECTION
        backButton.onClick.AddListener(() => { MoveIndicator(backButton); OnReturnToMainMenu(); }); 
        tutLevel.onClick.AddListener(() => { MoveIndicator(tutLevel); OnStartLevel(); }); 
    }

    private void AssignHoverListeners()
    {
        foreach (var button in menuButtons)
        {
            if (button == null) continue;

            EventTrigger trigger = button.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entry.callback.AddListener((data) => { OnButtonHover(button); });

            trigger.triggers.Add(entry);
        }
    }

    private void MoveIndicator(Button targetButton)
    {
        StopAllCoroutines(); // Stop any ongoing movement before starting a new one
        StartCoroutine(SmoothMoveIndicator(targetButton));
    }

    private void OnButtonHover(Button hoveredButton)
    {
        // Reset all button colors before applying new hover effect
        foreach (var button in menuButtons)
        {
            ResetButtonColor(button);
        }

        // Apply hover effect to the hovered button
        selectedIndex = menuButtons.IndexOf(hoveredButton);
        HighlightButton(hoveredButton);
    }

    private void HandleMenuNavigation()
    {
        int previousIndex = selectedIndex;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex = (selectedIndex - 1 + menuButtons.Count) % menuButtons.Count;
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex = (selectedIndex + 1) % menuButtons.Count;
        }

        if (previousIndex != selectedIndex)
        {
            StartCoroutine(SmoothMoveIndicator(menuButtons[selectedIndex]));
            AudioManager.Instance.PlaySoundEffect("Hover_SFX");

            // Reset previous button color and hover effect
            if (menuButtons[previousIndex] != null)
            {
                ResetButtonColor(menuButtons[previousIndex]);

                HoverAnim previousHover = menuButtons[previousIndex].GetComponent<HoverAnim>();
                if (previousHover != null)
                {
                    previousHover.RemoveKeyboardSelection();
                }
            }

            // Apply new button highlight
            if (menuButtons[selectedIndex] != null)
            {
                HighlightButton(menuButtons[selectedIndex]);

                HoverAnim newHover = menuButtons[selectedIndex].GetComponent<HoverAnim>();
                if (newHover != null)
                {
                    newHover.ApplyKeyboardSelection();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            menuButtons[selectedIndex].onClick.Invoke();
        }
    }



    // Smoothly moves the indicator to the selected button
    private IEnumerator SmoothMoveIndicator(Button targetButton)
    {
        float duration = 0.2f;
        float elapsed = 0f;

        Vector3 startPos = indicator.position;
        Vector3 targetPos = new Vector3(indicator.position.x, targetButton.transform.position.y - indicatorOffset, indicator.position.z);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            indicator.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        indicator.position = targetPos; // Ensure it reaches the final position
    }

    // Changes the button color to a highlighted state
    private void HighlightButton(Button button)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white; 
        button.colors = colors;
    }

    // Resets the button color to default
    private void ResetButtonColor(Button button)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = Color.gray; // Default color
        button.colors = colors;
    }

   
    #endregion

}
