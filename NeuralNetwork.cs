using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class NeuralNetwork {

    private Matrix[] layers;
    private Matrix[] bias;
    private Matrix[] weights;

    private Matrix[] dweights;
    private Matrix[] dbias;

    public NeuralNetwork(int input, int output, int[] hidden)
    {
        layers = new Matrix[hidden.Length + 2];
        bias = new Matrix[hidden.Length + 1];
        weights = new Matrix[hidden.Length + 1];

        dweights = new Matrix[weights.Length];
        dbias = new Matrix[bias.Length];

        layers[0] = new Matrix(input, 1);

        for (int i = 0; i < hidden.Length; i++)
        {
            layers[i + 1] = new Matrix(hidden[i], 1);
        }

        layers[hidden.Length + 1] = new Matrix(output, 1);

        for (int i = 1; i < layers.Length; i++)
        {
            bias[i - 1] = new Matrix(layers[i].rows, 1, true);
            weights[i - 1] = new Matrix(layers[i].rows, layers[i - 1].rows, true);
            dbias[i - 1] = new Matrix(layers[i].rows, 1);
            dweights[i - 1] = new Matrix(layers[i].rows, layers[i - 1].rows);
        }
    }

    public void SaveNetwork(string path)
    {
        FileStream fs = new FileStream(path, FileMode.Create);

        BinaryFormatter bf = new BinaryFormatter();

        bf.Serialize(fs, this);
        fs.Close();
    }

    public void LoadNetwork(string path)
    {
        try
        {
            FileStream fs = new FileStream(path, FileMode.Open);

            BinaryFormatter bf = new BinaryFormatter();

            NeuralNetwork loaded = (NeuralNetwork)bf.Deserialize(fs);

            ApplyLoadedWeights(loaded.GetWeights());
            ApplyLoadedBias(loaded.GetBias());
            fs.Close();
        } catch 
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            fs.Close();
        } 
    }

    public Matrix[] GetWeights() { return weights; }

    public Matrix[] GetBias() { return bias; }

    private void ApplyLoadedWeights(Matrix[] weights) { this.weights = weights; }

    private void ApplyLoadedBias(Matrix[] bias) { this.bias = bias; }

    public Matrix ProcessInput(float[] input)
    {
        layers[0] = new Matrix(layers[0].rows, layers[0].columns, input);

        for (int layer = 1; layer < layers.Length; layer++)
        {
            layers[layer] = Sigmoid((weights[layer - 1] * layers[layer - 1]) + bias[layer - 1]);
        }

        return layers[layers.Length - 1];
    }

    private Matrix Sigmoid(Matrix matrix)
    {
        for (int row = 0; row < matrix.rows; row++)
        {
            matrix[row, 0] = (float)(1.0 / (1.0 + Math.Pow(Math.E, -matrix[row, 0])));
        }

        return matrix;
    }

    private Matrix dSigmoid(Matrix matrix)
    {
        for (int row = 0; row < matrix.rows; row++)
        {
            matrix[row, 0] = (float)(1.0 / (4.0 * (Math.Pow(Math.Cosh(matrix[row, 0]), 2))));
        }

        return matrix;
    }

    public void ProcessOutput(Matrix dCost)
    {
        Matrix dZ = dSigmoid(weights[weights.Length - 1] * layers[layers.Length - 2] + bias[bias.Length - 1]);
        weights[weights.Length - 1] += ComputeWeights(layers[layers.Length - 2], layers[layers.Length - 1], dZ, dCost);
        bias[bias.Length - 1] += ComputeBias(layers[layers.Length - 1], dZ, dCost);
        Matrix dcost = ComputeCost(weights[weights.Length - 1], layers[layers.Length - 2], layers[layers.Length - 1], dZ, dCost);

        for (int i = 2; i < weights.Length; i++)
        {
            Matrix dz = dSigmoid(weights[weights.Length - i] * layers[layers.Length - (i + 1)] + bias[bias.Length - i]);
            weights[weights.Length - i] += ComputeWeights(layers[layers.Length - (i + 1)], layers[layers.Length - i], dz, dcost);
            bias[bias.Length - i] += ComputeBias(layers[layers.Length - i], dz, dcost);
            dcost = ComputeCost(weights[weights.Length - i], layers[layers.Length - (i + 1)], layers[layers.Length - i], dz, dcost);
        }
    }

    public void UpdateNetwork()
    {
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] += dweights[i];
            bias[i] += dbias[i];
            dweights[i] = 0 * dweights[i];
            dbias[i] = 0 * dbias[i];
        }
        
    }

    private Matrix ComputeWeights(Matrix layerA, Matrix layerB, Matrix dz, Matrix dcost)
    {
        Matrix dweights = new Matrix(layerB.rows, layerA.rows);

        for (int rowA = 0; rowA < layerA.rows; rowA++)
        {
            for (int rowB = 0; rowB < layerB.rows; rowB++)
            {
                dweights[rowB, rowA] = layerA[rowA, 0] * dz[rowB, 0] * dcost[rowB, 0]; 
            }
        }

        return dweights;
    }

    private Matrix ComputeBias(Matrix layerA, Matrix dz, Matrix dcost)
    {
        Matrix dbias = new Matrix(layerA.rows, 1);

        for (int rowA = 0; rowA < layerA.rows; rowA++)
        {
            dbias[rowA, 0] = dz[rowA, 0] * dcost[rowA, 0];
        }

        return dbias;
    }

    private Matrix ComputeCost(Matrix weight, Matrix layerA, Matrix layerB, Matrix dz, Matrix dcost)
    {
        Matrix dCost = new Matrix(layerA.rows, 1);

        for (int rowA = 0; rowA < layerA.rows; rowA++)
        {
            for (int rowB = 0; rowB < layerB.rows; rowB++)
            {
                dCost[rowA, 0] = ((weight[rowB, 0] * dz[rowB, 0] * dcost[rowB, 0]) - layerA[rowA, 0]);
            }
        }

        return dCost;
    }

    public override string ToString()
    {
        string net = "Network:\n";

        net += "\tLayers:\n";
        foreach (Matrix layer in layers)
        {
            net += "\t\t" + layer.rows + "," + layer.columns + "\n";
        }

        net += "\tBias:\n";
        foreach (Matrix bia in bias)
        {
            net += "\t\t" + bia.rows + "," + bia.columns + "\n";
        }

        net += "\tWeights:\n";
        foreach (Matrix weight in weights)
        {
            net += "\t\t" + weight.rows + "," + weight.columns + "\n";
        }

        return net;
    }
}
