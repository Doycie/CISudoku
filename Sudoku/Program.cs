using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
namespace Sudoku
{
    class Program
    {
        //Lock object for the chart window to make sure the threads dont write to the queue at the same time
        static private Object thisLock = new Object();

        //Queue for holding old scores in case we want to see the chart window
        private static Queue<int> oldscores;

        //Enable of disable the chart window, to fully activate add this line as a constructor: SSudoku s = new SSudoku(false,true, oldscores,thisLock);
        private static bool chartwindow = false;

        //Int array to save the original input board
        static int[,] OriginalSudoku;

        private static void Main(string[] args)
        {

            Console.WriteLine("Please input a Sudoku.");

            //Read in the board with spaces or without, whatever suits you
            int N = readBoard();
            Console.Clear();

            //Queue for the chart window
            oldscores = new Queue<int>(100);

            //Start a new thread for the chart window
            if (chartwindow)
            {
                System.Threading.Thread mythread;
                mythread = new System.Threading.Thread(new System.Threading.ThreadStart(OpenWindow));
                mythread.Start();
            }


            //Start a new thread for solving a sudoku
            System.Threading.Thread t = new System.Threading.Thread(() => solve(N,1));
            t.Start();
            //System.Threading.Thread t2 = new System.Threading.Thread(() => solve(N, 2));
            //t2.Start();
           // System.Threading.Thread t3 = new System.Threading.Thread(() => solve(N, 3));
           // t3.Start();
            //System.Threading.Thread t4 = new System.Threading.Thread(() => solve(N,4));
           // t4.Start();

            //While the sudoku is searching keep the main thread sleeping
            while (t.IsAlive
                //&& t2.IsAlive && t3.IsAlive && t4.IsAlive
                ) {
                loading();
                Thread.Sleep(1000);


            }

            

            t.Join();
            //t2.Join();
            //t3.Join();
            //t4.Join();

            Console.ReadLine();
        }

        //Function to print out some lines to make sure the program is searching
        public static void loading()
        {
            Console.Clear();
            Random r = new Random();
            int i = r.Next(0, 4);

            switch (i)
            {
                case 0:
                    Console.WriteLine("Crunching the latest numbers...");
                    break;
                case 1:
                    Console.WriteLine("Calculating pi...");
                    break;
                case 2:
                    Console.WriteLine("Counting to nine...");
                    break;
                case 3:
                    Console.WriteLine("Cracking the sudoku...");
                    break;
            }
        }

        //Makes a new SSudoku object and runs the solve algoritm with local search
        private static void solve(int N, int i)
        {
            //Keep track of time with a stopwatch
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SSudoku s = new SSudoku();

            //Initialize the class with the original board and size
            s.init(OriginalSudoku, N);
            //Actually solve the sudoku
            s.solve(i);
            sw.Stop();

            //Print the time it took and the board
            Console.WriteLine("Solved in: " + sw.Elapsed.TotalSeconds + "s!");
            s.print();

        }

        private static void OpenWindow()
        {
            GraphTest f1 = new Sudoku.GraphTest(oldscores, thisLock);
            System.Windows.Forms.Application.Run(f1);
        }

        private static int readBoard()
        {
            //Read the board from input, if there are spaces go to else
            string[] input = Console.ReadLine().Split();
            int N = input.Length;

            if (input.Length == 1)
            {
                N = input[0].Length;

                OriginalSudoku = new int[N, N];
                for(int i = 0; i < N; i++)
                {
                    for(int j = 0; j < N; j++)
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
    }
}