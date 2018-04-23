using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public static GameController instance;

    public GameObject fishermanPrefab, emailGO, emailPrefab, gameOverScreen, tutPanel, startPoint, endPoint, nextButton;
    public Text senderText, subjectText, messageText, mishandledMsgsText, moneyText, tutMessageText, gameOverText;
    public int startingAmtMoney, startingMsgsLeft;
    public bool isOnTutorial, canPullRod, canBeBitten;
    public AudioSource error, trash, mail, fired;
    bool curMessageSpam;
    EmailController currentMessage;
    public int curAmtMoney = 0, msgsLeft = 0, goodMsgsDelivered = 0, spamRemoved = 0;

    float spawnTime = 3f;

    string[] names = {"Pricilla Manry", "Despina Tollison","Corinne Pyatt","Irwin Brightwell","Clemente Arline","Fredericka Banach",
                      "Shaunta Sciacca","Jennifer Plain","Marcelene Sereno","Wilton Lorusso","Lawrence Colquitt","Julie Hiner",
                      "Roger Amell","Reuben Brack","Erlinda Bissonette","Lydia Garten","Bobbie Carrera","Wanda Rall",
                      "Antonia Blackwelder","Jarrett Kita","Jacque Cassara","Casie Petrillo","Danyelle Rowse","Della Brenton",
                      "Ciera Molina","Consuelo Friddle","Olin Higuchi","Herminia Parton","Marylou Parkerson","Peggy Redfern",
                      "Tyisha Hauk","Gerald Maniscalco","Modesto Gerstein","Mallory Vallejo","Cristobal Bays","Meridith Janey",
                      "Polly Hazley","Trista Clapp","Season Telles","Winifred Barb","Minta Minder","Margurite Struck",
                      "Rena Delao","Alleen Eagles","Fidela Calles","Dolores Lightner","Rheba Mcwhirter","Anastasia Poquette",
                      "Raymonde Carbone","Kathi Olaughlin"};

    string[] goodSubjects = {"Dinner Plans", "Vacation", "It happened again...", "I CAN'T BELIEVE IT!!", "WE NEED TO TALK NOW!",
                             "Long Time no see", "Hey", "Hello Friend", "Your shoes", "Business Proposal", "Meeting in 10",
                             "....really...", "Ever Seen Super Troopers?", "This weekend", "Next Week", "April Billing" };
    string[] goodBodies = {"Hey you,\n\nWant to get dinner this week ;)", "Hey Boss,\n\nI will be traveling to Hawaii next week." +
                           " Please take me off the schedule. Thanks!", ""};

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        emailGO.SetActive(false);
        nextButton.SetActive(false);
        gameOverScreen.SetActive(false);
        isOnTutorial = true;
        canPullRod = false;
        canBeBitten = false;
        ChangeMishandledMsgsAmt(startingMsgsLeft);
        ChangeMoneyAmt(startingAmtMoney);
    }

    void DecreaseSpawnTime()
    {
        spawnTime -= 0.45f;
        if(spawnTime <= 0.15f)
        {
            spawnTime = 0.15f;
        }
    }

	public void OnButtonHire()
    {
        if (ChangeMoneyAmt(-10))
        {
            Instantiate(fishermanPrefab, Vector3.one * 100, Quaternion.identity);
            if (isOnTutorial)
            {
                tutMessageText.text = "Great! Now go ahead and click anywhere on the land that you would like him to go.";
            }
        }
    }

    public void OnButtonNext()
    {
        Time.timeScale = 1f;
        nextButton.SetActive(false);
        tutPanel.SetActive(false);
    }

    public bool ChangeMoneyAmt(int amtToChangeBy)
    {
        if(curAmtMoney + amtToChangeBy < 0)
        {
            return false;
        }
        curAmtMoney += amtToChangeBy;
        moneyText.text = "Money: $" + curAmtMoney.ToString();
        return true;
    }

    public void ChangeMishandledMsgsAmt(int amtToChangeBy)
    {
        msgsLeft += amtToChangeBy;
        mishandledMsgsText.text = "Mishandled Messages Until Company Closure: " + msgsLeft.ToString();
        if(msgsLeft <= 0)
        {
            GameOver();
        }
    }

    public void SpawnTutEmail()
    {
        tutPanel.SetActive(false);
        canBeBitten = true;
        Instantiate(emailPrefab, startPoint.transform.position, Quaternion.identity);
        Invoke("ShowMessageTut", 0.1f);
    }

    void ShowMessageTut()
    {
        tutPanel.SetActive(true);
        tutMessageText.text = "Looks Like we have a message coming through. It will become attracted to your fishing rod as it approaches.";
        Time.timeScale = 0f;
        nextButton.SetActive(true);
    }

    public void ShowHowToCatch()
    {
        tutPanel.SetActive(true);
        canPullRod = true;
        tutMessageText.text = "When your fisherman is flashing, you know he has a bite. Click on your fisherman to reel in!";
    }

    public void ShowHowToDelMsg()
    {
        tutMessageText.text = "Yikes! This looks like some nasty spam, click on the 'Catch' button to get rid of it.";
        tutPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -220);
    }

    void EndTut()
    {
        isOnTutorial = false;
        tutMessageText.text = "Those are basics, good luck!";
        Invoke("HideFinalMessage", 5f);
    }

    void HideFinalMessage()
    {
        tutPanel.SetActive(false);
        SpawnNewEmail();
        InvokeRepeating("DecreaseSpawnTime", 10f, 15f);
    }

    void SpawnNewEmail()
    {
        Instantiate(emailPrefab, startPoint.transform.position, Quaternion.identity);
        Invoke("SpawnNewEmail", spawnTime);
    }

    public static Vector3 ClosestStreamPos(Vector3 startPoint)
    {
        startPoint.y = 0;

        NavMeshHit nmh = new NavMeshHit();

        Vector3 bestPoint = Vector3.zero;
        float lowestDist = 10;

        for (int i = 0; i < 36; i++)
        {
            Vector3 tv = GetRotatedVector(i * 10);

            //Debug.Log("TV: " + tv.ToString());

            if(NavMesh.FindClosestEdge(startPoint + tv, out nmh, 8)){
                //Debug.DrawLine(startPoint, tv + startPoint, Color.yellow, 50f);
                float curDist = Vector3.Distance(nmh.position, startPoint);
                //Debug.Log("curDist: " + curDist + " for i = " + i);
            
                if (curDist < lowestDist)
                {
                    //Debug.Log("New best point with dist: " + curDist);
                    lowestDist = curDist;
                    bestPoint = nmh.position;
                }
            }
            /*
            else
            {
                Debug.DrawLine(startPoint, tv + startPoint, Color.red, 50f);
            }
            */
        }
        /*
        Debug.Log(lowestDist);
        Debug.DrawLine(startPoint, bestPoint, Color.blue, 50f);
        Debug.Log(startPoint.ToString() + " bestPoint: " + bestPoint.ToString());
        */
        return (bestPoint);
    }

    static Vector3 GetRotatedVector(float angle)
    {
        //Debug.Log("angle: " + angle);
        return new Vector3(1.5f * Mathf.Cos(angle * Mathf.Deg2Rad), 0, 1.5f * Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    public void ShowMessageContents(EmailController curMessage)
    {
        emailGO.SetActive(true);

        curMessageSpam = curMessage.isSpam;
        currentMessage = curMessage;

        if (curMessageSpam)
        {
            senderText.text = "0nl1ne Farma-C";
            subjectText.text = "Wana get rippd fast!?!?";
            messageText.text = "r u week? wanna get ripd fastly??? buy sOM pillz online fast.. no doctor need!" + 
                               "\n\n100% legit, trustme!!!";
        }
        else
        {
            senderText.text = "Jimmy A. Miller";
            subjectText.text = "Hello Friend";
            messageText.text = "Hey man,\n\nLong time no see, we should catch up sometime! Let me know if you are free next week";
        }
        Time.timeScale = 0f;
    }

    public void Catch()
    {
        if (isOnTutorial)
        {
            EndTut();
        }

        Time.timeScale = 1f;
        emailGO.SetActive(false);
        Destroy(currentMessage.gameObject);
        if (curMessageSpam)
        {
            //Give Player Points
            ChangeMoneyAmt(1);
            spamRemoved++;
            trash.Play();
        }
        else
        {
            //Remove Player Points
            ChangeMishandledMsgsAmt(-1);
            error.Play();
        }
    }

    public void Release()
    {
        if (isOnTutorial)
        {
            EndTut();
        }

        Time.timeScale = 1f;
        emailGO.SetActive(false);
        currentMessage.StartScare();
    }

    void GameOver()
    {
        fired.Play();
        Time.timeScale = 0f;
        gameOverScreen.SetActive(true);
        gameOverText.text = "Spam Messages Caught: " + spamRemoved + "\nGood Messages Delivered: " + goodMsgsDelivered;
    }
  
}
