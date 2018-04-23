using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FishermanController : MonoBehaviour {

    public FishermanState currentState;
    public InputValue currentInput;
    public Material startingMat, flashingMat;
    public bool canGoThere = false;
    public GameObject fishingRod;
    public EmailController attractedEmail;

    MeshRenderer myMR;
    NavMeshAgent myNMA;
    LineRenderer myLR;
    Vector3 vecToStream;

    void Awake()
    {
        myLR = GetComponent<LineRenderer>();
        myNMA = GetComponent<NavMeshAgent>();
        myMR = GetComponent<MeshRenderer>();
    }

    void Start () {
        currentState = FishermanState.JustSpawned;
    }
	
	void Update()
    {

        HandleInput();

        switch (currentState)
        {

            case FishermanState.BeingPlaced:
                transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) 
                                   - new Vector3(0, 4, Camera.main.transform.position.z);
                // This places the tower on the map
                if (currentInput == InputValue.singleClick)
                {
                    currentInput = InputValue.none;
                    currentState = FishermanState.Idle;
                    myNMA.enabled = true;
                    //myNMA.SetDestination(new Vector3(19, 0, 6));
                }
                break;

            case FishermanState.Fishing:
                NavMeshPath thePath = new NavMeshPath();
                canGoThere = myNMA.CalculatePath(Camera.main.ScreenToWorldPoint(Input.mousePosition)
                                   - new Vector3(0, 4, Camera.main.transform.position.z), thePath);
                if(currentInput == InputValue.singleClick && GameController.instance.canPullRod)
                {
                    currentInput = InputValue.none;
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if(Physics.Raycast(ray, out hit, 20f, 2048) && hit.transform.gameObject == gameObject)
                    {
                        currentState = FishermanState.LineOut;
                        CastRod(false);
                    }
                }
                break;

            case FishermanState.LineOut:
                break;

            case FishermanState.Walking:
                if (IsAtDestination())
                {
                    currentState = FishermanState.Fishing;
                }
                DrawMovementPath();
                break;

            case FishermanState.Idle:
                GoToStream();
                break;

            case FishermanState.JustSpawned:
                currentState = FishermanState.BeingPlaced;
                currentInput = InputValue.none;
                break;
        }
    }

    void HandleInput()
    {
        if(currentInput == InputValue.singleClick)
        {
            currentInput = InputValue.none;
        }
        if (Input.GetButtonDown("Fire1"))
        {
            currentInput = InputValue.singleClick;
        }
    }

    void DrawMovementPath()
    {
        if (myNMA.path.corners.Length < 2)
        {
            return;
        }

        //myLR.SetVertexCount(myNMA.path.corners.Length);
        myLR.positionCount = myNMA.path.corners.Length;

        for (int i = 0; i < myNMA.path.corners.Length; i++)
        {
            myLR.SetPosition(i, myNMA.path.corners[i]);
        }
    }

    void GoToStream()
    {
        //Debug.Log("going to stream");
        currentState = FishermanState.Walking;
        CastRod(false);
        NavMeshHit nmh;

        if (myNMA.FindClosestEdge(out nmh))
        {
            myNMA.SetDestination(nmh.position);
            vecToStream = GameController.ClosestStreamPos(myNMA.destination);
        }
        else
        {
            Debug.LogError("Could not find the edge closest to the NavMeshAgent on a fisherman!");
        }
    }

    bool IsAtDestination()
    {
        if(myNMA.remainingDistance > 0.01)
        {
            return false;
        }
        else
        {
            myNMA.isStopped = true;

            transform.LookAt(vecToStream, Vector3.up);
            CastRod(true);
            if (GameController.instance.isOnTutorial)
            {
                
                //GameController.instance.Invoke("SpawnTutEmail", 5f);
                GameController.instance.SpawnTutEmail();
            }
            return true;
        }
    }

    void CastRod(bool rodOut)
    {
        //Debug.Log("Casting Rod " + rodOut.ToString());
        myMR.material = startingMat;
        fishingRod.SetActive(rodOut);
        if (!rodOut && currentState == FishermanState.LineOut)
        {
            if (GameController.instance.isOnTutorial)
            {
                GameController.instance.ShowHowToDelMsg();
            }
            StartCoroutine(DelayCastRod(1.5f));
            //Catch or scare fish
            if (attractedEmail != null)
            {
                attractedEmail.CatchOrScare();
                attractedEmail = null;
            }
        }
        else if(currentState == FishermanState.LineOut) {
            currentState = FishermanState.Fishing;
        }
    }

    IEnumerator DelayCastRod(float delay)
    {
        yield return new WaitForSeconds(delay);
        CastRod(true);
    }

    public IEnumerator FlashColors()
    {
        while(attractedEmail != null && attractedEmail.currentState == EmailState.biting)
        {
            myMR.material = flashingMat;
            yield return new WaitForSeconds(0.1f);
            myMR.material = startingMat;
            yield return new WaitForSeconds(0.1f);
        }
    }
}

public enum FishermanState { BeingPlaced, Walking, Fishing, LineOut, Idle, JustSpawned }

public enum InputValue { singleClick, doubleClick, none}