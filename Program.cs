using System;
using System.Collections.Generic;
using System.IO;

namespace tic_tac_toe
{
    public struct Point
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public override string ToString() => $"({X}, {Y})";
    }

    interface IGame
    {
        //поле для игры
        Players[,] field { get; set; }

        //символы игроков
        enum Players
        {
            player1 = 'x',
            player2 = '0'
        }

        Players currentPlayer { get; set; }

        void OpositePlayerStep(); //ходы противника


        bool CheckWinner();
        bool TryToTakeCell(int x, int y); //попытка занять место на карте

        void RealPlayerStep(int x, int y); //отправка хода пользователя
        public bool IsDraw(); // проверка на ничью

        bool isWinner { get; set; }
    }

    abstract class AGame : IGame
    {
        public AGame()
        {
            currentPlayer = IGame.Players.player1;
            field = new IGame.Players[3, 3];
        }

        public IGame.Players currentPlayer { get; set; }
        public IGame.Players[,] field { get; set; }
        public bool isWinner { get; set; }


        public bool IsDraw()
        {
            for (int x = 0; x < field.GetLength(0); x++)
            {
                for (int y = 0; y < field.GetLength(1); y++)
                {

                    if (field[x, y] == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        protected List<Point[]> winningPositions = new List<Point[]> {
            new Point[] { new Point(0, 0), new Point(0, 1), new Point(0, 2), },
            new Point[] { new Point(1, 0), new Point(1, 1), new Point(1, 2), },
            new Point[] { new Point(2, 0), new Point(2, 1), new Point(2, 2), },
            new Point[] { new Point(0, 0), new Point(1, 0), new Point(2, 0), },
            new Point[] { new Point(0, 1), new Point(1, 1), new Point(2, 1), },
            new Point[] { new Point(0, 2), new Point(1, 2), new Point(2, 2), },
            new Point[] { new Point(0, 0), new Point(1, 1), new Point(2, 2), },
            new Point[] { new Point(2, 0), new Point(1, 1), new Point(0, 2), },
        };

        protected IGame.Players getCell(Point p)
        {
            return field[p.X, p.Y];
        }

        public bool CheckWinner()
        {
            /**    0 1 2
             *   0 = = =
             *   1 = = =
             *   2 = = =
             * */
            foreach (var p in winningPositions)
            {
                Point first = p[0], second = p[1], third = p[2];
                if (getCell(first) != 0 && getCell(first) == getCell(second) && getCell(second) == getCell(third))
                {
                    this.isWinner = true;
                    return true;
                }

            }

            return false;
        }

        public bool TryToTakeCell(int x, int y)
        {
            bool isNumbersInRange = x >= 0 && x <= 2 && y >= 0 && y <= 2;
            if (!isNumbersInRange) return false;

            if (this.field[x, y] == 0)
            {
                this.field[x, y] = currentPlayer;
                return true;
            }

            return false;
        }

        public void RealPlayerStep(int x, int y) {
            currentPlayer = IGame.Players.player1;
            Console.WriteLine($"you: {x},{y}");

            if (this.TryToTakeCell(x, y))
            {
                if (CheckWinner())
                {
                    Console.WriteLine("You Win!");
                }
                else
                {
                    if (IsDraw())
                    {
                        this.isWinner = true;
                        Console.WriteLine("Is Draw!");
                    }

                    OpositePlayerStep();
                }
            }
            else
            {
                Console.WriteLine("This coordinates is taken");
            }
        }

        abstract public void OpositePlayerStep();
    }

    class RandomStepGame : AGame
    {
        public override void OpositePlayerStep()
        {
            currentPlayer = IGame.Players.player2;
            // выбираем случайное место для хода...и ходим...
            Random random = new Random();
            int x, y;

            do
            {
                x = random.Next(0, this.field.GetLength(1));
                y = random.Next(0, this.field.GetLength(1));
            } while (!TryToTakeCell(x, y));

            Console.WriteLine($"opponent: {x},{y}");
            // end
            if (CheckWinner())
            {
                Console.WriteLine("Artificial inteligence Win!");
            }

            if (IsDraw())
            {
                this.isWinner = true;
                Console.WriteLine("Is Draw!");
            }

        }
    }

    class LogicStepGame : AGame
    {
        private bool isAvailableCombination(Point[] row)
        {
            foreach (var p in row)
            {
                if (getCell(p) == IGame.Players.player1)
                {
                    return false;
                }
            }

            return true;
        }

        public override void OpositePlayerStep()
        {
            currentPlayer = IGame.Players.player2;
            // выбираем место для хода...и ходим...
            Point fakeStep = new Point(100, 100);
            Point opStep = fakeStep;
            foreach (Point[] combo in winningPositions)
            {
                if (isAvailableCombination(combo))
                {
                    if (TryToTakeCell(combo[0].X, combo[0].Y))
                    {
                        opStep = combo[0];
                        break;
                    }
                    else if (TryToTakeCell(combo[1].X, combo[1].Y))
                    {
                        opStep = combo[1];
                        break;
                    }
                    else if (TryToTakeCell(combo[2].X, combo[2].Y))
                    {
                        opStep = combo[2];
                        break;
                    }
                }
            }

            if (opStep.Equals(fakeStep)) //  если выиграшного хода нет
            {
                bool brk = false;
                for (int x = 0; x < field.GetLength(0); x++)
                {
                    for (int y = 0; y < field.GetLength(1); y++)
                    {

                        if (field[x, y] == 0)
                        {
                            TryToTakeCell(x, y);
                            opStep = new Point(x, y);
                            brk = true;
                            break;
                        }
                    }
                    if (brk) break;
                }
            }



            // end
            if (CheckWinner())
            {
                Console.WriteLine("Artificial inteligence Win!");
            } else if (IsDraw())
            {
                this.isWinner = true;
                Console.WriteLine("Is Draw!");
            } else if (!opStep.Equals(fakeStep))
            {
                Console.WriteLine($"opponent: {opStep.X},{opStep.Y}");
            }
        }
    }

    class GameFromFile : AGame
    {
        protected List<IGame.Players[,]> winningResults { get; set; }
        private int step = 0;
        public GameFromFile() {
            winningResults = new List<IGame.Players[,]>();

            using (FileStream fstream = File.OpenRead("../../../fileForGame.txt"))
            {
                // преобразуем строку в байты
                byte[] array = new byte[fstream.Length];
                // считываем данные
                fstream.Read(array, 0, array.Length);
                // декодируем байты в строку
                string textFromFile = System.Text.Encoding.Default.GetString(array);
                var rows = textFromFile.Split("\n");
                foreach (var row in rows)
                {
                   var cells = row.Trim().Split(",");
                    IGame.Players[,] map2d = new IGame.Players[3,3];
                    int cellIndex = 0;
                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            map2d[x, y] = cells[cellIndex] == "x" ? IGame.Players.player1 : 
                                (cells[cellIndex] == "o" ? IGame.Players.player2 : 0);
                            cellIndex++;
                        }
                    }
                    winningResults.Add(map2d);
                }
            }
        }
        
        public override void OpositePlayerStep()
        {
            currentPlayer = IGame.Players.player2;
            // choose best matrix and make step
           
            var mostSimilar = winningResults[0];
            foreach (var map in winningResults)
            {
                if (GetFieldSimilarityLevel(mostSimilar) <= GetFieldSimilarityLevel(map)) {
                    mostSimilar = map;
                }
            }

            bool stepDone = false;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (
                        mostSimilar[x, y] == IGame.Players.player2 &&
                        field[x, y] == 0
                        ) 
                    {
                        /*Console.WriteLine("mostSimilar");
                        Program.DrawField(mostSimilar);*/

                        if (TryToTakeCell(x, y))
                        {
                            stepDone = true;
                            break;
                        } else continue;
                    }
                }

                if (stepDone) break;
            }


            if (!stepDone) {
                // do logic step if step wasn't made
                Point fakeStep = new Point(100, 100);
                Point opStep = fakeStep;
                foreach (Point[] combo in winningPositions)
                {
                    if (isAvailableCombination(combo))
                    {
                        if (TryToTakeCell(combo[0].X, combo[0].Y))
                        {
                            opStep = combo[0];
                            break;
                        }
                        else if (TryToTakeCell(combo[1].X, combo[1].Y))
                        {
                            opStep = combo[1];
                            break;
                        }
                        else if (TryToTakeCell(combo[2].X, combo[2].Y))
                        {
                            opStep = combo[2];
                            break;
                        }
                    }
                }

                if (opStep.Equals(fakeStep)) //  если выиграшного хода нет
                {
                    bool brk = false;
                    for (int x = 0; x < field.GetLength(0); x++)
                    {
                        for (int y = 0; y < field.GetLength(1); y++)
                        {

                            if (field[x, y] == 0)
                            {
                                TryToTakeCell(x, y);
                                opStep = new Point(x, y);
                                brk = true;
                                break;
                            }
                        }
                        if (brk) break;
                    }
                }
            }

            //Console.WriteLine("stepDone: " + stepDone);

            // end

            if (CheckWinner())
            {
                Console.WriteLine("Artificial inteligence Win!");
            }

            if (IsDraw())
            {
                this.isWinner = true;
                Console.WriteLine("Is Draw!");
            }

        }

        protected int GetFieldSimilarityLevel(IGame.Players [,] candidate)
        {
            step++;
            int ex = 3, o = 1, b = 2;
          
            if (step <= 2)
            {
                ex = 4;
                o = 4;
            } else if (step <= 3)
            {
                ex = 2;
                o = 3;
                b = 3;
            }
            

            int res = 0;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (candidate[x, y] == field[x, y])
                    {
                        switch(candidate[x,y])
                        {
                            case IGame.Players.player1:
                                res += ex;
                                break;
                            case IGame.Players.player2:
                                res += o;
                                break;
                            case 0:
                                res += b;
                                break;
                        }
                    }
                        
                }
            }
            return res;
        }

        private bool isAvailableCombination(Point[] row)
        {
            foreach (var p in row)
            {
                if (getCell(p) == IGame.Players.player1)
                {
                    return false;
                }
            }

            return true;
        }
    }


    class Program
    {
      
        public static void DrawField(IGame.Players[,] field)
        {
            Console.WriteLine("  0  1  2 ");
            for (int x = 0; x < field.GetLength(0); x++)
            {
                Console.Write(x);
                for (int y = 0; y < field.GetLength(1); y++)
                {

                    if (field[x, y] == IGame.Players.player1)
                    {
                        Console.Write(" x ");
                    }
                    else if (field[x, y] == IGame.Players.player2)
                    {
                        Console.Write(" o ");
                    }
                    else
                    {
                        Console.Write("   ");
                    }
                }
                Console.WriteLine();
            }
        }

        static void StartGameLoop(IGame game)
        {
            do
            {
                Console.WriteLine("================================================");
                DrawField(game.field);
                Console.WriteLine("your step (x,y): ");
                var stringNums = Console.ReadLine().Split(",");
                Console.Clear();
                if (stringNums.Length != 2) continue;
                game.RealPlayerStep(Convert.ToInt32(stringNums[0]), Convert.ToInt32(stringNums[1]));

            } while (!game.isWinner);
            Console.WriteLine("================================================");
            DrawField(game.field);
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Введите номер типа игры:\n" +
                "1-легко\n" +
                "2-средне\n" +
                "3-сложно");
            int typeOfGame = int.Parse(Console.ReadLine());
            switch (typeOfGame) {
                case 1: StartGameLoop(new RandomStepGame());
                    break;
                case 2: StartGameLoop(new LogicStepGame());
                    break;
                case 3: StartGameLoop(new GameFromFile());
                    break;
                default:
                    Console.WriteLine("Ничего не выбрано!");
                    break;
            }
        }
    }




}
/*
 * Спроектировать интерфейсы согласно варианту задания. Для каждого интерфейса
разработать три реализующих его класса. Все необходимые данные должны передаваться
в класс только посредством интерфейсных методов. Таким же образом должны
возвращаться результаты работы. Ввод-вывод данных должен осуществляться за
пределами классов, реализующих интерфейсы.
Обеспечить корректное поведение программы в случае отсутствия реализации части
функциональности интерфейса (например, в случае отсутствия в файле нужного значения).
Обеспечить выбор пользователем в процесс работы программы одной из трех
реализаций интерфейса.

 * Разработать интерфейс для игры в Крестики-нолики. На базе данного интерфейса
реализовать следующие классы:

 класс, выполняющий ход в соответствии с записанными в файле данными
под каждую позицию;
 класс, выбирающий решение на основе логики;
 класс-заглушку, выполняющий ход случайным образом.
*/