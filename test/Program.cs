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
    private const float followersPerReputation = 0f;
    private const float incomePerFollower = 0f;

    private List<Action> possible;

    private List<Event> events;

    private Dictionary<int, List<Action>> scheduled;

    public Simulation()
    {
        turn = 0;
        actions = new List<Action>();
        people = new List<Character>();

        // money, followers, belief, reputation
        resources = new Resources(100, 1, 0, 0);

        // List of possible actions.
        possible = new List<Action>();
        possible.Add(new Action(
            "Study theology",
            3,
            new Resources(0, 1, 0, 0),
            new Resources(0, -1, 0, 0),
            new Resources(0, 1, .1f, .1f),
            new Resources(0, 0, 0, 0)
        ));
        possible.Add(new Action(
            "Meditate",
            1,
            new Resources(0, 1, 0, 0), // requires nothing
            new Resources(0, -1, 0, 0), // costs nothing
            new Resources(0, 1, .1f, 0), // gives you some belief
            new Resources()
        ));
        possible.Add(new Action(
            "Door to door campaign",
            1,
            new Resources(20, 5, 0, .3f),
            new Resources(-20, -5, 0, 0),
            new Resources(0, 8, .1f, -.1f),
            new Resources()
        ));

        scheduled = new Dictionary<int, List<Action>>();
        events = new List<Event>();
    }

    public void Advance(int t)
    {
        turn += t;

        // Update followers and money amounts.
        resources.Followers = Math.Max(resources.Followers + t * followersPerReputation * resources.Reputation, 0f);
        resources.Money = Math.Max(resources.Money + t * incomePerFollower * resources.Followers, 0f);

        if (scheduled.ContainsKey(turn))
        {
            foreach (Action a in scheduled[turn]) {
                resources.Add(a.reward);
                Console.WriteLine(">>> \"" + a.Name + "\" succeeded!");
                Console.WriteLine(">>> Rewards: " + a.reward.Short());
            }
            scheduled.Remove(turn);
        }

        // Add whatever events we want.
        foreach (var e in GetEvents(turn, resources))
        {
            AddEvent(e);
        }
    }

    public void StartAction(Action a)
    {
        // Check that there are sufficient resources.
        if (resources.Money < a.prerequisites.Money) {
            Console.WriteLine(">>> Insufficient funds.");
            return;
        } else if (resources.Followers < a.prerequisites.Followers) {
            Console.WriteLine(">>> Insufficient followers.");
            return;
        } else if (resources.Belief < a.prerequisites.Belief) {
            Console.WriteLine(">>> Insufficient belief.");
            return;
        } else if (resources.Reputation < a.prerequisites.Reputation) {
            Console.WriteLine(">>> Insufficient reputation.");
            return;
        }
        // Schedule the action to finish.
        var end = turn + a.TurnCost;
        if (!scheduled.ContainsKey(end)) {
            scheduled.Add(end, new List<Action>());
        }
        scheduled[end].Add(a);
        resources.Add(a.cost);
    }

    public List<Action> PossibleActions()
    {
        return possible;
    }

    private void AddEvent(Event e)
    {
        // HACK Check the string name of the event to make sure it only happens once.
        foreach (var other in events)
        {
            if (e.Name == other.Name)
            {
                return;
            }
        }

        // Procure the event.
        resources.Add(e.Change);
        if (e.Recruit != null)
        {
            AddPerson(e.Recruit);
        }

        // Keep the event for historical records.
        events.Add(e);

    }

    private void AddPerson(Character c)
    {
        people.Add(c);
        foreach (var a in c.Actions)
        {
            a.Name += " (" + c.Name + ")";
            possible.Add(a);
        }
    }

    private List<Event> GetEvents(int t, Resources r)
    {
        var e = new List<Event>();
        if (r.Belief > .5f)
        {
            e.Add(new Event(t,
                "The conviction of your followers improves your public image.",
                new Resources(0, 0, 0, .05f)
            ));
        }
        if (r.Reputation > .1f)
        {
            e.Add(new Event(t,
                "People are interested in your ideals.",
                new Resources(0, 2, 0, 0)
            ));
        }
        if (r.Reputation > .4f)
        {
            e.Add(new Event(t,
                "Some newcomers have joined your group.",
                new Resources(0, 3, 0, 0)
            ));
        }
        if (t == 5)
        {
            e.Add(new Event(t,
                "Cathy McDonald joins your organization.",
                new Resources(0, 1, 0, 0),
                new Character("Cathy McDonald", "Writer", "Occult", new Action[]{
                    new Action("Publish a pamphlet", 3,
                           new Resources(10, 0, 0, 0),
                           new Resources(-10, 0, 0, 0),
                           new Resources(0, 3, .1f, 0),
                           new Resources(0, 0, 0, 0)
                    )
                })
            ));
        }
        if (t == 10)
        {
            e.Add(new Event(t,
                "A powerful politician denounces your organization.",
                new Resources(0, -10, -.1f, -.5f)
            ));
        }
        if (t == 15)
        {
            e.Add(new Event(t,
                "Arthur Sutherland joins your organization.",
                new Resources(0, 1, 0, 0),
                new Character("Arthur Sutherland", "Politician", "Sex", new Action[]{
                    new Action("Lobby for government funding", 3,
                           new Resources(100, 0, 0, .3f),
                           new Resources(-100, 0, 0, 0),
                           new Resources(1000, 0, .1f, .1f),
                           new Resources()
                    )
                })
            ));
        }
        return e;
    }

    public override string ToString()
    {
        string s = "";
        s += "=== Turn " + turn + " ===\n";
        s += "RESOURCES\n";
        s += resources.ToString();
        s += "ACTIONS\n";
        for (int i = 0; i < possible.Count; i++)
        {
            s += (i + 1) + ". " + possible[i].Name + "\n" + 
                          "  Takes " + possible[i].TurnCost + " turns" + "\n" +
                          "  Requires: " + possible[i].prerequisites.Short() + "\n" +
                          "  Costs:    " + possible[i].cost.Short() + "\n" +
                          "  Returns:  " + possible[i].reward.Short() + "\n";
        }
        if (scheduled.Count > 0)
        {
            s += "SCHEDULED\n";
            foreach (KeyValuePair<int, List<Action>> entry in scheduled)
            {
                s += "Finishes on turn " + entry.Key + ": " + "\n";
                foreach (Action a in entry.Value) {
                    s += "  " + a.Name + "\n";
                }
            }
        }
        if (people.Count > 0)
        {
            s += "PEOPLE\n";
            foreach (var person in people)
            {
                s += person.Name + ":" + person.Title + ":" + person.Likes + "\n";
            }

        }
        if (events.Count > 0)
        {
            s += "EVENTS\n";
            foreach (var e in events)
            {
                s += "Turn " + e.Turn + ": " + e.Name + "\n";
            }

        }

        return s;
    }
}

public class Action
{
    public string Name;
    public int TurnCost;

    public Resources prerequisites; // Check to know if action is possible.
    public Resources cost;          // Change in resources upon starting.
    public Resources reward;        // Change in resources upon success.
    public Resources penalty;       // Change in resources upon failure. (unused)


    public Action(string n, int t, Resources p, Resources c, Resources r, Resources l)
    {
        Name = n;
        TurnCost = t;
        prerequisites = p;
        cost = c;
        reward = r;
        penalty = l;
    }

    public override string ToString()
    {
        return Name;
    }
}

public class Resources
{
    public float Money;
    public float Followers;
    public float Belief;
    public float Reputation;

    public Resources() {
        Money = 0;
        Followers = 0;
        Belief = 0;
        Reputation = 0;
    }
    public Resources(float m, float f, float b, float r)
    {
        Money = m;
        Followers = f;
        Belief = b;
        Reputation = r;
    }

    public void Add(Resources other)
    {
        Money += other.Money;
        Followers += other.Followers;
        Belief += other.Belief;
        Reputation += other.Reputation;
    }

    public bool More(Resources other) {
        return Money > other.Money && 
            Followers > other.Followers &&
            Belief > other.Belief &&
            Reputation > other.Reputation;
    }

    public string Short() {
        string s = "";
        s += "$:" + Money + " ";
        s += "f:" + Followers + " ";
        s += "b:" + Belief + " ";
        s += "r:" + Reputation + " ";
        return s;
    }

    public override string ToString()
    {
        return
            "Money: " + Money + "\n" +
            "Followers: " + Followers + "\n" +
            "Belief: " + Belief + "\n" +
            "Reputation: " + Reputation + "\n";
    }
}

public class Event
{
    public int Turn;
    public string Name;
    public Resources Change;
    public Character Recruit;

    public Event(int t, string n, Resources c)
    {
        Turn = t;
        Name = n;
        Change = c;
        Recruit = null;
    }
    public Event(int t, string n, Resources r, Character c)
    {
        Turn = t;
        Name = n;
        Change = r;
        Recruit = c;
    }
}

public class Character
{
    public string Name;
    public string Title;
    public string Likes;
    public Action[] Actions;

    public Character(string n, string t, string l, Action[] a)
    {
        Name = n;
        Title = t;
        Likes = l;
        Actions = a;
    }
}


class Program
{
    static void Main()
    {
        var sim = new Simulation();
        while (true)
        {
            Console.WriteLine(sim);
            Console.Write("> ");
            string input = Console.ReadLine();
            int n;
            bool isNumeric = int.TryParse(input, out n);

            if (input == "q") {
                break;
            }
            if (isNumeric) {
                sim.StartAction(sim.PossibleActions()[n-1]);
            }
            if (input == "") {
                sim.Advance(1);
            }
                
        }
    }
}
