using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sudoku
{
    internal class Program
    {
        //Sudoku size
        private const int N = 9;

        //Block size (squareroot of N)
        private const int B = 3;

        private static bool stepbystep = false;
        private static bool chartwindow = false;

        static private Object thisLock = new Object();

        private static Queue<int> oldscores;

        private static void Main(string[] args)
        {
            int[,] board = new int[N, N];
            int[,] fixedboard = new int[N, N];

            Console.WriteLine("Please input a " + N + " by " + N + " Sudoku.");

            //Read in the board, also make a fixed copie for utilities
            readBoard(ref board, ref fixedboard);

            Console.Clear();

            //Fill in the remaining numbers randomly for each block
            fillInBoard(ref board);

            int score = Evaluation(ref board);

            Console.WriteLine("COMMENCING LOCAL SEARCH, CURRENT SCORE: " + score);
            print(ref board);

            Random random = new Random(1);

            //Queue for the chart

            oldscores = new Queue<int>(100);
            oldscores.Enqueue(score);

            //Start a new thread for the chart window
            if (chartwindow)
            {
                System.Threading.Thread mythread;
                mythread = new System.Threading.Thread(new System.Threading.ThreadStart(OpenWindow));
                mythread.Start();
            }

            const int amountSolves = 1;

            List<double> times = new List<double>(amountSolves);

            for (int i = 0; i < amountSolves; i++)
            {
                for(int k = 0; k < N; k++)
                {
                    for (int p = 0; p < N; p++)
                    {
                        board[k, p] = fixedboard[k, p];
                    }
                }


                fillInBoard(ref board);

                score = Evaluation(ref board);

                int sameScore = 0;

                Stopwatch sw = new Stopwatch();

                sw.Start();
                int it = 0;
                while (score != 0)
                {
                    it++;
                    //Make the best swap we can and later check if we have a better score
                    int oldscore = score;
                    score = makeBestRandomSwap(ref random, score, ref board, ref fixedboard);

                    //   Console.WriteLine("We might have made a swap, oldscore: " + oldscore +  " newscore: " + score + "Actualscore: " + Evaluation(ref board));
                    //  Console.ReadLine();

                    //Add the new values for the graph
                    if (chartwindow)
                    {
                        lock (thisLock)
                        {
                            oldscores.Enqueue(score);
                            if (oldscores.Count > 200)
                                oldscores.Dequeue();
                        }
                    }

                    //Keep track if the score has changed
                    if (score == oldscore)
                        sameScore++;
                    else
                        sameScore = 0;

                    //If the score has stayed the same for too long go random
                    if (sameScore > 36)
                    {
                        sameScore = 0;
                        int t = 20;
                        if (it > 1000)
                        {
                            it = 0;
                            Console.WriteLine("Starting with 300 random steps");
                            t = 300;
                        }
                        walkRandomly(ref board, ref fixedboard,t , ref random);
                        score = Evaluation(ref board);
                    }
                }

                sw.Stop();

                Console.Clear();
                Console.WriteLine("Solved in: " + sw.Elapsed.TotalSeconds + "s!");
                Console.WriteLine(Evaluation(ref board));
                times.Add( sw.Elapsed.TotalSeconds);

            }

            double total = 0;
            for(int i = 0; i < times.Count; i++)
            {
                total += times[i];
            }
            total /= amountSolves;

            Console.WriteLine("Average: " + total);


            print(ref board);




            Console.ReadLine();
        }

        private static int makeBestRandomSwap(ref Random random, int score, ref int[,] board, ref int[,] fixedboard)
        {
            //Choose random block
            int randomN = random.Next(0, N);

            int xb = (randomN % B) * B;
            int yb = (randomN / B) * B;

            //Try every swap but only remember the one with the best score

            int xibest = 0;
            int yibest = 0;
            int xjbest = 0;
            int yjbest = 0;
            int bestscore = score;
            bool foundimprov = false;

            for (int i = 0; i < N - 1; i++)
            {
                int xi = xb + i / 3;
                int yi = yb + i % 3;
                if (!(fixedboard[xi, yi] == 0))
                    continue;

                for (int j = i + 1; j < N; j++)
                {
                    int xj = xb + j / 3;
                    int yj = yb + j % 3;

                    if (!(fixedboard[xj, yj] == 0))
                        continue;

                    int n1 = board[xi, yi];
                    int n2 = board[xj, yj];
                    //check if with swap we are faster

                    int differenceInScore = 0;

                    //Check if a row already has the number double if so the score stays the same, else the score is incremented because we will be missing an number
                    for (int k = 0; k < N; k++)
                    {
                        if (board[xi, k] == n1 && k != yi)
                        {
                            differenceInScore -=1;
                            break;
                        }
                    }
                    differenceInScore++;

                    for (int k = 0; k < N; k++)
                    {
                        if (board[k, yi] == n1 && k != xi)
                        {
                            differenceInScore -= 1;
                            break;
                        }
                    }
                    differenceInScore++;

                    //J
                    for (int k = 0; k < N; k++)
                    {
                        if (board[xj, k] == n2 && k != yj)
                        {
                            differenceInScore -= 1;
                            break;
                        }
                    }
                    differenceInScore++;

                    for (int k = 0; k < N; k++)
                    {
                        if (board[k, yj] == n2 && k != xj)
                        {
                            differenceInScore -= 1;
                            break;
                        }
                    }
                    differenceInScore++;

                    swap(ref board, xi, yi, xj, yj);

                    //After swap


                    for (int k = 0; k < N; k++)
                    {
                        if (board[xi, k] == n2 && k != yi)
                        {
                            differenceInScore += 1;
                            break;
                        }
                    }
                    differenceInScore--;

                    for (int k = 0; k < N; k++)
                    {
                        if (board[k, yi] == n2 && k != xi)
                        {
                            differenceInScore += 1;
                            break;
                        }
                    }
                    differenceInScore--;

                    //J
                    for (int k = 0; k < N; k++)
                    {
                        if (board[xj, k] == n1 && k != yj)
                        {
                            differenceInScore += 1;
                            break;
                        }
                    }
                    differenceInScore--;

                    for (int k = 0; k < N; k++)
                    {
                        if (board[k, yj] == n1 && k != xj)
                        {
                            differenceInScore += 1;
                            break;
                        }
                    }
                    differenceInScore--;


                    swap(ref board, xi, yi, xj, yj);

                    /*
                    int oldrowiscore = CountMissingNumbersR(xi, ref board);
                    int oldcolumniscore = CountMissingNumbersC(yi, ref board);

                    int oldrowjscore = CountMissingNumbersR(xj, ref board);
                    int oldcolumnjscore = CountMissingNumbersC(yj, ref board);

                    swap(ref board, xi, yi, xj, yj);

                    int newrowiscore = CountMissingNumbersR(xi, ref board);
                    int newcolumniscore = CountMissingNumbersC(yi, ref board);

                    int newrowjscore = CountMissingNumbersR(xj, ref board);
                    int newcolumnjscore = CountMissingNumbersC(yj, ref board);

                    int sco = score - oldrowiscore - oldrowjscore - oldcolumniscore - oldcolumnjscore +
                                    +newrowiscore + newrowjscore + newcolumniscore + newcolumnjscore;

                    */
                    int sco2 = score + differenceInScore;

                    if (sco2 <= bestscore)
                    {
                        xibest = xi;
                        yibest = yi;
                        xjbest = xj;
                        yjbest = yj;
                        bestscore = sco2;
                        foundimprov = true;
                    }

                   //swap(ref board, xi, yi, xj, yj);
                }
            }

            if (foundimprov)
            {
                if (stepbystep)
                {
                    Console.Clear();
                    Console.WriteLine("Found an improvement: " + bestscore + " vs " + score + " | " + " at: (" + xibest + "," + yibest + ") and (" + xjbest + "," + yjbest + ").");
                    print(ref board, xibest, yibest, xjbest, yjbest);
                    Console.ReadLine();
                }
                swap(ref board, xibest, yibest, xjbest, yjbest);
                if (stepbystep)
                {
                    Console.Clear();
                    Console.WriteLine("Found an improvement: " + bestscore + " vs " + score + " | " + " at: (" + xibest + "," + yibest + ") and (" + xjbest + "," + yjbest + ").");
                    print(ref board, xibest, yibest, xjbest, yjbest);
                    Console.ReadLine();
                }
                return bestscore;
            }
            else
            {
                //Console.WriteLine("No better or equal swap was found");
            }
            return score;
        }

        private static void readBoard(ref int[,] board, ref int[,] fixedboard)
        {
            for (int i = 0; i < N; i++)
            {
                string[] input = Console.ReadLine().Split();
                for (int j = 0; j < N; j++)
                {
                    int k = int.Parse(input[j]);
                    board[i, j] = k;
                    fixedboard[i, j] = k;
                }
            }
        }

        private static void walkRandomly(ref int[,] board, ref int[,] fixedboard, int amountOfTimes, ref Random random)
        {
            for (int i = 0; i < amountOfTimes; i++)
            {
                int randomN = random.Next(0, N);

                int xb = (randomN % B) * B;
                int yb = (randomN / B) * B;

                int randomI = random.Next(0, N);
                int randomJ = random.Next(0, N);

                int xib = xb + randomI % B;
                int yib = yb + randomI / B;

                int xjb = xb + randomJ % B;
                int yjb = yb + randomJ / B;

                if (!(xib == xjb && yib == yjb) && fixedboard[xib, yib] == 0 && fixedboard[xjb, yjb] == 0)
                {
                    if (stepbystep)
                    {
                        Console.Clear();
                        Console.WriteLine("Randomly taking some steps. Remaining " + (amountOfTimes - i));
                        print(ref board, xib, yib, xjb, yjb);
                        Console.ReadLine();
                    }
                    swap(ref board, xib, yib, xjb, yjb);

                    if (stepbystep)
                    {
                        Console.Clear();
                        Console.WriteLine("Randomly taking some steps. Remaining " + (amountOfTimes - i));
                        print(ref board, xib, yib, xjb, yjb);
                        Console.ReadLine();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void swap(ref int[,] board, int xi, int yi, int xj, int yj)
        {
            int t = board[xi, yi];
            board[xi, yi] = board[xj, yj];
            board[xj, yj] = t;
        }

        private static void OpenWindow()
        {
            GraphTest f1 = new Sudoku.GraphTest(oldscores, thisLock);
            System.Windows.Forms.Application.Run(f1);
        }

        //Count for a row or column the amount of numbers that are missing
        private static int CountMissingNumbersOLD(bool row, int i, ref int[,] board)
        {
            int total = 0;
            bool[] NumberOccurrences = new bool[N];

            for (int k = 0; k < N; k++)
            {
                if (row)
                    NumberOccurrences[board[i, k] - 1] = true;
                else
                    NumberOccurrences[board[k, i] - 1] = true;
            }

            for (int k = 0; k < N; k++)
            {
                if (NumberOccurrences[k] == false)
                {
                    total += 1;
                }
            }
            return total;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CountMissingNumbersC(int i, ref int[,] board)
        {
            int total = 0;
            int bits = 0;
            for (int k = 0; k < N; k++)
            {
                bits |= 1 << board[k, i];
            }
            for (int k = 1; k < N + 1; k++)
            {
                total += (bits >> k) & 1;
            }
            return N - total;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CountMissingNumbersR(int i, ref int[,] board)
        {
            int total = 0;
            int bits = 0;
            for (int k = 0; k < N; k++)
            {
                bits |= 1 << board[i, k];
            }
            for (int k = 1; k < N + 1; k++)
            {
                total += (bits >> k) & 1;
            }
            return N - total;
        }

        //Evaluation function counts how many numbers are missing in each row and column, the lower the better
        private static int Evaluation(ref int[,] board)
        {
            int total = 0;
            //Count each row
            for (int i = 0; i < N; i++)
            {
                total += CountMissingNumbersR(i, ref board);
            }
            //Count each column
            for (int i = 0; i < N; i++)
            {
                total += CountMissingNumbersC(i, ref board);
            }
            return total;
        }

        //Print the current board
        private static void print(ref int[,] board, int markx = -1, int marky = -1, int markx2 = -1, int marky2 = -1)
        {
            Console.WriteLine("-------------------------");
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if ((j) % 3 == 0)
                        Console.Write("| ");
                    if (markx == i && marky == j)
                        Console.ForegroundColor = ConsoleColor.Red;
                    if (markx2 == i && marky2 == j)
                        Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(board[i, j] + " ");
                    Console.ResetColor();
                    if (j == N - 1)
                        Console.Write("|");
                }
                Console.WriteLine();
                if ((i) % 3 == 2)
                    Console.WriteLine("-------------------------");
            }
        }

        //Print something of a graph for the queue
        private static void printQueue(ref Queue<int> q)
        {
            foreach (int t in q)
            {
                StringBuilder s = new StringBuilder();
                for (int i = 0; i < t; i++)
                {
                    s.Append(" ");
                }
                Console.Write(s);
                Console.WriteLine("@");
            }
        }

        private static void fillInBoard(ref int[,] board)
        {
            for (int i = 0; i < B; i++)
            {
                for (int j = 0; j < B; j++)
                {
                    for (int k = 1; k < N + 1; k++)
                    {
                        if (!NumberInBlock(k, i * B, j * B, ref board))
                        {
                            FillInNumber(k, i * B, j * B, ref board);
                        }
                    }
                }
            }
        }

        //Check if a number is in a block
        private static bool NumberInBlock(int n, int xb, int yb, ref int[,] board)
        {
            for (int i = xb; i < xb + B; i++)
            {
                for (int j = yb; j < yb + B; j++)
                {
                    if (n == board[i, j])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //Fill in a number in a certain block on the first 0 position it comes across
        private static void FillInNumber(int n, int xb, int yb, ref int[,] board)
        {
            for (int i = xb; i < xb + B; i++)
            {
                for (int j = yb; j < yb + B; j++)
                {
                    if (board[i, j] == 0)
                    {
                        board[i, j] = n;
                        return;
                    }
                }
            }
        }
    }
}