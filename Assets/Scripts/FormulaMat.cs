using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Assertions.Comparers;
using UnityEngine;


[System.Serializable]
public class Var
{
    public string name;
    public double value;

    public Var(string name)
    {
        this.name = name;
    }

    public Var(string name, double value)
    {
        this.name = name;
        this.value = value;
    }

    public override string ToString()
    {
        return name;
    }

    public static Term operator -(Var v)
    {
        return new Term(v, -1);
    }
}

[System.Serializable]
public class Term
{
    public int Sign { get; set; }
    public List<Var> Vars { get; set; }
    public double Value
    {
        get
        {
            double x = Sign;
            for (int i = 0; i < Vars.Count; i++)
            {
                x *= Vars[i].value;
            }
            return x;
        }
    }

    // Constructors
    public Term(int sign = 1)
    {
        this.Sign = sign;
        Vars = new List<Var>();
    }

    public Term(Var v, int sign = 1)
    {
        this.Sign = sign;
        Vars = new List<Var>();
        Vars.Add(v);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        if (Sign == -1)
            sb.Append("-");
        for (int i = 0; i < Vars.Count; i++)
            sb.Append(Vars[i]);
        if (Vars.Count == 0)
            sb.Append("1");
        return sb.ToString();
    }

    public static Term operator *(Term a, Term b)
    {
        Term c = new Term(a.Sign * b.Sign);
        c.Vars = a.Vars.Concat(b.Vars).ToList();
        return c;
    }

    public static implicit operator Term(Var v)
    {
        return new Term(v);
    }
}

public class Formula
{
    public List<Term> terms;

    public double Value
    {
        get
        {
            double sum = 0;
            for (int i = 0; i < terms.Count; i++)
            {
                sum += terms[i].Value;
            }
            return sum;
        }
    }

    public IEnumerable<Var> Vars
    {
        get
        {
            var vars = new HashSet<Var>();
            foreach (Term t in terms)
            {
                foreach (var v in t.Vars)
                {
                    vars.Add(v);
                }
            }
            return vars;
        }
    }

    // Constructors
    public Formula()
    {
        terms = new List<Term>();
    }

    public Formula(Term t)
    {
        terms = new List<Term>();
        terms.Add(t);
    }

    public Formula(Var t, int sign = 1)
    {
        terms = new List<Term>();
        terms.Add(new Term(t, sign));
    }


    public override string ToString()
    {
        if (terms.Count == 0)
            return "0";
        var sb = new StringBuilder();
        for (int i = 0; i < terms.Count; i++)
        {
            if (terms[i].Sign > 0 && i != 0)
                sb.Append("+");
            sb.Append(terms[i]);
        }
        return sb.ToString();
    }


    // Operators
    public static Formula operator *(Formula a, Formula b)
    {
        Formula c = new Formula();
        foreach (var t1 in a.terms)
        {
            foreach (var t2 in b.terms)
            {
                c.terms.Add(t1 * t2);
            }
        }
        return c;
    }

    public static Formula operator +(Formula a, Formula b)
    {
        Formula c = new Formula();
        c.terms = a.terms.Concat(b.terms).ToList();
        return c;
    }

    // Static Functions
    public static Formula Zero => new Formula();

    public static Formula One()
    {
        var f = new Formula();
        f.terms.Add(new Term());
        return f;
    }

    public static Formula NegOne()
    {
        var f = new Formula();
        f.terms.Add(new Term(-1));
        return f;
    }

    public static implicit operator Formula(Term t)
    {
        return new Formula(t);
    }

    public static implicit operator Formula(Var v)
    {
        return new Formula(v);
    }
}

public class FormulaMat
{
    public int n;
    public List<List<Formula>> entities;

    // Constructors
    public FormulaMat(int n = 4)
    {
        this.n = n;
        entities = new List<List<Formula>>();
        for (int i = 0; i < n; i++)
        {
            entities.Add(new List<Formula>());
            for (int j = 0; j < n; j++)
            {
                entities[i].Add(new Formula());
            }
        }
    }


    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("[");
        for (int i = 0; i < entities.Count; i++)
        {
            sb.Append("\t");
            var line = new List<string>();
            for (int j = 0; j < entities[i].Count; j++)
            {
                line.Add(entities[i][j].ToString());
            }
            sb.AppendLine(string.Join("\t", line));
        }
        sb.Append("]");
        return sb.ToString();
    }

    public string ToString(bool eval)
    {
        return eval ? ToStringEval() : ToString();
    }

    private string ToStringEval()
    {
        var sb = new StringBuilder();
        sb.AppendLine("[");
        for (int i = 0; i < entities.Count; i++)
        {
            sb.Append("\t");
            var line = new List<string>();
            for (int j = 0; j < entities[i].Count; j++)
            {
                line.Add(entities[i][j].Value.ToString());
            }
            sb.AppendLine(string.Join("\t", line));
        }
        sb.Append("]");
        return sb.ToString();
    }

    // Operators
    public List<Formula> this[int i]
    {
        get { return entities[i]; }
        set { entities[i] = value; }
    }

    public static FormulaMat operator *(FormulaMat a, FormulaMat b)
    {
        var c = new FormulaMat(a.n);
        for (int i = 0; i < a.n; i++)
        {
            for (int j = 0; j < a.n; j++)
            {
                for (int k = 0; k < a.n; k++)
                {
                    c[i][j] += a[i][k] * b[k][j];
                }
            }
        }
        return c;
    }

    public static Vector4 operator *(FormulaMat mat, Vector4 v)
    {
        double[] res = new double[4];
        for (int i = 0; i < 4; i++)
        {
            res[i] = 0;
            for (int j = 0; j < 4; j++)
            {
                res[i] += mat[i][j].Value * v[j];
            }
        }
        return new Vector4((float)res[0], (float)res[1], (float)res[2], (float)res[3]);
    }



    // Static Functions
    public static FormulaMat Identity(int n)
    {
        var m = new FormulaMat(n);
        for (int i = 0; i < n; i++)
        {
            m[i][i] = Formula.One();
        }
        return m;
    }
}

