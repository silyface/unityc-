using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public CharacterStats playerStates;
    List<IEndGameObserver> endGameObservers = new List<IEndGameObserver>();

    public void  RigisterPlayerr(CharacterStats player)
    {
        playerStates =player;
    }

     //将敌人加入和移除广播名单
    public void AddObserver(IEndGameObserver observer)
    {
        endGameObservers.Add(observer);
    }

    public void RemoveObserver(IEndGameObserver observer)
    {
        endGameObservers.Remove(observer);
    }

    public void NotifyObservers()
    {
        foreach (var observer in endGameObservers)
        {
            observer.EndNotify();
        }
    }
}
