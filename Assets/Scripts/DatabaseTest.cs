using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
public class DatabaseTest : MonoBehaviour
{
    private string secretKey = "mySecretKey"; // Edit this value and make sure it's the same as the one stored on the server
    private string askQuestionURL = "http://192.168.1.81/AskQuestion.php"; //be sure to add a ? to your url
    private string selectURL = "http://192.168.1.81/SelectText.php";

    [SerializeField]
    private TextMeshProUGUI responseText;
    [SerializeField]
    private TextMeshProUGUI statusText;
    [SerializeField]
    private TextMeshProUGUI inputField;

    // remember to use StartCoroutine when calling this function!
    
    public void SendButton()
    {
        if (inputField.text == string.Empty)
        {
            statusText.text = "Error input empty";
            return;
        }

        StartCoroutine(AskQuestion(inputField.text));

        inputField.text = "";
    }
    IEnumerator AskQuestion(string _questionID)
    {
        List<IMultipartFormSection> wwwForm = new List<IMultipartFormSection>();
        wwwForm.Add(new MultipartFormDataSection("questionID", _questionID));
        UnityWebRequest www = UnityWebRequest.Post(askQuestionURL, wwwForm);
        statusText.text = "Sending Data...";

        yield return www.SendWebRequest();
        if (www.error != null)
        {
            Debug.LogError("ERROR: " + www.error);
            statusText.text = "Error Sending Request";
        }
        else
        {
            statusText.text = "Data Sent";
            StartCoroutine(SelectText());
        }

    }

    // Get the scores from the MySQL DB to display in a GUIText.
    // remember to use StartCoroutine when calling this function!
    IEnumerator SelectText()
    {
        responseText.text = "Downloading Data...";
        UnityWebRequest www = UnityWebRequest.Get(selectURL);
        yield return www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError("ERROR: " + www.error);
            statusText.text = "Error getting response";
            responseText.text = "ERROR";

        }
        else
        {
            responseText.text = www.downloadHandler.text;// this is a GUIText that will display the scores in game.
            statusText.text = "Data Recived";
        }
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
