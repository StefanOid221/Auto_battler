using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BaseUnit : MonoBehaviour
{
    //public GameObject gameObject;
    public Animator animator;


    public double baseDamage = 1;
    public int level = 1;
    public float baseHealth = 3;
    public float baseDefaulthealth;
    public float cost;
    public int unitType;
    [Range(1, 5)]
    public int range = 1;
    public double attackSpeed = 1f; //Attacks per second
    public float movementSpeed = 1f; //Attacks per second

    public bool moving;
    public bool isBenched = true;
    protected Node destination;

    public Team myTeam;
    protected Node currentNode;
    protected BaseUnit currentTarget;

    public Tile previousFightTile;

    public Node CurrentNode => currentNode;
    
    protected bool inRange => currentTarget != null && Vector3.Distance(this.transform.position, currentTarget.transform.position) <= range;

    protected bool hasEnemy => currentTarget != null;

    protected bool canAttack = true;
    protected bool dead = false;
    protected float waitBetweenAttack;

    public void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetTrigger("Idle");
        baseDefaulthealth = baseHealth;
    }
    public void Setup(Team team, Node spawnNode)
    {
        myTeam = team;
        this.currentNode = spawnNode;
        transform.position = currentNode.worldPosition;
        currentNode.SetOccupied(true);


    }
    protected void FindTarget()
    {
        var allEnemies = GameManager.Instance.GetUnitsAgainst(myTeam);
        float minDistance = Mathf.Infinity;
        BaseUnit entity = null;
        foreach (BaseUnit e in allEnemies)
        {
            if (Vector3.Distance(e.transform.position, this.transform.position) <= minDistance && e.isActiveAndEnabled)
            {
                minDistance = Vector3.Distance(e.transform.position, this.transform.position);
                entity = e;
            }
        }

        currentTarget = entity;
    }

    protected void GetInRange()
    {
        if (!isBenched)
        {

            if (currentTarget == null) {
                animator.SetTrigger("Idle");
                return;
            }
            if (!moving)
            {

                destination = null;
                List<Node> candidates = GridManager.Instance.GetNodesCloseTo(currentTarget.currentNode);
                candidates = candidates.OrderBy(x => Vector3.Distance(x.worldPosition, this.transform.position)).ToList();
                for (int i = 0; i < candidates.Count; i++)
                {


                    if (!candidates[i].IsOccupied)
                    {


                        destination = candidates[i];
                        break;
                    }
                }
                if (destination == null) {
                    animator.SetTrigger("Idle");
                    return;
                }



                var path = GridManager.Instance.GetPath(currentNode, destination);
                if (path == null && path.Count >= 1)
                    return;

                if (path[1].IsOccupied)
                    return;

                path[1].SetOccupied(true);
                destination = path[1];
            }


            moving = !MoveTowards();

            if (!moving)
            {


                currentNode.SetOccupied(false);
                currentNode = destination;
            }
        }
    }

    public void TakeDamage(double amount)
    {
        baseHealth -= (float)amount;

        if (baseHealth <= 0 && !dead)
        {
            dead = true;
            currentNode.SetOccupied(false);
            GameManager.Instance.UnitDead(this);
        }
    }
    protected virtual void Attack()
    {
        if (!canAttack)
            return;

        this.transform.LookAt(currentTarget.currentNode.worldPosition);
        animator.SetTrigger("Attacking");

        waitBetweenAttack = (float)(1 / attackSpeed);
        StartCoroutine(WaitCoroutine());
    }

    IEnumerator WaitCoroutine()
    {
        canAttack = false;
        yield return null;
        animator.ResetTrigger("Attacking");
        yield return new WaitForSeconds(waitBetweenAttack);
        canAttack = true;
    }

    public void SetCurrentNode(Node node)
    {
        currentNode = node;
    }



    protected bool MoveTowards()
    {
        
        Vector3 direction = destination.worldPosition - this.transform.position;
        if (direction.sqrMagnitude <= 0.005f)
        {
            transform.position = destination.worldPosition;
            animator.SetTrigger("Idle");
            return true;
        }
        animator.SetTrigger("Running");
        this.transform.position += direction.normalized * movementSpeed * Time.deltaTime;
        this.transform.LookAt(destination.worldPosition);
        return false;
    }
    protected virtual void Update()
    {
        if (this.isBenched)
            animator.SetTrigger("Idle");
        if (currentTarget != null) {
            if (!currentTarget.gameObject.activeSelf)
            {

                FindTarget();
            }
        }
    }
    public void respawn()
    {
        //if (this.dead)
        //{
            this.currentNode.SetOccupied(false);
            this.gameObject.SetActive(true);
            this.transform.position = previousFightTile.transform.position;
            this.Setup(myTeam, GridManager.Instance.GetNodeForTile(previousFightTile));
            this.baseHealth = baseDefaulthealth;
            this.dead = false;
            animator.SetTrigger("Idle");
        //}

    }
    public void levelUp()
    {
        this.level += 1;
        this.baseDamage *= 1.5;
        this.baseDefaulthealth *= 2;
        this.baseHealth = baseDefaulthealth;
        this.attackSpeed *= 1.2;
        if (this.level == 2)
        {
            this.gameObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            if (myTeam == Team.Team1)
                GameManager.Instance.checkLevelUp(this, Player.Player);
            else GameManager.Instance.checkLevelUp(this, Player.IA_Player);
        }
        if (this.level == 3)
            this.gameObject.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        this.transform.position = new Vector3(this.transform.position.x, 0, this.transform.position.z);
    }
    public void levelUpTrain()
    {
        this.level += 1;
        this.baseDamage *= 1.5;
        this.baseDefaulthealth *= 2;
        this.baseHealth = baseDefaulthealth;
        this.attackSpeed *= 1.2;
        if (this.level == 2)
        {
            this.gameObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        }
        if (this.level == 3)
            this.gameObject.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        this.transform.position = new Vector3(this.transform.position.x, 0, this.transform.position.z);
    }
    public void moveToNode ( Node spawnNode)
    {
        Tile spawnTile = GridManager.Instance.GetTileForNode(this.currentNode);
        this.previousFightTile = spawnTile;
        this.currentNode.SetOccupied(false);
        this.currentNode = spawnNode;
        transform.position = currentNode.worldPosition;
        currentNode.SetOccupied(true);
        Tile tile = GridManager.Instance.GetTileForNode(spawnNode);
        this.isBenched = false;
        this.moving = false;
        if (tile.isBench)
        {
            this.isBenched = true;
            if (this.myTeam == Team.Team2 &&  !GameManager.Instance.team2BenchUnits.Contains(this))
            {
                GameManager.Instance.team2BoardUnits.Remove(this);
                GameManager.Instance.team2BenchUnits.Add(this);
                GameManager.Instance.team2CopyBoardUnits.Remove(this);
                Debug.Log("a�adido desde moveToNode");
            }
        }
        else if (!GameManager.Instance.team2BoardUnits.Contains(this))
        {
            GameManager.Instance.team2BoardUnits.Add(this);
            GameManager.Instance.team2BenchUnits.Remove(this);
        }

    }
}
