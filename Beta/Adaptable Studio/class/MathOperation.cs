namespace Adaptable_Studio
{
    public class MathOperation
    {
        /// <summary> 矩阵乘法 </summary> 
        /// <param name="matrix1">矩阵1</param> 
        /// <param name="matrix2">矩阵2</param> 
        /// <returns>积</returns> 
        public static double[][] MatrixMult(double[][] matrix1, double[][] matrix2)
        {
            //matrix1是m*n矩阵，matrix2是n*p矩阵，则result是m*p矩阵 
            int m = matrix1.Length, n = matrix2.Length, p = matrix2[0].Length;
            double[][] result = new double[m][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new double[p];
            }
            //矩阵乘法：c[i,j]=Sigma(k=1→n,a[i,k]*b[k,j]) 
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < p; j++)
                {
                    //对乘加法则 
                    for (int k = 0; k < n; k++)
                    {
                        result[i][j] += (matrix1[i][k] * matrix2[k][j]);
                    }
                }
            }
            return result;
        }

    }
}
