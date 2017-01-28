using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication48
{

    //Наперед попереджую, реалізація даної програмки могла б бути краща, покищо без multi-threading-а
    // З радістю прийму критику та пропозиції щодо покращення якості коду

    class Cell // Комірка таблиці
    {
        public object Value { get; private set; }
        public char NumberOfColumn { get; private set; }
        public int NumberOfLine { get; private set; }
        public bool IsReady { get; private set; }
        public Cell(object Value,char NumberOfColumn,int NumberOfLine)
        {
            this.Value = Value;
            this.NumberOfColumn = NumberOfColumn;
            this.NumberOfLine = NumberOfLine;
            this.IsReady = false;
        }
        public void Compute()
        {
            float temp;
            if(float.TryParse(Value.ToString(),out temp))
            {
                this.Value = temp;
                this.IsReady = true;
                return;
            }
            if(Value.ToString().StartsWith("'"))
            {
                this.Value = Value.ToString().Substring(1);
                this.IsReady = true;
                return;
            }
            if(Value.ToString().StartsWith("="))//Якщо у комірці є вираз
            {
                this.Value = this.Value.ToString().Substring(1);
                string[] words = this.Value.ToString().Split('*', '/', '-', '+');
                string[] signs = this.Value.ToString().Split(words,StringSplitOptions.RemoveEmptyEntries);
                Queue<char> charPrior = new Queue<char>(new List<char>() { '/', '*', '+', '-' });
                
                while(true)
                {
                    if (words.Length == 1) //Якщо у комірці тільки є тільки посилання на іншу комірку
                    {
                        float tmp;
                        if (!float.TryParse(this.Value.ToString(), out tmp))
                        {
                            this.Value = this.GetCellValueByValue(this.Value.ToString());
                            this.IsReady = true;
                            return;
                        }
                        
                    }
                    if (!signs.Contains(charPrior.Peek().ToString()))
                    {
                        charPrior.Dequeue();
                        continue;
                    }
                    int signIndex = this.Value.ToString().IndexOf(charPrior.Peek());
                    int leftNumberIndex;
                    int rightNumberIndex;
                    string leftOperand = null;
                    string rightOperand = null;
                    for (leftNumberIndex = signIndex - 1; leftNumberIndex > 0; leftNumberIndex--) 
                    {
                        if (signs.Contains(this.Value.ToString()[leftNumberIndex].ToString()))//Шукаємо з якого індекса починається лівий операнд
                        {
                            leftNumberIndex++;
                            break;
                        }
                    }
                    
                    for (int count = 0; count < words.Length; count++)//Шукаємо лівий операнд
                    {
                        string tmp = this.Value.ToString().Substring(leftNumberIndex, signIndex - leftNumberIndex);
                        if (words[count] == tmp)
                        {
                            leftOperand = words[count];
                            break;
                        }
                    }

                    for (rightNumberIndex = signIndex + 1; rightNumberIndex < this.Value.ToString().Length; rightNumberIndex++)//Шукаємо з якого індекса починається правий операнд
                    {
                        if(signs.Contains(this.Value.ToString()[rightNumberIndex].ToString()))
                        {
                            break;
                        }
                    }

                    for (int count = 0; count < words.Length; count++)//Шукаємо правий операнд
                    {
                        string tmp = this.Value.ToString().Substring(signIndex + 1, rightNumberIndex - (signIndex + 1));
                        if (tmp == words[count])
                        {
                            rightOperand = words[count];
                            break;
                        }
                            
                    }
                    float numberOfLeftOperand;
                    float numberOfRightOperand;
                    if(!float.TryParse(leftOperand,out numberOfLeftOperand))
                    {
                        string tmp = GetCellValueByValue(leftOperand);
                        if(!float.TryParse(tmp,out numberOfLeftOperand))
                        {
                            this.Value = "#";
                            this.IsReady = true;
                            return;
                        }
                    }
                    if(!float.TryParse(rightOperand,out numberOfRightOperand))
                    {
                        string tmp = GetCellValueByValue(rightOperand);
                        if(!float.TryParse(tmp,out numberOfRightOperand))
                        {
                            this.Value = "#";
                            this.IsReady = true;
                            return;
                        }
                    }
                    float operationResult = 0;
                    char selectedOperation = ' ';
                    switch(this.Value.ToString()[signIndex])
                    {
                        case '/':operationResult = numberOfLeftOperand / numberOfRightOperand;
                            selectedOperation = '/';
                            break;
                        case '*':operationResult = numberOfLeftOperand * numberOfRightOperand;
                            selectedOperation = '*';
                            break;
                        case '+': operationResult = numberOfLeftOperand + numberOfRightOperand;
                            selectedOperation = '+';
                            break;
                        case '-':operationResult = numberOfLeftOperand - numberOfRightOperand;
                            selectedOperation = '-';
                            break;
                    }

                    int operationIndex = signs.ToList().IndexOf(selectedOperation.ToString());
                    signs[operationIndex] = string.Empty;//Видаляємо використаний знак операції

                    int leftOperandIndex = words.ToList().IndexOf(leftOperand);
                    int rightOperandIndex = words.ToList().IndexOf(rightOperand);
                    words[leftOperandIndex] = operationResult.ToString();
                    words[rightOperandIndex] = string.Empty;

                    StringBuilder intermediateResult = new StringBuilder();//Проміжна змінна для оновлення значення в комірці
                    bool updated = false;
                    for (int i = 0; i < this.Value.ToString().Length; i++) 
                    {
                        if (!updated && i >= leftNumberIndex && i < rightNumberIndex)
                        {
                            updated = true;
                            intermediateResult.Append(operationResult);
                            continue;
                        }
                        else if (i < leftNumberIndex || i >= rightNumberIndex)  
                            intermediateResult.Append(this.Value.ToString()[i]);
                    }
                    this.Value = intermediateResult.ToString();//Оновлюємо значення комірки

                    if (signs.All(n => n == string.Empty))//Якщо більше нема ніяких операцій, то завершуємо обчислення
                    {
                        this.IsReady = true;
                        return;
                    }
                        
                }
                
            }

        }
        private string GetCellValueByValue(string Name)
        {
            foreach(Cell c in Program.cells)
            {
                if (Name[0] == c.NumberOfColumn && Name.Substring(1) == c.NumberOfLine.ToString())
                {
                    if (!c.IsReady)
                        c.Compute();
                    return c.Value.ToString();
                }
            }
            return "#";
        }
    }
    class Program
    {
        public static List<Cell> cells = new List<Cell>();
        static void Main(string[] args)
        {
            int countOfLines = 3;
            int[] countOfColumnsInLine = new int[countOfLines];// Має в собі кількість стовбців у рядках
            for (int numberLine = 0; numberLine < countOfLines; numberLine++) 
            {
                string line = Console.ReadLine();
                string[] values = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                countOfColumnsInLine[numberLine] = values.Length;
                char letter = 'A';
                
                for (int numberColumn = 0; numberColumn < values.Length; numberColumn++,letter++) 
                {
                    Cell tempCell = new Cell(values[numberColumn], letter, numberLine + 1);
                    cells.Add(tempCell);
                }
            }

            Console.WriteLine();
            foreach (Cell c in cells) 
            {
                if (c.IsReady) 
                    continue;
                c.Compute();
            }
            int count = 0;
            for (int i = 0; i < countOfLines; i++)
            {
                for (int j = 0; j < countOfColumnsInLine[i]; j++)
                {
                    Console.Write(cells[count].Value + "\t\t");
                    count++;
                }
                Console.WriteLine();
            }

            Console.ReadLine();
        }
    }
}
