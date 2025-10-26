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
        transition.Play("start");
    }

    IEnumerator LoadNextScene()
    {
        transition.Play("end");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(PlayerPrefs.GetString("NextScene"));
    }
}
