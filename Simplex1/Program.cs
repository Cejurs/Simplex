using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplex1
{
    public class Simplex
    {
        public double[,] table; // таблица с коэфами ограничений, по ходу превращается в симплекс таблицу
        public double[] b; // правые части
        public string[] signs; // знаки
        public double[] targetFunction; // наша функция
        public bool maximize; // минимизация либо максимизация
        public List<int> basis;
        private List<double> artificialVariables;

        public Simplex(double[,] table,double[]b,double[] targetFunction,bool maximize,string[] signs)
        {
            if(table.GetLength(0)==b.GetLength(0)&& table.GetLength(0) == signs.GetLength(0)&&
                targetFunction.GetLength(0)==table.GetLength(1))
            {
                basis=new List<int>();
                artificialVariables = new List<double>();
                this.table = table;
                this.b = b;
                this.signs = signs;
                this.targetFunction = targetFunction;
                this.maximize = maximize;
                PrepareTable();
                CalculateIndexRow();
            }
            else
            {
                throw new ArgumentException("Неверный ввод данных");
            }
        }
        private void PrepareTable()
        {
            var m = table.GetLength(0);
            var n= table.GetLength(1);
            for (int i=0;i<signs.Length; i++)
            {
                switch (signs[i])
                {
                    case "=":
                        n++;
                        break;
                    case "<=":
                        n++;
                        break;
                    case ">=":
                        n = n + 2;
                        break;
                    case "<":
                        n++;
                        break;
                    case ">":
                        n = n + 2;
                        break;
                }
            }
            var newtable=new double[m+1,n];
            var newTargetFunction = new double[n];
            var newb = new double[m + 1];
            for (int i = 0; i < m; i++)
            {
                newb[i] = b[i];
                for (int j = 0; j < n; j++)
                {
                    if (table.GetLength(1) > j)
                    {
                        newtable[i, j] = table[i, j];
                        newTargetFunction[j] = targetFunction[j];
                    }
                }
            }
            double artificialVariableValue=maximize? -999999 : 999999;
            var position = 0;
            for (int i=0;i<m;i++)
            {
                switch (signs[i])
                {
                    case "<=":
                        newtable[i, table.GetLength(1) + i] = 1;
                        newTargetFunction[table.GetLength(1) + i] = 0;
                        basis.Add(table.GetLength(1) + i);
                        position++;
                        break;
                    case ">=":
                        newtable[i, table.GetLength(1) + i] = -1;
                        newTargetFunction[table.GetLength(1) + i] = 0;
                        position++;
                        break;
                    case "<":
                        newtable[i, table.GetLength(1) + i] = 1;
                        newTargetFunction[table.GetLength(1) + i] = 0;
                        basis.Add(table.GetLength(1) + i);
                        position++;
                        break;
                    case ">":
                        newtable[i, table.GetLength(1) + i] = -1;
                        newTargetFunction[table.GetLength(1) + i] = 0;
                        position++;
                        break;
                }
            }
            for (int i = 0; i < m; i++)
            {
                switch (signs[i])
                {
                    case "=":
                        newtable[i, table.GetLength(1) + i] = 1;
                        newTargetFunction[table.GetLength(1) + position+ i] = artificialVariableValue;
                        artificialVariables.Add(table.GetLength(1) + position+ i);
                        basis.Add(table.GetLength(1) + position+i);
                        break;
                    case ">=":
                        newtable[i, table.GetLength(1) + position+i] = 1;
                        newTargetFunction[table.GetLength(1) + position+i] = artificialVariableValue;
                        artificialVariables.Add(table.GetLength(1) + position+i);
                        basis.Add(table.GetLength(1) + position+i);
                        break;
                    case ">":
                        newtable[i, table.GetLength(1) + position+i] = 1;
                        newTargetFunction[table.GetLength(1) + position+i] = artificialVariableValue;
                        artificialVariables.Add(table.GetLength(1) + position+i);
                        basis.Add(table.GetLength(1) + position+i);
                        break;
                }
            }
            for (int i=0;i<newTargetFunction.Length;i++)
            {
                Console.Write("{0}\t", newTargetFunction[i]);
            }
            Console.WriteLine();
            Console.WriteLine("--------------------------------");
            targetFunction = newTargetFunction;
            table = newtable;
            b = newb;
        }
        public void PrintTable()
        {
            for (int i = 0; i < table.GetLength(0); i++)
            {
                Console.WriteLine();
                if(i>=basis.Count())
                {
                    Console.Write("-\t");
                }
                else
                {
                    Console.Write("X{0}\t", basis[i] + 1);
                }
                Console.Write("{0}\t", b[i]);
                for (int j = 0; j < table.GetLength(1); j++)
                {
                    Console.Write("{0}\t", table[i, j]);
                }
            }
            Console.WriteLine();
            Console.WriteLine("--------------------------------------");
        }

        private void CalculateIndexRow()
        {
            for(int j=0;j<table.GetLength(1);j++)
            {
                double value=0;
                for (int i=0;i<basis.Count;i++)
                {
                    value = value + table[i, j] * targetFunction[basis[i]];
                }
                table[table.GetLength(0)-1,j] = value-targetFunction[j];
            }
            for (int i=0;i<basis.Count;i++)
            {
                b[table.GetLength(0) - 1] += b[i] * targetFunction[basis[i]];
            }
        }

        private void PrintResult()
        {
            for(int j=0;j<basis.Count;j++)
            {
                if(artificialVariables.Contains(basis[j]) && b[j]!=0)
                {
                    Console.WriteLine("Так как в оптимальном решении присутствуют искусственные переменные X{0}, то задача не имеет допустимого решения.", basis[j] + 1);
                    return;
                }
            }
            for (int j=0;j<table.GetLength(1);j++)
            {
                if (table[table.GetLength(0)-1,j]==0 && !basis.Contains(j))
                {
                    Console.WriteLine("Альтернативный оптимум: X{0}", j + 1);
                }
            }
            Console.WriteLine("Решение");
            for (int j = 0; j < basis.Count; j++)
            {
                if (b[j]>0)
                {
                    Console.WriteLine("X{0}={1}", basis[j] + 1, b[j]);
                }
            }

        }
        public void Calculate()
        {

            int mainCol, mainRow;
            do
            {
                mainCol = FindMainCol();
                mainRow = FindMainRow(mainCol);
                if (mainCol == -1 || mainRow == -1) break;
                basis[mainRow] = mainCol;

                var newtable = new double[table.GetLength(0), table.GetLength(1)];
                var newb = new double[table.GetLength(0)];
                newb[mainRow] = b[mainRow] / table[mainRow, mainCol];
                for (int j = 0; j < table.GetLength(1); j++)
                {
                    newtable[mainRow, j] = table[mainRow, j] / table[mainRow, mainCol];
                }
                for (int i = 0; i < table.GetLength(0) - 1; i++)
                {
                    if (i == mainRow)
                        continue;
                    newb[i] = b[i] - table[i, mainCol] * newb[mainRow];
                    for (int j = 0; j < table.GetLength(1); j++)
                    {
                        newtable[i, j] = table[i, j] - table[i, mainCol] * newtable[mainRow, j];
                    }
                }
                table = newtable;
                b = newb;
                this.CalculateIndexRow();
                this.PrintTable();
            } while (!IsEnd());
            this.PrintResult();
        }
        private bool IsEnd()
        {
            bool flag = true;
            for (int j = 0; j < table.GetLength(1); j++)
            {
                bool findOut() => maximize ? table[table.GetLength(0) - 1, j] < 0  :
                    table[table.GetLength(0) - 1, j] > 0;
                if (findOut())
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }

        private int FindMainCol()
        {
            int mainCol = 0;
            int m = table.GetLength(0);
            int n=table.GetLength(1);
            for (int j = 0; j < n; j++)
            {
                bool findOut() => maximize ? table[m - 1, j] < table[m - 1, mainCol] : table[m - 1, j] > table[m - 1, mainCol];
                if (findOut())
                    mainCol = j;
            }
            return mainCol;
        }
        
        private int FindMainRow(int mainCol)
        {
            int mainRow = 0;
            bool isChange=false;
            for (int i = 0; i < table.GetLength(0) - 1; i++)
                if (table[i, mainCol] > 0)
                {
                    mainRow = i;
                    isChange = true;
                    break;
                }

            for (int i = mainRow; i < table.GetLength(0) - 1; i++)
                if ((table[i, mainCol] > 0) && ((b[i] / table[i, mainCol]) < (b[mainRow] / table[mainRow, mainCol])))
                {
                    mainRow = i;
                    isChange = true;
                }


            return isChange ? mainRow : -1;
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            var table = new double[,] {
                { 1,1,0,0,0,1 },
                { 0,1,0,0,1,1 },
                { 0,0,1,1,0,1 },
                { 0,0,0,1,-1,-1 },
            };
            //var b = new double[] { 1, 1, 1, 2 };
            //var targetFunction = new double[] { 1, 0, 0, 0, 0, 2 };
            //var signs = new string[] { "=", "=", "=", "=" };
            //var max = true;
            //var simplex = new Simplex(table, b, targetFunction, max, signs);
            //simplex.PrintTable();
            //simplex.Calculate();
            //var table1 = new double[,] {
            //    { 1,1,1,1,-1,-1 },
            //    { 0,1,1,-1,-1,-1 },
            //    { 0,1,0,0,0,-1 },
            //};
            //var b1 = new double[] { 1, 1, 2 };
            //var targetFunction1 = new double[] { 1, -4, 1, 1, 1, 1 };
            //var signs1 = new string[] { "=", "=", "=" };
            //var max1 = false;
            //var simplex1 = new Simplex(table1, b1, targetFunction1, max1, signs1);
            //simplex1.PrintTable();
            //simplex1.Calculate();
            var table2 = new double[,] {
                { 2,-1 },
                { 1,-2 },
                { 1,1 },
            };
            var b2 = new double[] { 4, 2, 5 };
            var targetFunction2 = new double[] { 1, -1 };
            var signs2 = new string[] { "<=", "<=", "<=" };
            var max2 = true;
            var simplex2 = new Simplex(table2, b2, targetFunction2, max2, signs2);
            simplex2.PrintTable();
            simplex2.Calculate();
            //var table3 = new double[,] {
            //   { 1,1 },
            //   { 1,5 },
            //   { 2,1 },
            //};
            //var b3 = new double[] { 3, 5, 4 };
            //var signs3 = new string[] { ">=", ">=", ">=" };
            //var max3 = false;
            //var targetFunction3 = new double[] { 7, 5 };
            //var simplex3= new Simplex(table3,b3,targetFunction3,max3,signs3);
            //simplex3.PrintTable();
            //simplex3.Calculate();
            Console.ReadKey();
        }
    }
}
