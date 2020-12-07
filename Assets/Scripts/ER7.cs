using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ER7 : MonoBehaviour
{
    public double[] theta = new double[6];
    private const double scale = 0.01;
    public Var c1 = new Var("c1");
    public Var s1 = new Var("s1");
    public Var c2 = new Var("c2");
    public Var s2 = new Var("s2");
    public Var c3 = new Var("c3");
    public Var s3 = new Var("s3");
    public Var c4 = new Var("c4");
    public Var s4 = new Var("s4");
    public Var c5 = new Var("c5");
    public Var s5 = new Var("s5");
    public Var d1 = new Var("d_1", 358.5);
    public Var d5 = new Var("d_5", 251);
    public Var a1 = new Var("a_1", 50);
    public Var a2 = new Var("a_2", 300);
    public Var a3 = new Var("a_3", 350);
    public Transform[] joints = new Transform[6];

    private FormulaMat T10;
    private FormulaMat T21;
    private FormulaMat T32;
    private FormulaMat T43;
    private FormulaMat T54;
    private FormulaMat[] T;

    void Start()
    {
        T10 = FormulaMat.Identity(4);
        T10[0][0] = c1; T10[0][1] = -s1;
        T10[1][0] = s1; T10[1][1] = c1;
        T10[2][3] = d1;

        T21 = new FormulaMat(4);
        T21[0][0] = c2; T21[0][1] = -s2;    T21[0][3] = a1;
        T21[1][2] = Formula.NegOne();
        T21[2][0] = s2; T21[2][1] = c2;
        T21[3][3] = Formula.One();

        T32 = FormulaMat.Identity(4);
        T32[0][0] = c3; T32[0][1] = -s3; T32[0][3] = a2;
        T32[1][0] = s3; T32[1][1] = c3;

        T43 = FormulaMat.Identity(4);
        T43[0][0] = c4; T43[0][1] = -s4; T43[0][3] = a3;
        T43[1][0] = s4; T43[1][1] = c4;

        T54 = new FormulaMat(4);
        T54[0][0] = c5; T54[0][1] = -s5;
        T54[1][2] = Formula.NegOne(); T54[1][3] = -d5;
        T54[2][0] = s5; T54[2][1] = c5;
        T54[3][3] = Formula.One();

        T = new FormulaMat[6];
        T[0] = FormulaMat.Identity(4);
        T[1] = T[0] * T10;
        T[2] = T[1] * T21;
        T[3] = T[2] * T32;
        T[4] = T[3] * T43;
        T[5] = T[4] * T54;

        Debug.Log($"T10: {T10}");
        Debug.Log($"T21: {T21}");
        Debug.Log($"T32: {T32}");
        Debug.Log($"T43: {T43}");
        Debug.Log($"T54: {T54}");
        Debug.Log($"T10*T21: {T10 * T21}");
        Debug.Log($"T50: {T[5]}");
        
        Debug.Log($"T10: {ToLatex(T10)}");
        Debug.Log($"T21: {ToLatex(T21)}");
        Debug.Log($"T32: {ToLatex(T32)}");
        Debug.Log($"T43: {ToLatex(T43)}");
        Debug.Log($"T54: {ToLatex(T54)}");

        for (int i = 0; i <= 5; i++)
        {
            Debug.Log($"T{i}0: {T[i]}");
        }

        FormulaMat T51 = T21 * T32 * T43 * T54;
        FormulaMat T52 = T32 * T43 * T54;
        FormulaMat T53 = T43 * T54;
        Debug.Log($"T51: {T51}");
        Debug.Log($"T52: {T52}");
        Debug.Log($"T53: {T53}");
    }

    void Update()
    {
        c1.value = Math.Cos(theta[1]);
        s1.value = Math.Sin(theta[1]);
        c2.value = Math.Cos(theta[2]);
        s2.value = Math.Sin(theta[2]);
        c3.value = Math.Cos(theta[3]);
        s3.value = Math.Sin(theta[3]);
        c4.value = Math.Cos(theta[4]);
        s4.value = Math.Sin(theta[4]);
        c5.value = Math.Cos(theta[5]);
        s5.value = Math.Sin(theta[5]);
        Vector4 xUnit = new Vector4(1, 0, 0);
        Vector4 yUnit = new Vector4(0, 1, 0);
        Vector4 zUnit = new Vector4(0, 0, 1);
        Vector4 origin = new Vector4(0, 0, 0, 1);

        for (int i = 0; i < 6; i++)
        {
            Vector4 p = ToLeftHand(T[i] * origin);

            joints[i].rotation = Quaternion.LookRotation(ToLeftHand(T[i] * yUnit), ToLeftHand(T[i] * zUnit));

            joints[i].localPosition = p*(float)scale;
        }
    }


    Vector4 ToLeftHand(Vector4 v)
    {
        return new Vector4(v.x, v.z, v.y, v.w);
    }


    private string ToLatex(FormulaMat m)
    {
        var sb = new StringBuilder();
        sb.AppendLine("\\begin{bmatrix}");
        for (int i = 0; i < m.n; i++)
        {
            var line = new List<string>();
            for (int j = 0; j < m.n; j++)
            {
                line.Add(m[i][j].ToString());
            }
            sb.Append(string.Join("&", line));
            sb.Append("\\\\");
        }
        sb.Append("\\end{bmatrix}");
        return sb.ToString();
    }
}
