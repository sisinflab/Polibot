using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

using UnityEngine;
using UnityEngine.Networking;

//Speech to text libraries
using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;

// A class to help in creating the Json object to be sent to the rasa server
public class PostMessageJson
{
    public string message;
    public string sender;
}

[Serializable]
// A class to extract multiple json objects nested inside a value
public class RootReceiveMessageJson
{
    public ReceiveMessageJson[] messages;
}

[Serializable]
// A class to extract a single message returned from the bot
public class ReceiveMessageJson
{
    public string recipient_id;
    public string text;
    public string image;
    public string attachemnt;
    public string button;
    public string element;
    public string quick_reply;
}

public class NetworkManager : MonoBehaviour
{

    private const string hello_url = "http://localhost:5005";
    private const string rasa_url = "http://localhost:5005/webhooks/rest/webhook";
    private const string voice_url = "http://127.0.0.1:8081";
    private bool rasa_loading = true;
    private bool voice_server_loading = true;
    private const string shell_program = "powershell.exe";
    private Process server_shell, actions_shell, voice_shell;
    private const string server_command = "Set-ExecutionPolicy Unrestricted -Scope Process -force; Polibot_venv/Scripts/activate.ps1; cd Assets/Rasa; rasa run";
    private const string actions_command = "Set-ExecutionPolicy Unrestricted -Scope Process -force; Polibot_venv/Scripts/activate.ps1; cd Assets/Rasa; rasa run actions";
    private const string voice_server_command = "Set-ExecutionPolicy Unrestricted -Scope Process -force; Polibot_venv/Scripts/activate.ps1; cd Assets/Scripts; python TTS_Engine.py";
    private DictationRecognizer dictationRecognizer;
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();
    private const string activationPhrase = "Ehi Polibot";
    private const string activationPhraseResponse = "Ti ascolto";
    private bool speaking = false;
    private int state = 1;

    //Reference to the UI script
    private TextScript textscript;
    [SerializeField] GameObject TextManager;

    public string getActivationPhrase()
    {
        return activationPhrase;
    }

    void Awake()
    {
        textscript = TextManager.GetComponent<TextScript>();
    }

    void Start()
    {
        StartCoroutine(startRasa());
        StartCoroutine(startVoiceServer());

        actions.Add(activationPhrase.ToLower(), detectedActivationPhrase); //The text is recognized in lower case
        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;

        dictationRecognizer = new DictationRecognizer();
        dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;
        dictationRecognizer.DictationComplete += DictationRecognizer_DictationComplete;
    }

    /*--------------------------------------------------------------------------------------------------------------------------------
     *                                          FUNCTIONS FOR THE VOICE INTERACTION: START
     * -------------------------------------------------------------------------------------------------------------------------------
     */

    private void Update()
    {
        if (speaking && state != 3)
        {
            state = 3;
            textscript.setState(3);

            dictationRecognizer.Stop();
            /*
             * I need to make sure that the Phrase recognition system is running otherwise the program raises an error for trying to shutdown
             * a phrase recognition system which is not running.
             */
            if (PhraseRecognitionSystem.Status != SpeechSystemStatus.Stopped)
            {
                PhraseRecognitionSystem.Shutdown();
            }
        }
    }

    private void checkAllServersRunning()
    {
        //The last running server makes the program switch to state = 2
        if (!voice_server_loading && !rasa_loading)
        {
            /*
             *  If both servers are up and running I can intialize the voice recognition
             *  and change the state of the program.
             */
            keywordRecognizer.Start();
            StartCoroutine(waitSpeechChangeState(2));
        }
    }

    //Function that opens the command prompt, starts the voice server and waits until it is up and running
        private IEnumerator startVoiceServer()
    {
        while (voice_server_loading)
        {
            UnityWebRequest request = new UnityWebRequest(voice_url, "GET");
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.error != null || request.downloadHandler.text == null || request.downloadHandler.text == "")
            {
                /*
                 * The connection error can be raised for two reasons:
                 * 1) The powershell is not running therefore it cannot start the voice server
                 * 2) The powershell is running but it didn't start the voice server yet.
                 * 
                 * In the first case the varaible "powershell" is null and i have to start the powershell,
                 * in the second case the varaible is not null and all I have to do is waiting.
                 */
                if (voice_shell == null)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(shell_program);
                    startInfo.WindowStyle = ProcessWindowStyle.Minimized;
                    startInfo.Arguments = voice_server_command;
                    voice_shell = Process.Start(startInfo);

                    UnityEngine.Debug.Log("Starting voice server");
                }
                yield return new WaitForSeconds(3);
            }
            else
            {
                voice_server_loading = false;
                StartCoroutine(checkVoiceStatus());

                checkAllServersRunning();
            }
            //I dispose the web request to avoid memory leaks
            request.Dispose();
        }
    }

    private IEnumerator checkVoiceStatus()
    {
        while (!voice_server_loading)
        {
            UnityWebRequest request = new UnityWebRequest(voice_url, "GET");
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.error != null || request.downloadHandler.text == null || request.downloadHandler.text == "")
            {
                /*
                 * In case of connection error or server error I have to reset the program to its initial state.
                 * 
                 * If the program is closed leaving the powershells open, then the program is opened again and then
                 * the voice server stops working for any reason the object voice_shell does not get
                 * instanciated so it is null.
                 * I need to use try-catch because the argument of the EndProcessTree function could be null.
                 */
                speaking = false;
                try
                {
                    EndProcessTree(voice_shell.Id.ToString());
                    voice_shell = null;
                    UnityEngine.Debug.Log("The voice server has been closed.");
                }
                catch (NullReferenceException e)
                {
                    UnityEngine.Debug.LogWarning("The voice server was null. \n" + e);
                }
                catch
                {
                    UnityEngine.Debug.LogWarning("An exception was raised trying to close the voice server.");
                }

                voice_server_loading = true;
                StartCoroutine(waitSpeechChangeState(1));
                StartCoroutine(startVoiceServer());
            }
            else
            {
                //In this case the server provided the voice status correctly
                if (request.downloadHandler.text == "True")
                {
                    speaking = true;
                }
                else
                {
                    speaking = false;
                }
                yield return new WaitForSeconds(0.5f);
            }
            //I dispose the web request to avoid memory leaks
            request.Dispose();
        }
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs phrase)
    {
        UnityEngine.Debug.Log(phrase.text);
        actions[phrase.text].Invoke();
    }

    private void DictationRecognizer_DictationResult(string resultText, ConfidenceLevel confidence)
    {
        //I prevent the activation phrase to be sent to rasa
        if (resultText != activationPhrase)
        {
            //I write the dictation result in the log for testing purposes
            UnityEngine.Debug.Log(resultText);
            SendMessageToRasa(resultText);
        }
    }

    private void DictationRecognizer_DictationComplete(DictationCompletionCause cause)
    {
        if (cause != DictationCompletionCause.Complete)
        {
            UnityEngine.Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", cause);
            StartCoroutine(waitSpeechChangeState(2));
        }
    }

    private void detectedActivationPhrase()
    {
        StartCoroutine(speakingCoroutine(activationPhraseResponse, 4));
    }

    private IEnumerator speakingCoroutine(string sentence, int newState)
    {
        // Create a json object from user message
        PostMessageJson postMessage = new PostMessageJson
        {
            sender = "user",
            message = sentence
        };
        string jsonBody = JsonUtility.ToJson(postMessage);

        UnityWebRequest request = new UnityWebRequest(voice_url, "POST");
        byte[] rawBody = new System.Text.UTF8Encoding().GetBytes(jsonBody);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(rawBody);
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        //I dispose the web request to avoid memory leaks
        request.Dispose();

        /*
         * I need to wait for the voice to start speaking before the state changes, otherwise the state changing coroutine
         * would detect that the voice is not speaking and it would change the state before the voice starts.
         */
        while (!speaking)
            yield return null;

        StartCoroutine(waitSpeechChangeState(newState));
    }

    //This coroutine prevents the state from being changed while the voice is speaking
    private IEnumerator waitSpeechChangeState(int newState)
    {
        while (speaking)
            yield return null;

        switch (newState)
        {
            case 1:
                dictationRecognizer.Stop();
                while (dictationRecognizer.Status != SpeechSystemStatus.Stopped)
                    yield return null;

                PhraseRecognitionSystem.Shutdown();
                while (PhraseRecognitionSystem.Status != SpeechSystemStatus.Stopped)
                    yield return null;
                break;
            case 2:
                dictationRecognizer.Stop();
                while (dictationRecognizer.Status != SpeechSystemStatus.Stopped)
                    yield return null;

                PhraseRecognitionSystem.Restart();
                while (PhraseRecognitionSystem.Status != SpeechSystemStatus.Running)
                    yield return null;
                break;
            case 4:
                /*
                 * I need to make sure that the Phrase recognition system is running otherwise the program raises an error for trying to shutdown
                 * a phrase recognition system which is not running.
                 */
                if (PhraseRecognitionSystem.Status != SpeechSystemStatus.Stopped)
                {
                    PhraseRecognitionSystem.Shutdown();
                    while (PhraseRecognitionSystem.Status != SpeechSystemStatus.Stopped)
                        yield return null;
                }

                dictationRecognizer.Start();
                while (dictationRecognizer.Status != SpeechSystemStatus.Running)
                    yield return null;
                break;
        }
        state = newState;
        textscript.setState(newState);
    }

    /*--------------------------------------------------------------------------------------------------------------------------------
     *                                          FUNCTIONS FOR THE VOICE INTERACTION: END
     * -------------------------------------------------------------------------------------------------------------------------------
     */

    /*--------------------------------------------------------------------------------------------------------------------------------
     *                                          FUNCTIONS TO INTERACT WITH RASA: START
     * -------------------------------------------------------------------------------------------------------------------------------
     */

    //Function that opens the command prompt and starts Rasa
    private IEnumerator startRasa()
    {
        /*
         * Now that Rasa is starting we need to wait until it is completely up and running.
         * If it is up and running the hello_url leads to a page in which is written: Hello from Rasa: [Version],
         * otherwise the url leads to an error page.
         * 
         * This operation might take a lot of time so it cannot be done by a function, which code is executed within a frame time,
         * but has to be done with a coroutine that downloads the content of the page and checks it every 3 seconds.
         */

        while (rasa_loading)
        {
            UnityWebRequest request = new UnityWebRequest(hello_url, "GET");
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.error != null)
            {
                UnityEngine.Debug.LogError(request.error);

                /*
                 * The connection error can be raised for two reasons:
                 * 1) The powershell is not running therefore it cannot start Rasa
                 * 2) The powershell is running but it didn't start Rasa yet.
                 * 
                 * In the first case the varaible "powershell" is null and i have to start the powershell,
                 * in the second case the varaible is not null and all I have to do is waiting.
                 */
                if (server_shell == null && actions_shell == null)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(shell_program);
                    startInfo.WindowStyle = ProcessWindowStyle.Minimized;
                    startInfo.Arguments = server_command;
                    server_shell = Process.Start(startInfo);

                    UnityEngine.Debug.Log("Starting Rasa server");
                    yield return null;

                    startInfo = new ProcessStartInfo(shell_program);
                    startInfo.WindowStyle = ProcessWindowStyle.Minimized;
                    startInfo.Arguments = actions_command;
                    actions_shell = Process.Start(startInfo);

                    UnityEngine.Debug.Log("Starting action server");
                }

                yield return new WaitForSeconds(3);
            }
            else
            {
                UnityEngine.Debug.Log(request.downloadHandler.text);
                rasa_loading = false;
                StartCoroutine(checkRasa());

                checkAllServersRunning();
            }
            //I dispose the web request to avoid memory leaks
            request.Dispose();
        }
    }

    private IEnumerator checkRasa()
    {
        while (!rasa_loading)
        {
            UnityWebRequest request = new UnityWebRequest(hello_url, "GET");
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.error != null)
            {
                /*
                 * In case of connection error I have to reset the program to its initial state.
                 * 
                 * If the program is closed leaving the powershells open, then the program is opened again and then
                 * the Rasa server stops working for any reason the objects server_shell and actions_shell do not get
                 * instanciated so they are null.
                 * I need to use try-catch because the arguments of the EndProcessTree functions could be null.
                 */
                try
                {
                    EndProcessTree(actions_shell.Id.ToString());
                    actions_shell = null;
                    UnityEngine.Debug.Log("The actions server has been closed.");
                }
                catch (NullReferenceException e)
                {
                    UnityEngine.Debug.LogWarning("The action server was null. \n" + e);
                }
                catch
                {
                    UnityEngine.Debug.LogWarning("An exception was raised trying to close the actions server.");
                }

                yield return null;

                try
                {
                    EndProcessTree(server_shell.Id.ToString());
                    server_shell = null;
                    UnityEngine.Debug.Log("The server has been closed.");
                }
                catch (NullReferenceException e)
                {
                    UnityEngine.Debug.LogWarning("The Rasa server was null. \n" + e);
                }
                catch
                {
                    UnityEngine.Debug.LogWarning("An exception was raised trying to close the Rasa server.");
                }

                yield return null;

                rasa_loading = true;
                StartCoroutine(waitSpeechChangeState(1));

                StartCoroutine(startRasa());
            }
            else
            {
                yield return new WaitForSeconds(5);
            }
            //I dispose the web request to avoid memory leaks
            request.Dispose();
        }
    }

    public void SendMessageToRasa(string var_message)
    {
        // Create a json object from user message
        PostMessageJson postMessage = new PostMessageJson
        {
            sender = "user",
            message = var_message
        };

        string jsonBody = JsonUtility.ToJson(postMessage);
        print("User json : " + jsonBody);

        // Create a post request with the data to send to Rasa server
        StartCoroutine(PostRequest(rasa_url, jsonBody));
    }

    private IEnumerator PostRequest(string url, string jsonBody)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] rawBody = new System.Text.UTF8Encoding().GetBytes(jsonBody);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(rawBody);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        RecieveResponse(request.downloadHandler.text);
        //I dispose the web request to avoid memory leaks
        request.Dispose();
    }

    // Parse the response received from the bot
    public void RecieveResponse(string response)
    {
        // Deserialize response recieved from the bot
        RootReceiveMessageJson recieveMessages =
            JsonUtility.FromJson<RootReceiveMessageJson>("{\"messages\":" + response + "}");

        //Show message based on message type on UI
        foreach (ReceiveMessageJson message in recieveMessages.messages)
        {
            FieldInfo[] fields = typeof(ReceiveMessageJson).GetFields();
            foreach (FieldInfo field in fields)
            {
                string data = null;

                // extract data from response in try-catch for handling null exceptions
                try
                {
                    data = field.GetValue(message).ToString();
                }
                catch (NullReferenceException) { }

                // print data
                if (data != null && field.Name != "recipient_id")
                {
                    UnityEngine.Debug.Log("Bot said \"" + data + "\"");

                    StartCoroutine(speakingCoroutine(data, 2));
                }
            }
        }
    }

    /*--------------------------------------------------------------------------------------------------------------------------------
    *                                          FUNCTIONS TO INTERACT WITH RASA: END
    * --------------------------------------------------------------------------------------------------------------------------------
    */

    /*--------------------------------------------------------------------------------------------------------------------------------
    *                                          FUNCTIONS TO INTERACT WITH THE PROCESSES: START
    * --------------------------------------------------------------------------------------------------------------------------------
    */

    private static void EndProcessTree(string PID)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "taskkill",
            Arguments = $"/pid {PID} /f /t",
            CreateNoWindow = true,
            UseShellExecute = false
        }).WaitForExit();
    }

    /*--------------------------------------------------------------------------------------------------------------------------------
    *                                          FUNCTIONS TO INTERACT WITH THE PROCESSES: END
    * --------------------------------------------------------------------------------------------------------------------------------
    */
}