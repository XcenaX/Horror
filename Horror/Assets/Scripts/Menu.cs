using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{

    [SerializeField]
    private Button playButton,exitButton, optionsButton;

    // Start is called before the first frame update
    void Start()
    {
        playButton.onClick.AddListener(Play);
		exitButton.onClick.AddListener(Exit);
		optionsButton.onClick.AddListener(Options);
    }

    private void Play(){
        SceneManager.LoadScene("Scene1");
    }
    private void Options(){
        SceneManager.LoadScene("Options");
    }
    private void Exit(){
        Application.Quit();
    }

    private void OnDestroy() {
		playButton.onClick.RemoveAllListeners();
		exitButton.onClick.RemoveAllListeners();
		optionsButton.onClick.RemoveAllListeners();
	}
}
