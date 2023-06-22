using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class QuestionaryTest : MonoBehaviour
{
    private string secretKey = "mySecretKey"; // Edit this value and make sure it's the same as the one stored on the server
    private string questionCountURL = "http://192.168.1.81/GetTotalQuestions.php"; //be sure to add a ? to your url
    private string questionRequestURL = "http://192.168.1.81/SetQuestion.php"; //be sure to add a ? to your url
    private string getQuestionURL = "http://192.168.1.81/GetQuestion.php";
    private string answerSendURL = "http://192.168.1.81/CheckAnswer.php";
    private string questionID;

    private int totalQuestions;
    public int questionsToAnswer;
    private int answeredQuestions;

    private List<int> questionsAsnwered;

    [SerializeField]
    private TextMeshProUGUI questionText;
    [SerializeField]
    private Image[] buttonsImg;
    private TextMeshProUGUI[] buttonsText;
    [SerializeField]
    private Color correctColor;
    [SerializeField]
    private Color wrongColor;
    


    [Space, SerializeField]
    private Animator panel;
    [SerializeField]
    private Animator loadingIcon;

    bool canSendQuestion = true;
    // remember to use StartCoroutine when calling this function!

    private void Start()
    {
        buttonsText = new TextMeshProUGUI[buttonsImg.Length];
        for (int i = 0; i < buttonsText.Length; i++)
        {
            buttonsText[i] = buttonsImg[i].GetComponentInChildren<TextMeshProUGUI>();
        }
        questionsAsnwered = new List<int>();
        StartCoroutine(GetQuestionsCount());

    }


    public void SendButton(int _buttonPressed)
    {
        if(canSendQuestion)
            StartCoroutine(SendAnswer(_buttonPressed));

    }

    IEnumerator GetQuestionsCount()
    {
        UnityWebRequest www = UnityWebRequest.Get(questionCountURL);
        yield return www.SendWebRequest();
        if (www.error != null)
        {
            Debug.LogError("ERROR: " + www.error);

        }
        else
        {
            totalQuestions = int.Parse(www.downloadHandler.text);

            RandomizeQuestion();
        }


    }

    private void RandomizeQuestion()
    {
        int randomQID = Random.Range(1, totalQuestions + 1);

        bool canGenerate = true;
        foreach (int item in questionsAsnwered)
        {
            if (randomQID == item)
            {
                canGenerate = false; 
                break;
            }
        }

        if (canGenerate)
        {
            questionID = randomQID.ToString();
            foreach (Image item in buttonsImg)
            {
                item.color = Color.white;
            }
            StartCoroutine(SendQuestionRequest());
            questionsAsnwered.Add(randomQID);
        }
        else
        {
            RandomizeQuestion();
        }
        
    }

    IEnumerator SendQuestionRequest()
    {
        List<IMultipartFormSection> wwwForm = new List<IMultipartFormSection>();
        wwwForm.Add(new MultipartFormDataSection("questionID", questionID));
        UnityWebRequest www = UnityWebRequest.Post(questionRequestURL, wwwForm);

        yield return www.SendWebRequest();
        if (www.error != null)
        {
            Debug.LogError("ERROR: " + www.error);
        }
        else
        {
            canSendQuestion = true;
            StartCoroutine(GetQuestion());
        }

    }
    IEnumerator GetQuestion()
    {
        UnityWebRequest www = UnityWebRequest.Get(getQuestionURL);
        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError("ERROR: " + www.error);

        }
        else
        {
            string[] fullQuestion = www.downloadHandler.text.Split('-');
            questionText.text = fullQuestion[0];
            buttonsText[0].text = fullQuestion[1];
            buttonsText[1].text = fullQuestion[2];
            buttonsText[2].text = fullQuestion[3];
            buttonsText[3].text = fullQuestion[4];
            HideLoadingScreen();
        }
    }
    IEnumerator SendAnswer(int _buttonPressed)
    {
        canSendQuestion = false;
        List<IMultipartFormSection> wwwForm = new List<IMultipartFormSection>();
        wwwForm.Add(new MultipartFormDataSection("answer", _buttonPressed.ToString()));
        UnityWebRequest www = UnityWebRequest.Post(answerSendURL, wwwForm);

        yield return www.SendWebRequest();
        if (www.error != null)
        {
            Debug.LogError("ERROR: " + www.error);
            yield break;
        }

        answeredQuestions++;
        if (www.downloadHandler.text == "OK")
        {
            buttonsImg[_buttonPressed - 1].color = correctColor;
        }
        else
        {
            buttonsImg[_buttonPressed - 1].color = wrongColor;
        }

        Invoke("ShowLoadingScreen", 1);
        if (answeredQuestions >= questionsToAnswer)
        {
            Invoke("EndQuiz", 4f);
        }
        else
        {
            Invoke("RandomizeQuestion", 4f);
        }
    }    

    public void ShowLoadingScreen()
    {
        panel.SetTrigger("FadeIn");
        loadingIcon.SetTrigger("GrowUp");
    }
    public void HideLoadingScreen()
    {
        panel.SetTrigger("FadeOut");
        loadingIcon.SetTrigger("GrowDown");
    }

    private void EndQuiz()
    {
        questionText.text = "Questionario Finalizado";
        foreach (Image item in buttonsImg)
        {
            item.gameObject.SetActive(false);
        }
        HideLoadingScreen();
    }

    public string Md5Sum(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }


}
