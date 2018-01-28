using System.Collections;
using System.Collections.Generic;
using System;

// TODO
// Characters enable certain actions. (2 hr)
// Events can occur.
// Characters join your party.


public class Simulation
{
    private int turn;

    private List<Action> actions;
    private List<Character> people;

    private Resources resources;

    // Per tick increments for followers and money.
    private const float followersPerReputation = 12.5f;
    private const float incomePerFollower = 1.5f;

    private List<Action> possible;

    private Dictionary<int, Action> scheduled;

    public Simulation()
    {
        turn = 0;
        actions = new List<Action>();
        people = new List<Character>();

        // money, followers, belief, reputation
        resources = new Resources(100, 1, -0.5f, 0.5f);

        // List of possible actions.
        possible = new List<Action>();
        possible.Add(new Action(
            "Publish a pamphlet",
            3,
            new Resources(50, 0, 0, 0), 
            new Resources(-50, 0, 0, 0),
            new Resources(0, 0, 0, .1f),
            new Resources(0, 0, 0, 0)
        ));
        possible.Add(new Action(
            "Human sacrifice",
            1,
            new Resources(10, 1, .7f, 0),
            new Resources(-10, -1, 0, 0),
            new Resources(0, 0, .1f, -.5f),
            new Resources(0, 1, -.1f, -.5f)
        ));

        scheduled = new Dictionary<int, Action>();
    }

    public void Advance(int t)
    {
        turn += t;

        // Update followers and money amounts.
        resources.Followers = Math.Max(resources.Followers + t * followersPerReputation * resources.Reputation, 0f);
        resources.Money = Math.Max(resources.Money + t * incomePerFollower * resources.Followers, 0f);

        if (scheduled.ContainsKey(turn)) {
            resources.Add(scheduled[turn].Succeed());
            Console.WriteLine(">>> " + scheduled[turn] + " succeeded!");
            Console.WriteLine("Rewards: ");

            Console.WriteLine(scheduled[turn].Succeed());
            scheduled.Remove(turn);
        }
    }

    public void StartAction(Action a) {
        scheduled.Add(turn + a.TurnCost, a);
        resources.Add(a.Start());
    }

    public List<Action> PossibleActions() {
        return possible;
    }

    public override string ToString() {
        string s = "";
        s += "=== Turn " + turn + " ===\n";
        s += "RESOURCES\n";
        s += resources.ToString();
        s += "ACTIONS\n";
        for (int i = 0; i < possible.Count; i++) {
            s += (i+1) + ". " + possible[i].Name + "\n";
        }
        if (scheduled.Count > 0) {
            s += "SCHEDULED\n";
            foreach (KeyValuePair<int, Action> entry in scheduled)
            {
                s += "Finishes on turn " + entry.Key + ": " + entry.Value + "\n";
            }
        }

        return s;
    }
}

public class Action
{
    public string Name;
    public int TurnCost;

    private Resources prerequisites; // Check to know if action is possible.
    private Resources cost;          // Change in resources upon starting.
    private Resources reward;        // Change in resources upon success.
    private Resources penalty;       // Change in resources upon failure.


    public Action (string n, int t, Resources p, Resources c, Resources r, Resources l) {
        Name = n;
        TurnCost = t;
        prerequisites = p;
        cost = c;
        reward = r;
        penalty = l;
    }

    public Resources Start() {
        return cost;
    }

    public Resources Succeed() {
        return reward;
    }

    public Resources Fail() {
        return penalty;
    }

    public override string ToString() {
        return Name;
    }
}

public class Resources
{
    public float Money;
    public float Followers;
    public float Belief;
    public float Reputation;

    public Resources (float m, float f, float b, float r) {
        Money = m;
        Followers = f;
        Belief = b;
        Reputation = r;
    }

    public void Add(Resources other) {
        Money += other.Money;
        Followers += other.Followers;
        Belief += other.Belief;
        Reputation += other.Reputation;
    }

    public override string ToString() {
        return
            "Money: " + Money + "\n" +
            "Followers: " + Followers + "\n" +
            "Belief: " + Belief + "\n" +
            "Reputation: " + Reputation + "\n";
    }
}

public class Event {
    
}

class Program
{
    static void Main()
    {
        var sim = new Simulation();
        while (true)
        {
            Console.WriteLine(sim);

            var key = Console.ReadKey().Key;
            if (key == ConsoleKey.D1) {
                sim.StartAction(sim.PossibleActions()[0]);
            } else if (key == ConsoleKey.D2) {
                sim.StartAction(sim.PossibleActions()[1]);
            } else if (key == ConsoleKey.Q) {
                break;
            } else {
                sim.Advance(1);
            }
        }
    }
}