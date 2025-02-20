using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Menu")]
    Animator anim;

    public GameObject levelSelectionCanvas;
    public GameObject mainMenuCanvas;

    public TextMeshProUGUI gameName;

    public Button startButton;
    public Button settingsButton;
    public Button quitButton;
    public Button backButton;
    public Button Quit_Yes;
    public Button Quit_No;
    public Button tutLevel;

    public GameObject confirmMenuCanvas;

    public Camera cam;
    public Transform camTarget; // Assign this in the inspector to the desired position and rotation

    [Header("Scenes To Load")]
    [SerializeField] private SceneField _tutorialScene;
    [SerializeField] private SceneField _mainMenuScene;

    private List<AsyncOperation> _scenesToLoad = new List<AsyncOperation>();

    private void Awake()
    {
        Time.timeScale = 1f;
        anim = GetComponent<Animator>();

        confirmMenuCanvas.SetActive(false);
        levelSelectionCanvas.SetActive(false);

        startButton.onClick.AddListener(MainMenuOut);
        backButton.onClick.AddListener(MainMenuIn);
        quitButton.onClick.AddListener(QuitGame);
        Quit_Yes.onClick.AddListener(OnConfirmQuit);
        Quit_No.onClick.AddListener(OnConfirmBack);
        tutLevel.onClick.AddListener(OnStartLevel);
    }

    public void MainMenuOut()
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        anim.SetBool("fromMenu", true);
        levelSelectionCanvas.SetActive(true);

        // Move the camera to the target position
        StartCoroutine(MoveCamera());
    }

    public void MainMenuIn()
    {
        anim.SetBool("fromMenu", false);
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        mainMenuCanvas.SetActive(true);
        levelSelectionCanvas.SetActive(false);
    }

    IEnumerator MoveCamera()
    {
        float duration = 1.5f; // Adjust time to match animation
        float elapsed = 0f;

        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            cam.transform.position = Vector3.Lerp(startPos, camTarget.position, t);
            cam.transform.rotation = Quaternion.Slerp(startRot, camTarget.rotation, t);

            yield return null;
        }


        cam.transform.position = camTarget.position;
        cam.transform.rotation = camTarget.rotation;
    }

    void OnStartLevel()
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

    public void QuitGame()
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
}
