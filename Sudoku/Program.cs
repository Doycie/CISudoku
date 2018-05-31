using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Sudoku
{
    internal class Program
    {
        //Lock object for the chart window to make sure the threads dont write to the queue at the same time
        static private Object thisLock = new Object();

        private static StreamWriter streamwriter = new StreamWriter("Results.txt");

        //Queue for holding old scores in case we want to see the chart window
        private static Queue<int> oldscores;

        //Enable of disable the chart window, to fully activate add this line as a constructor: SSudoku s = new SSudoku(false,true, oldscores,thisLock);
        private static bool chartwindow = false;

        //Int array to save the original input board
        private static int[,] OriginalSudoku;

        private static void Main(string[] args)
        {
            Console.WriteLine("Please input a Sudoku.");

            //Read in the board with spaces or without, whatever suits you
            int N = readBoard();
            //int N = readBoardFromFile(6);

            //Queue for the chart window
            oldscores = new Queue<int>(100);

            //Start a new thread for the chart window
            if (chartwindow)
            {
                System.Threading.Thread mythread;
                mythread = new System.Threading.Thread(new System.Threading.ThreadStart(OpenWindow));
                mythread.Start();
            }


            //Solve the sudoku
            SSudoku sudoku = new SSudoku();
            sudoku.init(OriginalSudoku, N);
            sudoku.solve(1,8);
            sudoku.print();
            
            //For multithreading

            /*
            System.Threading.Thread t = new System.Threading.Thread(() => solve(N, 2));
            t.Start();

            //While the sudoku is searching keep the main thread sleeping
            while (t.IsAlive)
            {
                Thread.Sleep(1000);
            }

            t.Join();
            */


            Console.ReadLine();
        }

        //Makes a new SSudoku object and runs the solve algoritm with local search 50 times for every sudoku for every random walk length, outputs the results to a text file
        private static void solve(int N, int i)
        {
            //Keep track of time with a stopwatch
            Stopwatch sw = new Stopwatch();
            SSudoku s = new SSudoku();

            //Initialize the class with the original board and size

            double time = 0;

            double bestTime = double.MaxValue;
            int bestP = 0;
            for (int r = 0; r < 10; r++)
            {
                readBoardFromFile(r);
                for (int p = 2; p < 40; p += 2)
                {
                    for (int k = 0; k < 50; k++)
                    {
                        //Actually solve the sudoku

                        s.init(OriginalSudoku, N);
                        sw.Reset();
                        sw.Start();
                        s.solve(i + 5 + k, p);
                        // Console.WriteLine("Solved");
                        sw.Stop();
                        time += sw.Elapsed.TotalSeconds;
                    }
                    streamwriter.WriteLine(time / 50);
                    streamwriter.Flush();
                    // Console.WriteLine(time / 10);
                    if (time / 50 < bestTime)
                    {
                        bestTime = time / 50;
                        bestP = p;
                    }
                    time = 0;
                }
                streamwriter.WriteLine();
                Console.WriteLine("Solved " + (r + 1) + " with " + (bestP) + "!");
                bestTime = double.MaxValue;
                bestP = 0;
            }

        }

        //Debugging window to view the graph of the score
        private static void OpenWindow()
        {
            GraphTest f1 = new Sudoku.GraphTest(oldscores, thisLock);
            System.Windows.Forms.Application.Run(f1);
        }

        //Function to read the board from a text file. Make sure there are spaces at the end
        private static int readBoardFromFile(int a)
        {
            StreamReader sr = new StreamReader("sudoku_puzzels.txt");

            sr.ReadLine();
            for (int i = 0; i < (a * 10); i++)
            {
                sr.ReadLine();
            }
            string[] input = sr.ReadLine().Split();
            int N = input.Length + 1;

            if (input.Length == 1)
            {
                N = input[0].Length;

                OriginalSudoku = new int[N, N];
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        int k = (int)(input[0][j]) - '0';
                        OriginalSudoku[i, j] = k;
                    }

                    input = sr.ReadLine().Split();
                }
            }
            return N;
        }

        //Read the board from the Console by manualling copying
        private static int readBoard()
        {
            //Read the board from input, if there are spaces go to else
            string[] input = Console.ReadLine().Split();
            int N = input.Length;

            if (input.Length == 1)
            {
                N = input[0].Length;

                OriginalSudoku = new int[N, N];
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        int k = (int)(input[0][j]) - '0';
                        OriginalSudoku[i, j] = k;
                    }
                    input = Console.ReadLine().Split();
                }
            }
            else
            {
                if (input[input.Length - 1] == "")
                    N = input.Length - 1;

                OriginalSudoku = new int[N, N];

                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        int k = int.Parse(input[j]);
                        OriginalSudoku[i, j] = k;
                    }
                    input = Console.ReadLine().Split();
                }
            }
            return N;
        }

        private static void printResult(double[] results)
        {
            Console.WriteLine("Results:");
            for(int k = 1; k < 11; k++)
            {
                Console.WriteLine(5*k + " " + results[k-1]);
            }
        }
    }
}