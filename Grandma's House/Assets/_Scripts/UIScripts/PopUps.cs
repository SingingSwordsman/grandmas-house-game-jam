using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PopUps : MonoBehaviour
{
    //Singleton to be called whenever
    public static PopUps instance;

    [SerializeField] GameObject popUpPanel;
    [SerializeField] TMP_Text popUpText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Start()
    {
       if(instance == null)
       {
            instance = this;
            DontDestroyOnLoad(gameObject);
       }
       else if(instance != this && instance != null) 
       {
            Destroy(gameObject);
       }

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PopUp("This is a test");
        }
  
    }

    public void ClosePopUp()
    {
        if(popUpPanel.activeInHierarchy)
        {
            popUpPanel.SetActive(false);
        }
    }

    public void PopUp(string text)
    {
        if (!popUpPanel.activeInHierarchy)
        {
            popUpText.text = text;
            popUpPanel.SetActive(true);
        }
    }

}
