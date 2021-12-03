using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Approximator //оголошення блоку, в якому зберігається програма
{
    public partial class FormMain : System.Windows.Forms.Form //клас форми програми, в якому зберігається код
    {
        private double[] result; //загальна масиви розвя'зків системи рівнянь
        private double[,] xyTable, matrix; //загальні масиви табличних даних та значень матриці

        public FormMain()
        {
            InitializeComponent();
        }

        private void TableRemake() //зміна структури таблиці вхідних даних
        {
            //кількість рядків таблиці відповідає значенню "кількість вузлів"
            dataGridTable.RowCount = Convert.ToInt32(numericUpDownCount.Value);
            for (int i = 0; i < dataGridTable.RowCount; i++)
            {
                //встановлення типу даних в комірках
                dataGridTable[0, i].ValueType = System.Type.GetType("System.Double");
                dataGridTable[1, i].ValueType = System.Type.GetType("System.Double");
                //В пустих комірках автоматично ставиться нуль
                if (Convert.ToString(dataGridTable[0, i].Value) == "")
                    dataGridTable[0, i].Value = 0;
                if (Convert.ToString(dataGridTable[1, i].Value) == "")
                    dataGridTable[1, i].Value = 0;
            }
        }

        //метод для виклику розв'язання методом Гаусса
        private double[] Gauss(double[,] matrix, int rowCount, int colCount)
        {
            int i; //індекс масиву
            int[] mask = new int[colCount - 1]; //допоміжний масив індексів стовпців матриці
            for (i = 0; i < colCount - 1; i++) mask[i] = i;
            //ref використовується посилання на об'єкт для його безпосередньої зміни
            //Виконання методу складається з двох етапів,
            if (GaussDirectPass(ref matrix, ref mask, colCount, rowCount))
            {
                //тому другий етап почне працювати тільки після успішного виконання першого, який поверне true для цієї умови
                double[] answer = GaussReversePass(ref matrix, mask, colCount, rowCount);
                //другий етап повертає результат розв'язання
                return answer;
            }
            //нульвое значення при невдалому виконані першого етапу
            else return null;
        }

        //Прямий хід методу Гаусса
        private bool GaussDirectPass(ref double[,] matrix, ref int[] mask, int colCount, int rowCount)
        {
            int i, j, k, maxId, tmpInt;
            double maxVal, tempDouble;
            for (i = 0; i < rowCount; i++) //цикл, який перебирає рядки матриці
            {
                maxId = i; //індекс найбільшого значення в рядку
                maxVal = matrix[i, i]; //найбільше значення в рядку
                //цикл перебиратиме значення рядку для пошуку найбільшого по модулю(головного)
                for (j = i + 1; j < colCount - 1; j++)
                    if (Math.Abs(maxVal) < Math.Abs(matrix[i, j]))
                    {
                        maxVal = matrix[i, j];
                        maxId = j;
                    }
                if (maxVal == 0) return false; //рядок складається з нулів - розв'язку немає, завершення методу
                //перестановка головного рядка "нагору" матриці
                if (i != maxId)
                {
                    //перестановка значень
                    for (j = 0; j < rowCount; j++)
                    {
                        tempDouble = matrix[j, i];
                        matrix[j, i] = matrix[j, maxId];
                        matrix[j, maxId] = tempDouble;
                    }
                    //перестановка вказівників на шуканий коефіцієнт відповідно до рядків
                    tmpInt = mask[i];
                    mask[i] = mask[maxId];
                    mask[maxId] = tmpInt;
                }
                for (j = 0; j < colCount; j++) matrix[i, j] /= maxVal; //множники для неголовних рядків
                for (j = i + 1; j < rowCount; j++)
                {
                    //почленне додавання помноженого головного рядка до неголовного
                    double tempMn = matrix[j, i];
                    for (k = 0; k < colCount; k++)
                        matrix[j, k] -= matrix[i, k] * tempMn;
                }
            }
            return true;
        }

        //Зворотній ход методу Гаусса
        private double[] GaussReversePass(ref double[,] matrix, int[] mask, int colCount, int rowCount)
        {
            int i, j, k;
            //аналогія прямому ходу, але вже з другого кінця матриці
            for (i = rowCount - 1; i >= 0; i--)
                for (j = i - 1; j >= 0; j--)
                {
                    double tempMn = matrix[j, i];
                    for (k = 0; k < colCount; k++)
                        matrix[j, k] -= matrix[i, k] * tempMn;
                }
            //створення і повернення массиву розв'язків
            double[] answer = new double[rowCount];
            for (i = 0; i < rowCount; i++) answer[mask[i]] = matrix[i, colCount - 1];
            return answer;
        }

        //класс створення системи рівнянь
        private double[,] MakeSystem(double[,] xyTable, int basis)
        {
            double[,] matrix = new double[basis, basis + 1];
            for (int i = 0; i < basis; i++)
            {
                for (int j = 0; j < basis; j++)
                {
                    matrix[i, j] = 0;
                }
            }
            for (int i = 0; i < basis; i++)
            {
                for (int j = 0; j < basis; j++)
                {
                    double sumA = 0, sumB = 0;
                    for (int k = 0; k < dataGridTable.RowCount; k++)
                    {
                        sumA += Math.Pow(xyTable[0, k], i) * Math.Pow(xyTable[0, k], j);
                        sumB += xyTable[1, k] * Math.Pow(xyTable[0, k], i);
                    }
                    matrix[i, j] = sumA;
                    matrix[i, basis] = sumB;
                }
            }
            return matrix;
        }


        //реакція на зміну значення "Кількість вузлів"
        private void numericUpDownCount_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                TableRemake();
                //максимально допустимий для пошуку степінь многочлена напряму залежить від кількості відомих даних
                numericUpDownBasis.Maximum = numericUpDownCount.Value - 1;
            }
            catch
            {
                if (numericUpDownCount.Value < 1)
                {
                    MessageBox.Show("Кількість вузлів не може бути рівною 0");
                    numericUpDownCount.Minimum = 1;
                }
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            TableRemake();
        }

        private void dataGridTable_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show("Введене значення не є дійсним числом");
        }


        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridTable.RowCount; i++)
            {
                dataGridTable[0, i].Value = 0;
                dataGridTable[1, i].Value = 0;
            }
            matrix = null;
            result = null;
            xyTable = null;

        }

        private void buttonCalc_Click(object sender, EventArgs e)
        {
            //виділення пам'яті під матрицю введених значень
            xyTable = new double[2, dataGridTable.RowCount];
            try
            {
                for (int i = 0; i < dataGridTable.RowCount; i++)
                {
                    xyTable[0, i] = Convert.ToDouble(dataGridTable[0, i].Value);
                    xyTable[1, i] = Convert.ToDouble(dataGridTable[1, i].Value);
                }
            }
            //якщо було введено нечислове значення
            catch
            {
                MessageBox.Show("Одне із введених в таблицю значень не є дійсним числом");
                richTextBoxResult.Select();
                SendKeys.Send("^{END}");
                return;
            }
            //запис у пам'ять значення "Степінь многочлена"
            int basis = Convert.ToInt32(numericUpDownBasis.Value) + 1;
            matrix = MakeSystem(xyTable, basis); //утворення матриці системи лінійних рівнянь
            //виведення системи лінійних рівнянь
            richTextBoxResult.Text += "-------------------------------------------------------------------------------\n";
            richTextBoxResult.Text += "Створюємо систему рівнянь:\n";
            for (int i = 0; i < basis; i++)
            {
                for (int j = 0; j < basis; j++)
                {
                    richTextBoxResult.Text += ((matrix[i, j] > 0) ? "+" : "") +
                        Math.Round(matrix[i, j], 3).ToString() + "*x" + j.ToString() + " ";
                }
                richTextBoxResult.Text += " = " + matrix[i, basis] + "\n";
            }
            //підключаємо метод Гаусса, результат виконання якого записано в вище ініціалізований загальний масив
            result = Gauss(matrix, basis, basis + 1);
            //якщо метод не зміг виконати прямий хід
            if (result == null)
            {
                richTextBoxResult.Text += "Неможливо знайти часткове рішення системи рівнянь\n";
                richTextBoxResult.Select();
                SendKeys.Send("^{END}");
                return;
            }
            //виведення розв'язків
            richTextBoxResult.Text += "Рішення системи рівнянь:\n";
            for (int i = 0; i < basis; i++)
            {
                richTextBoxResult.Text += "C" + i.ToString() + " = " + Math.Round(result[i], 3).ToString() + "\n";
            }
            //виведення рівняння
            richTextBoxResult.Text += "Апроксимація:\ny = ";
            for (int i = 0; i < basis; i++)
            {
                if (Math.Round(result[i], 3) != 0)
                    richTextBoxResult.Text += ((result[i] > 0) ? "+" : "") +
                        Math.Round(result[i], 3).ToString() + ((i > 0) ? "*x^" + i.ToString() : "") + " ";
            }
            richTextBoxResult.Text += "\n";
            richTextBoxResult.Select();
            SendKeys.Send("^{END}");
        }

        private void buttonCalcX_Click(object sender, EventArgs e)
        {
            int basis = Convert.ToInt32(numericUpDownBasis.Value) + 1;
            //якщо ще не знайдена апроксимація
            if (result == null)
            {
                richTextBoxResult.Text += "Апроксимація функції ще не була проведена\n";
                richTextBoxResult.Select();
                SendKeys.Send("^{END}");
                return;
            }
            double x;
            try
            {
                //запис у пам'ять значення Х
                x = Convert.ToDouble(textBoxX.Text);
            }
            //Якщо Х - не число
            catch
            {
                MessageBox.Show("Введене значення Х не є дійсним числом");
                richTextBoxResult.Select();
                SendKeys.Send("^{END}");
                return;
            }
            //розрахунок Y при такому Х
            double y = 0;
            for (int i = 0; i < basis; i++)
            {
                y += result[i] * Math.Pow(x, i);
            }
            //виведення результату
            richTextBoxResult.Text += "f(" + x.ToString() + ") = " + Math.Round(y, 5).ToString() + " \n";
            richTextBoxResult.Select();
            SendKeys.Send("^{END}");

        }

    }
}
