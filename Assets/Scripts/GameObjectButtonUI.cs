using UnityEngine;
using UnityEngine.SceneManagement;

// Designed by Jacob Weedman
public class GameObjectButtonUI : MonoBehaviour
{
    public string ButtonMode; // "MENU", "SCENE"
    public string DestinationScene;
    public string MenuName; // Name of the menu
    public bool StartMenuDisabled;

    public GameObject MenuRefrence;

    void Awake()
    {
        if (ButtonMode == "MENU")
        {
            MenuRefrence =  GameObject.Find(MenuName); // GameObject refrence for menu as it will otherwise not work when disabled

            if (StartMenuDisabled && GameObject.Find(MenuName))
            {
                GameObject.Find(MenuName).SetActive(false);
            }
        }
    }

    void OnMouseOver()
    {
	gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
    }

    void OnMouseExit()
    {
	gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
    }

    void OnMouseDown()
    {
        switch (ButtonMode)
        {
            case "MENU":
                // Open or close UI Element
                try
                {
                    // Switches the active state of the MenuGameObject every time the button is pressed
                    MenuRefrence.SetActive(!MenuRefrence.activeSelf);
                }
                catch
                {
                    Debug.Log("Hey you! The button you just pressed is broken.");
                }
                break;
            case "SCENE":
                // Change Scene
                
                try // Scene success
                {
                    SceneManager.LoadScene(DestinationScene);
                }
                catch // Scene error
                {
                    Debug.Log("Hey you! The button you just pressed is broken. It seems like the scene you selected does not exist or is not added to the build profile.");
                }           
                break;
            default:
                // Error message
                Debug.Log("Hey you! The button you just pressed is broken. It seems like 'ButtonMode' variable is invalid.");
                break;
        }
    }

}


