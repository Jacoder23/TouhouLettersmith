using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
public class SceneTransition : MonoBehaviour
{
    // https://medium.com/c-sharp-progarmming/make-smooth-scene-transitions-in-unity-c-6b7c97e4c7e0
    public Animator transition;

    [Button]
    public void RestartScene()
    {
        StartCoroutine(RestartCurrentScene());
    }
    [Button]
    public void NextScene()
    {
        StartCoroutine(LoadNextScene());
    }

    [Button]
    public void SetNextScene(string sceneName)
    {
        PlayerPrefs.SetString("NextScene", sceneName);
    }

    public void Start()
    {
        transition.Play("ScreenTransitionEnter");
    }

    public void FakeTransition()
    {
        transition.Play("ScreenTransitionEnter");
    }

    IEnumerator LoadNextScene()
    {
        transition.Play("ScreenTransitionExit");
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(PlayerPrefs.GetString("NextScene"));
    }
    IEnumerator RestartCurrentScene()
    {
        transition.Play("ScreenTransitionExit");
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
