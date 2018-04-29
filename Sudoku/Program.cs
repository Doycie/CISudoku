using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Sudoku
{
    class Program
    {
        //Sudoku size 
        const int N = 9;
        //Block size (squareroot of N)
        const int B = 3;


        static bool stepbystep = false;
        static bool chartwindow = false;

        static private Object thisLock = new Object();


        static Queue<int> oldscores;

        static void Main(string[] args)
        {

            int[,] board = new int[N, N];
            int[,] fixedboard = new int[N, N];

            Console.WriteLine("Please input a " + N + " by " + N + " Sudoku." );

            //Read in the board, also make a fixed copie for utilities
            readBoard(ref board, ref fixedboard);

            Console.Clear();

            //Fill in the remaining numbers randomly for each block
            fillInBoard(ref board);

            int score = Evaluation(ref board);

            Console.WriteLine("COMMENCING LOCAL SEARCH, CURRENT SCORE: " + score);
            print(ref board);


            Random random = new Random();


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

            int sameScore = 0;



            Stopwatch sw = new Stopwatch();

            sw.Start();

            while (score != 0)
            {

                //Make the best swap we can and later check if we have a better score
                int oldscore = score;
                score = makeBestRandomSwap(ref random,score,ref board,ref fixedboard );

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
                if(score == oldscore)
                    sameScore++;
                else
                    sameScore = 0;

                //If the score has stayed the same for too long go random
                if(sameScore > 150)
                {
                    sameScore = 0;
                    walkRandomly(ref board, ref fixedboard, 300, ref random);
                    score = Evaluation(ref board);
                }
                
            }

            sw.Stop();

            Console.Clear();
            Console.WriteLine("Solved in: " + sw.Elapsed.TotalSeconds + "s!");
            Console.WriteLine(Evaluation(ref board));
            print(ref board);
            Console.ReadLine();

        }


        static int makeBestRandomSwap(ref Random random, int score,ref int[,] board,ref int[,] fixedboard)
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

                    //check if with swap we are faster
                    int oldrowiscore = CountMissingNumbers(true, xi, ref board);
                    int oldcolumniscore = CountMissingNumbers(false, yi, ref board);

                    int oldrowjscore = CountMissingNumbers(true, xj, ref board);
                    int oldcolumnjscore = CountMissingNumbers(false, yj, ref board);

                    swap(ref board, xi, yi, xj, yj);

                    int newrowiscore = CountMissingNumbers(true, xi, ref board);
                    int newcolumniscore = CountMissingNumbers(false, yi, ref board);

                    int newrowjscore = CountMissingNumbers(true, xj, ref board);
                    int newcolumnjscore = CountMissingNumbers(false, yj, ref board);

                    int sco = score - oldrowiscore - oldrowjscore - oldcolumniscore - oldcolumnjscore +
                                    +newrowiscore + newrowjscore + newcolumniscore + newcolumnjscore;


                    if (sco <= bestscore)
                    {
                        xibest = xi;
                        yibest = yi;
                        xjbest = xj;
                        yjbest = yj;
                        bestscore = sco;
                        foundimprov = true;
                    }

                    swap(ref board, xi, yi, xj, yj);

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
                 
            }else
            {
                //Console.WriteLine("No better or equal swap was found");
            }
            return score;

        }

        static void readBoard( ref int[,] board, ref int[,] fixedboard)
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


        static void walkRandomly(ref int[,] board,ref int[,] fixedboard, int amountOfTimes, ref Random random)
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
                    if (stepbystep) {
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


        static void swap(ref int[,] board,  int xi, int yi, int xj,int yj)
        {
            int t = board[xi, yi];
            board[xi, yi] = board[xj, yj];
            board[xj, yj] = t;
        }


        static void OpenWindow()
        {
            GraphTest f1 = new Sudoku.GraphTest( oldscores, thisLock);
            System.Windows.Forms.Application.Run(f1);
        }

        //Count for a row or column the amount of numbers that are missing
        static int CountMissingNumbers(bool row, int i ,ref int[,] board)
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

        //Evaluation function counts how many numbers are missing in each row and column, the lower the better
        static int Evaluation(ref int[,] board)
        {
            int total = 0;
            //Count each row
            for (int i = 0; i < N; i++)
            {
                total += CountMissingNumbers(true, i,ref board);

            }
            //Count each column
            for (int i = 0; i < N; i++)
            {
                total += CountMissingNumbers(false, i, ref board);
            }
            return total;
        }

        //Print the current board
        static void print(ref int[,] board, int markx = -1,int marky = -1,int markx2 = -1, int marky2 = -1)
        {
            Console.WriteLine("-------------------------");
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {

                    if((j) % 3 == 0 )
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
        static void printQueue(ref Queue<int> q)
        {
            foreach (int t in q)
            {
                StringBuilder s = new StringBuilder();
                for(int i = 0; i < t; i++)
                {
                    s.Append(" ");
                }
                Console.Write(s);
                Console.WriteLine("@");
            }
        }

        static void fillInBoard(ref int[,] board)
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
        static bool NumberInBlock(int n, int xb, int yb, ref int[,] board)
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
        static void FillInNumber(int n, int xb, int yb, ref int[,] board)
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
