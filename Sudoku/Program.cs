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
        static private Object thisLock = new Object();

        private static Queue<int> oldscores;

        private static bool chartwindow = false;

        static int[,] OriginalSudoku;

        private static void Main(string[] args)
        {

            Console.WriteLine("Please input a Sudoku.");

            //Read in the board, also make a fixed copie for utilities
            int N = readBoard();
            Console.Clear();

            //Queue for the chart
            oldscores = new Queue<int>(100);

            //Start a new thread for the chart window
            if (chartwindow)
            {
                System.Threading.Thread mythread;
                mythread = new System.Threading.Thread(new System.Threading.ThreadStart(OpenWindow));
                mythread.Start();
            }


            
            System.Threading.Thread t = new System.Threading.Thread(() => solve(N,1));
            t.Start();
            //System.Threading.Thread t2 = new System.Threading.Thread(() => solve(N, 2));
            //t2.Start();
           // System.Threading.Thread t3 = new System.Threading.Thread(() => solve(N, 3));
           // t3.Start();
            //System.Threading.Thread t4 = new System.Threading.Thread(() => solve(N,4));
           // t4.Start();


            /*while (t.IsAlive
                //&& t2.IsAlive && t3.IsAlive && t4.IsAlive
                ) {
                loading();
                Thread.Sleep(1000);


            }*/

            

            t.Join();
            //t2.Join();
            //t3.Join();
            //t4.Join();

            Console.ReadLine();
        }

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

        private static void solve(int N, int i)
        {
            for (int k = 1; k < 21; k++)
            {
                double temp = 0;
                for (int j = 0; j < 50; j++)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    SSudoku s = new SSudoku();      //Deze constructor is voor als er geen graph is.
                                                    //SSudoku s = new SSudoku(false, true, oldscores, thisLock);        //Deze constructor is voor als er wel een graph is.
                    s.init(OriginalSudoku, N);



                    s.solve(i, 5*k);

                    sw.Stop();

                    //Console.WriteLine("Solved in: " + sw.Elapsed.TotalSeconds + "s!");
                    //s.print();
                    temp += sw.Elapsed.TotalSeconds;
                }
                temp = temp / 100;
                Console.WriteLine(5*k + " " + temp);
            }
        }

        private static void OpenWindow()
        {
            GraphTest f1 = new Sudoku.GraphTest(oldscores, thisLock);
            System.Windows.Forms.Application.Run(f1);
        }

        private static int readBoard()
        {
            string[] input = Console.ReadLine().Split();
            int N = input.Length-1;

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