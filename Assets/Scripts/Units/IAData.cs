using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAData : Manager<IAData>
{
    public int Money { get; private set; }
    public int level { get; private set; }
    public int exp { get; private set; }

    public int expNeeded { get; private set; }

    public System.Action OnUpdate;

    private void Start()
    {
        Money = 10;
        level = 1;
        exp = 0;
        expNeeded = 2;
    }
    public void setLevel(int level)
    {
        this.level = level;
    }
    public bool CanAfford(int amount)
    {
        return amount <= Money;
    }
    public void moneyEndRound()
    {
        Money += 10 + level;
        OnUpdate?.Invoke();
    }
    public void SpendMoney(int amount)
    {
        Money -= amount;
        OnUpdate?.Invoke();
    }
    public void UpdateExp()
    {
        exp += 2;
        if (exp == expNeeded)
        {
            level += 1;
            exp = 0;
        }
        switch (level)
        {
            case 2:
                expNeeded = 6;
                break;
            case 3:
                expNeeded = 10;
                break;
            case 4:
                expNeeded = 20;
                break;
            case 5:
                expNeeded = 36;
                break;
            default:
                expNeeded = 50;
                break;

        }



        OnUpdate?.Invoke();
    }
}