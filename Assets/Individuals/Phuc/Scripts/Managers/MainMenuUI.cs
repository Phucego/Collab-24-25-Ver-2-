using System;
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

    public GameObject confirmationMenu;
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

    [Header("Navigation Indicators")]
    public RectTransform indicator; // Main menu indicator
    public RectTransform confirmationIndicator; // Separate indicator for confirmation menu
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

        // Initialize main menu buttons
        menuButtons.Add(startButton);
        menuButtons.Add(settingsButton);
        menuButtons.Add(quitButton);

        AssignButtonListeners();
        AssignHoverListeners();
        MoveIndicator(menuButtons[selectedIndex]);
    }

    private void Start()
    {
        Quit_Yes.onClick.AddListener(Application.Quit);
        Quit_No.onClick.AddListener(() => ToggleConfirmationMenu(false));
    }

    private void Update()
    {
        HandleMenuNavigation();
    }

    private void ToggleConfirmationMenu(bool isActive, bool isMainMenu = false)
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        confirmationMenu.SetActive(isActive);
        anim.SetBool(isMainMenu ? "isConfirmMainMenu" : "isConfirmationMenu", isActive);

        menuButtons.Clear();

        if (isActive)
        {
            menuButtons.Add(Quit_Yes);
            menuButtons.Add(Quit_No);
            selectedIndex = 0; // Default to "Yes" button
        }
        else
        {
            menuButtons.Add(startButton);
            menuButtons.Add(settingsButton);
            menuButtons.Add(quitButton);
        }

        MoveIndicator(menuButtons[selectedIndex]);
    }
    public void OnStartLevel()
    {
        levelSelectionCanvas.SetActive(false);
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        StartCoroutine(StartLevelTransition());
    }

    IEnumerator StartLevelTransition()
    {
        anim.SetTrigger("isStart");
        AudioManager.Instance.PlaySoundEffect("Swoosh_SFX");
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(_tutorialScene);
    }
    private void HandleMenuNavigation()
    {
        int previousIndex = selectedIndex;

        if (confirmationMenu.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                selectedIndex = Mathf.Max(0, selectedIndex - 1);
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                selectedIndex = Mathf.Min(menuButtons.Count - 1, selectedIndex + 1);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                selectedIndex = (selectedIndex - 1 + menuButtons.Count) % menuButtons.Count;
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                selectedIndex = (selectedIndex + 1) % menuButtons.Count;
            }
        }

        if (previousIndex != selectedIndex)
        {
            StartCoroutine(SmoothMoveIndicator(menuButtons[selectedIndex]));
            AudioManager.Instance.PlaySoundEffect("Hover_SFX");
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            menuButtons[selectedIndex].onClick.Invoke();
        }
    }

    private IEnumerator SmoothMoveIndicator(Button targetButton)
    {
        float duration = 0.2f;
        float elapsed = 0f;

        RectTransform targetIndicator = confirmationMenu.activeSelf ? confirmationIndicator : indicator;

        Vector3 startPos = targetIndicator.position;
        Vector3 targetPos = new Vector3(
            confirmationMenu.activeSelf ? targetButton.transform.position.x : targetIndicator.position.x,
            targetButton.transform.position.y - indicatorOffset,
            targetIndicator.position.z
        );

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            targetIndicator.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        targetIndicator.position = targetPos;
    }

    private void MoveIndicator(Button button)
    {
        RectTransform targetIndicator = confirmationMenu.activeSelf ? confirmationIndicator : indicator;

        targetIndicator.position = new Vector3(
            confirmationMenu.activeSelf ? button.transform.position.x : targetIndicator.position.x,
            button.transform.position.y - indicatorOffset,
            targetIndicator.position.z
        );
    }

    private void AssignButtonListeners()
    {
        startButton.onClick.AddListener(OnStartLevel);
        settingsButton.onClick.AddListener(() => Debug.Log("Open Settings"));
        quitButton.onClick.AddListener(() => ToggleConfirmationMenu(true));
    }

    private void AssignHoverListeners()
    {
        foreach (Button button in menuButtons)
        {
            EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entry.callback.AddListener((eventData) => MoveIndicator(button));
            trigger.triggers.Add(entry);
        }
    }
}
