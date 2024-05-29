using Hwdtech;

namespace SpaceBattle.Lib;

public class PosIterator : IEnumerator<object>
{
    private readonly List<int> teams;
    private readonly int innerSpace;
    private readonly int outerSpace;
    private int counter = 1;
    private int teamSize;
    private int currentTeam = 0;
    private Vector startingPoint;
    public PosIterator(List<int> teams, int innerSpace, int outerSpace)
    {
        this.teams = teams;
        this.innerSpace = innerSpace;
        this.outerSpace = outerSpace;
        teamSize = teams[0];
        startingPoint = IoC.Resolve<Vector>("Services.GetStartingPoint");
    }

    public object Current
    {
        get
        {
            var buf = startingPoint + new Vector(0, innerSpace * counter);
            counter++;
            return buf;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public bool MoveNext()
    {
        if (counter <= teamSize)
        {
            return true;
        }
        else
        {
            currentTeam++;
            if (currentTeam < teams.Count)
            {
                startingPoint += new Vector(outerSpace, 0);
                counter = 1;
                teamSize = teams[currentTeam];
                return true;
            }
        }

        return false;
    }

    public void Reset()
    {
        startingPoint = IoC.Resolve<Vector>("Services.GetStartingPoint");
        currentTeam = 0;
        counter = 1;
    }
}
