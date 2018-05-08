using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    class SSudoku
    {
        //Sudoku size
        private int N = 9;

        //Block size (squareroot of N)
        private int B = 3;
        private bool stepbystep;
        private bool chartwindow;
        int[,] board;
        int[,] fixedboard;
        Queue<int> oldscores;
        Object lockie;

        public SSudoku( bool step = false, bool cw = false, Queue<int> oldsc = null, Object l = null)
        {
            stepbystep = step;
            chartwindow = cw;
            oldscores = oldsc;
            lockie = l;
        }
        public void init(int[,] b, int n)
        {
            N = n;
            B = (int)Math.Sqrt(N);

            board = new int[N, N];
            fixedboard = new int[N, N];

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    board[i, j] = b[i, j];
                    fixedboard[i, j] = b[i, j];
                }
            }

        }
        public void solve(int rs)
        {

            //Fill in the remaining numbers randomly for each block
            fillInBoard();

            int score = Evaluation();
            Random random = new Random(rs);
            int it = 0;
            int sameScore = 0;

            while (score != 0)
            {
                it++;
                //Make the best swap we can and later check if we have a better score
                int oldscore = score;
                score = makeBestRandomSwap(ref random, score);

                //Add the new values for the graph
                if (chartwindow)
                {
                    lock (lockie)
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
                    if (it > 2000)
                    {
                        it = 0;
                        t = 500;
                    }
                    walkRandomly( t, ref random);
                    score = Evaluation();
                }
            }



        }

        private int makeBestRandomSwap(ref Random random, int score)
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
                            differenceInScore -= 1;
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

                    swap( xi, yi, xj, yj);

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


                    swap( xi, yi, xj, yj);

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
                    print( xibest, yibest, xjbest, yjbest);
                    Console.ReadLine();
                }
                swap(xibest, yibest, xjbest, yjbest);
                if (stepbystep)
                {
                    Console.Clear();
                    Console.WriteLine("Found an improvement: " + bestscore + " vs " + score + " | " + " at: (" + xibest + "," + yibest + ") and (" + xjbest + "," + yjbest + ").");
                    print( xibest, yibest, xjbest, yjbest);
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

        private  void walkRandomly( int amountOfTimes, ref Random random)
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
                        print( xib, yib, xjb, yjb);
                        Console.ReadLine();
                    }
                    swap( xib, yib, xjb, yjb);

                    if (stepbystep)
                    {
                        Console.Clear();
                        Console.WriteLine("Randomly taking some steps. Remaining " + (amountOfTimes - i));
                        print( xib, yib, xjb, yjb);
                        Console.ReadLine();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private  void swap( int xi, int yi, int xj, int yj)
        {
            int t = board[xi, yi];
            board[xi, yi] = board[xj, yj];
            board[xj, yj] = t;
        }


        //Count for a row or column the amount of numbers that are missing
        private  int CountMissingNumbersOLD(bool row, int i, ref int[,] board)
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
        private  int CountMissingNumbersC(int i, ref int[,] board)
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
        private int CountMissingNumbersR(int i, ref int[,] board)
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
        private int Evaluation()
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
        public void print( int markx = -1, int marky = -1, int markx2 = -1, int marky2 = -1)
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
        private void printQueue(ref Queue<int> q)
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

        private void fillInBoard()
        {
            for (int i = 0; i < B; i++)
            {
                for (int j = 0; j < B; j++)
                {
                    for (int k = 1; k < N + 1; k++)
                    {
                        if (!NumberInBlock(k, i * B, j * B))
                        {
                            FillInNumber(k, i * B, j * B);
                        }
                    }
                }
            }
        }

        //Check if a number is in a block
        private bool NumberInBlock(int n, int xb, int yb)
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
        private void FillInNumber(int n, int xb, int yb)
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
