using System;
using Random = UnityEngine.Random;

[Serializable]
public class Matrix {

    private float[,] matrix;

    public int columns { get; set; }
    public int rows { get; set; }

    public Matrix(int rows, int columns)
    {
        this.rows = rows;
        this.columns = columns;
        matrix = new float[rows, columns];
    }

    public Matrix(int rows, int columns, bool randData)
    {
        this.rows = rows;
        this.columns = columns;
        matrix = new float[rows, columns];

        if (randData)
        {

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    matrix[row, column] = Random.Range(-1f, 1f);
                }
            }
        }
    }

    public Matrix(int rows, int columns, float[] data)
    {
        this.rows = rows;
        this.columns = columns;
        matrix = new float[rows, columns];
        ApplyData(data);
    }

    public Matrix(float[,] data)
    {
        rows = data.GetLength(0);
        columns = data.GetLength(1);
        matrix = data;
    }

    public float this[int row, int column]
    {
        get { return matrix[row, column]; }
        set { matrix[row, column] = value; }
    }

    private void ApplyData(float[] data)
    {
        if (data.Length != (rows * columns)) throw new Exception("The given data array is not compatible with the matrix.");

        int i = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                matrix[row, column] = data[i];
                i++;
            }
        }
    }

    public float[] ToArray()
    {
        float[] array = new float[rows * columns];
        int i = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                array[i] = matrix[row, column];
                i++;
            }
        }

        return array;
    }

    public static Matrix operator +(Matrix a, Matrix b) 
    {
        if (a.rows != b.rows || a.columns != b.columns) throw new Exception("Can not add matrices of different sizes.");

        for (int row = 0; row < a.rows; row++)
        {
            for (int column = 0; column < a.columns; column++)
            {
                a[row, column] += b[row, column];
            }
        }

        return a;
    }

    public static Matrix operator -(Matrix a, Matrix b)
    {
        if (a.rows != b.rows || a.columns != b.columns) throw new System.Exception("Can not subtract matrices of different sizes.");

        for (int row = 0; row < a.rows; row++)
        {
            for (int column = 0; column < a.columns; column++)
            {
                a[row, column] -= b[row, column];
            }
        }

        return a;
    }

    public static Matrix operator *(int a, Matrix b)
    {
        for (int row = 0; row < b.rows; row++)
        {
            for (int column = 0; column < b.columns; column++)
            {
                b[row, column] *= a;
            }
        }

        return b;
    }

    public static Matrix operator *(float a, Matrix b)
    {
        for (int row = 0; row < b.rows; row++)
        {
            for (int column = 0; column < b.columns; column++)
            {
                b[row, column] *= a;
            }
        }

        return b;
    }

    public static Matrix operator *(Matrix a, Matrix b)
    {
        if (a.columns != b.rows) throw new Exception("Can not multiply matrices of incompatible sizes.");

        Matrix c = new Matrix(a.rows, b.columns);

        for (int row = 0; row < c.rows; row++)
        {
            for (int column = 0; column < c.columns; column++)
            {
                float dot = 0;
                for (int num = 0; num < a.columns; num++)
                {
                    dot += a[row, num] * b[num, column];
                }
                c[row, column] = dot;
            }
        }

        return c;
    }

    public override string ToString()
    {
        string result = "\n";

        for (int row = 0; row < rows; row++)
        {
            result += "[";
            for (int column = 0; column < columns; column++)
            {
                result += matrix[row, column] + ((column != columns - 1) ? "," : "");
            }
            result += "]\n";
        }

        return result;
    }
}
