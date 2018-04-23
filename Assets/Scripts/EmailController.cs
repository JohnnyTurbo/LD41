using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EmailController : MonoBehaviour {

    public Transform endPoint;
    public NavMeshAgent myNMA;
    public FishermanController rodILike;
    public EmailState currentState;
    public float swimSpeed, scareSpeed, attractedSpeed, scareDuration, minBiteTime, maxBiteTime, biteDuration;
    public bool isSpam;

    void Start()
    {
        endPoint = GameController.instance.endPoint.transform;
        myNMA.SetDestination(endPoint.position);
        myNMA.speed = swimSpeed;
        isSpam = (Random.value > 0.5f) ? true : false;
        if (GameController.instance.isOnTutorial)
        {
            isSpam = true;
        }
        //Debug.Log("isSpam: " + isSpam.ToString());
    }

    void Update()
    {
        if(Vector3.Distance(transform.position, endPoint.position) <= 0.1f)
        {
            EndReached();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log(Vector3.Distance(transform.position, endPoint.position).ToString());
        }
    }

    public void CatchOrScare()
    {
        //Debug.Log("CatchOrScare!");
        switch (currentState)
        {
            case EmailState.biting:
                //Email caught
                GameController.instance.ShowMessageContents(this);
                break;

            case EmailState.attracted:
                //Email scared off
                StartScare();
                break;

            default:
                Debug.LogError("Catch or Scare called in undefined state!");
                break;
        }
    }

    void EndReached()
    {
        if (isSpam)
        {
            //Remove Player Points
            GameController.instance.error.Play();
            GameController.instance.ChangeMishandledMsgsAmt(-1);
        }
        else
        {
            //Give Player Points
            GameController.instance.mail.Play();
            GameController.instance.ChangeMoneyAmt(1);
            GameController.instance.goodMsgsDelivered++;
        }
        Destroy(gameObject);
    }

    public void StartScare()
    {
        //Debug.Log("StartScare()");
        currentState = EmailState.scared;
        rodILike.attractedEmail = null;
        transform.localScale = Vector3.one;
        myNMA.speed = scareSpeed;
        myNMA.destination = endPoint.position;
        Invoke("StopScare", scareDuration);
    }

    void StopScare()
    {
        currentState = EmailState.swimming;
        myNMA.speed = swimSpeed;
    }

    IEnumerator Bite()
    {
        float timeToBite = Random.Range(minBiteTime, maxBiteTime);
        yield return new WaitForSeconds(timeToBite);

        if(currentState == EmailState.attracted)
        {
            if(GameController.instance.isOnTutorial && GameController.instance.canBeBitten)
            {
                GameController.instance.ShowHowToCatch();
                biteDuration = 10000f;
            }
            //Debug.Log("Biting");
            currentState = EmailState.biting;
            StartCoroutine(rodILike.FlashColors());
            yield return new WaitForSeconds(biteDuration);
            //Debug.Log("Done Biting");
            if (currentState == EmailState.biting)
            {
                StartScare();
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        
        if (col.gameObject.tag == "Bait" && currentState == EmailState.swimming)
        {
            rodILike = col.transform.parent.parent.GetComponent<FishermanController>();
            if (rodILike != null && rodILike.attractedEmail == null)
            {
                //Debug.Log("Im baitted!");
                currentState = EmailState.attracted;
                rodILike.attractedEmail = this;
                myNMA.SetDestination(col.transform.position);
                myNMA.speed = attractedSpeed;
                transform.localScale = new Vector3(1, 1.25f, 1);
                StartCoroutine(Bite());
            }
            else
            {
                rodILike = null;
            }
        }
    }
}

public enum EmailState { swimming, attracted, biting, scared } 
