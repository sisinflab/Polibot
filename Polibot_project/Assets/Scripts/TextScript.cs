using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextScript : MonoBehaviour
{
    [SerializeField] GameObject loading_text;
    [SerializeField] GameObject Dot_1;
    [SerializeField] GameObject Dot_2;
    [SerializeField] GameObject Dot_3;

    [SerializeField] GameObject idle_instruction_text;
    [SerializeField] GameObject listening_instruction_text;

    private const float timeStep = 1.0f;
    private float startTime, timeDiff;

    private string activation_phrase;

    /*
     * The user interface has 4 states:
     *      1) Loading state: Rasa or the voice server are still not active;
     *      2) Idle state: The servers are  active and the chatbot is waiting for the user to say the activation phrase;
     *      3) Speaking state: The servers are active and the chatbot is responding to the user;
     *      4) Listening state: The servers are active, the user said the activation phrase and the chatbot is waiting for the user's request.
     *      
     *      The variable is initiated to 1 because when the program starts the servers are assumibily still not active.
     */
    private int state = 1;

    //Reference to the NetworkManager script
    private NetworkManager networkManager;
    [SerializeField] GameObject GameManager;

    //Reference to the animator to animate the screen
    public Animator animator;

    void Awake()
    {
        networkManager = GameManager.GetComponent<NetworkManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //I Instantiate the variables
        startTime = Time.time;

        //I retrieve the activation phrase from the main script
        activation_phrase = networkManager.getActivationPhrase();
    }

    //This function allows NetworkManager.cs to change the value of the state.
    public void setState(int state)
    {
        if(state != this.state)
        {
            UnityEngine.Debug.Log("Passaggio di stato da " + this.state + " a " + state);
            this.state = state;
        }
    }

    void Update()
    {
        switch (state)
        {
            case 1:
                animator.SetBool("Speaking", false);

                loading_text.SetActive(true);
                idle_instruction_text.SetActive(false);
                listening_instruction_text.SetActive(false);

                timeDiff = Time.time - startTime;

                if (timeDiff < timeStep)
                {
                    Dot_1.SetActive(false);
                    Dot_2.SetActive(false);
                    Dot_3.SetActive(false);
                }
                else if (timeDiff >= timeStep && timeDiff < 2 * timeStep)
                {
                    Dot_1.SetActive(true);
                }
                else if (timeDiff >= 2 * timeStep && timeDiff < 3 * timeStep)
                {
                    Dot_2.SetActive(true);
                }
                else if (timeDiff >= 3 * timeStep && timeDiff < 4 * timeStep)
                {
                    Dot_3.SetActive(true);
                }
                else
                {
                    startTime = Time.time;
                }
                break;

            case 2:
                animator.SetBool("Speaking", false);

                loading_text.SetActive(false);
                idle_instruction_text.SetActive(true);
                listening_instruction_text.SetActive(false);
                break;

            case 3:
                animator.SetBool("Speaking", true);

                loading_text.SetActive(false);
                idle_instruction_text.SetActive(false);
                listening_instruction_text.SetActive(false);
                break;

            case 4:
                animator.SetBool("Speaking", false);

                loading_text.SetActive(false);
                idle_instruction_text.SetActive(false);
                listening_instruction_text.SetActive(true);
                break;
        }
    }
}