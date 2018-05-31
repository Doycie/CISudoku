using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Sudoku
{
    internal class SSudoku
    {
        //Sudoku size
        private int N = 9;

        //Block size (squareroot of N)
        private int B = 3;

        //Bool if you want extensive debug information, activate in the constructor
        private bool stepbystep;

        //Bool to know if the chartwindow is active, activate in the constructor
        private bool chartwindow;

        //Local copy of the board we are working on
        private int[][] board;

        //Local copy of the numbers that are fixed for easy lookup
        private int[][] fixedboard;

        //The queue for updating the chart window (reference)
        private Queue<int> oldscores;

        //The lock for the chart window (reference)
        private Object lockie;

        //Constructor with all the options for debugging info and the chart window
        public SSudoku(bool step = false, bool cw = false, Queue<int> oldsc = null, Object l = null)
        {
            stepbystep = step;
            chartwindow = cw;
            oldscores = oldsc;
            lockie = l;
        }

        //Initialize the sudoku by setting the size and the local copies of the board
        public void init(int[,] b, int n)
        {
            N = n;
            B = (int)Math.Sqrt(N);

            board = new int[N][];
            fixedboard = new int[N][];

            for (int i = 0; i < N; i++)
            {
                board[i] = new int[N];
                fixedboard[i] = new int[N];
                for (int j = 0; j < N; j++)
                {
                    board[i][j] = b[i, j];
                    fixedboard[i][j] = b[i, j];
                }
            }
        }

        //Method for starting the solve
        public void solve(int rs, int ra)
        {
            //Fill in the remaining numbers randomly for each block
            fillInBoard();

            //Calculate the initial score
            int score = Evaluation();
            Random random = new Random(rs);

            //To keep track of the iterations
            int it = 0;
            int iterations = 0;
            //To keep track of whether the score changes
            int sameScore = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //While we havent solved the sudoku
            while (score != 0)
            {
                iterations++;
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

                if(sw.Elapsed.TotalMilliseconds > 1000.0)
                {
                    Console.WriteLine(it);
                    it = 0;
                    sw.Restart();
                   
                }

                //Keep track if the score has changed
                if (score == oldscore)
                {
                    sameScore++;
                }
                else
                    sameScore = 0;

                //If the score has stayed the same for too long go random
                if (sameScore > 20)
                {
                    sameScore = 0;
                    int t = ra;
                    //Apply the neighbour operation a certain amout of times
                    walkRandomly(t, ref random);
                    //Recalculate the score
                    score = Evaluation();
                }
            }
            Console.WriteLine("Total iterations: " + iterations);
        }

        //Method for making the best swap we can find a 3 by 3 square on the board
        private int makeBestRandomSwap(ref Random random, int score)
        {
            //Choose random block
            int randomN = random.Next(0, N);

            //The x and y start coordinate for that block
            int xb = (randomN % B) * B;
            int yb = (randomN / B) * B;

            //To keep track of the best score we have found
            int xibest = 0;
            int yibest = 0;
            int xjbest = 0;
            int yjbest = 0;
            int bestscore = score;
            bool foundimprov = false;

            //Try every swap but only remember the one with the best score
            for (int i = 0; i < N - 1; i++)
            {
                //The offset coordinates for the first potential swap candidate
                int xi = xb + i / 3;
                int yi = yb + i % 3;
                //Only continue if it is not a fixed square
                if (!(fixedboard[xi][yi] == 0))
                    continue;

                for (int j = i + 1; j < N; j++)
                {
                    //The offset coordinates for the second potential swap candidate
                    int xj = xb + j / 3;
                    int yj = yb + j % 3;

                    //Only continue if it is viable
                    if (!(fixedboard[xj][yj] == 0))
                        continue;

                    int n1 = board[xi][yi];
                    int n2 = board[xj][yj];

                    board[xi][yi] = 0;
                    board[xj][yj] = 0;

                    int differenceInScore = 0;



                    //We go over all the squares and check if the number is there, in the old rows/columns we substract a number by one and in the new ones
                    //we add one to the number. We do have to make sure we dont check again so we use boolean values.

                    if (xi != xj && yi != yj)
                    {
                        bool foundNumber1 = false;
                        bool foundNumber2 = false;

                        bool foundNumber3 = false;
                        bool foundNumber4 = false;

                        bool foundNumber5 = false;
                        bool foundNumber6 = false;

                        bool foundNumber7 = false;
                        bool foundNumber8 = false;
                        for (int k = 0; k < N; k++)
                        {
                            if (board[xi][k] == n1 && !foundNumber1)
                            {
                                differenceInScore -= 1;
                                foundNumber1 = true;
                            }

                            if (board[k][yi] == n1 && !foundNumber3)
                            {
                                differenceInScore -= 1;
                                foundNumber3 = true;
                            }

                            if (board[xj][k] == n2 && !foundNumber4)
                            {
                                differenceInScore -= 1;
                                foundNumber4 = true;
                            }

                            if (board[k][yj] == n2 && !foundNumber5)
                            {
                                differenceInScore -= 1;
                                foundNumber5 = true;
                            }

                            if (board[xi][k] == n2 && !foundNumber2)
                            {
                                differenceInScore += 1;
                                foundNumber2 = true;
                            }
                            if (board[k][yi] == n2 && !foundNumber6)
                            {
                                differenceInScore += 1;
                                foundNumber6 = true;
                            }

                            if (board[xj][k] == n1 && !foundNumber7)
                            {
                                differenceInScore += 1;
                                foundNumber7 = true;
                            }

                            if (board[k][yj] == n1 && !foundNumber8)
                            {
                                differenceInScore += 1;
                                foundNumber8 = true;
                            }
                        }
                    }else if(xi != xj && yi == yj)
                    {
                        bool foundNumber1 = false;
                        bool foundNumber2 = false;

                        bool foundNumber4 = false;


                        bool foundNumber7 = false;
                        for (int k = 0; k < N; k++)
                        {
                            if (board[xi][k] == n1 && !foundNumber1)
                            {
                                differenceInScore -= 1;
                                foundNumber1 = true;
                            }

                            if (board[xj][k] == n2 && !foundNumber4)
                            {
                                differenceInScore -= 1;
                                foundNumber4 = true;
                            }

                            if (board[xi][k] == n2 && !foundNumber2)
                            {
                                differenceInScore += 1;
                                foundNumber2 = true;
                            }


                            if (board[xj][k] == n1 && !foundNumber7)
                            {
                                differenceInScore += 1;
                                foundNumber7 = true;
                            }
                        }

                    }else if (xi == xj && yi != yj)
                    {
                       

                        bool foundNumber3 = false;

                        bool foundNumber5 = false;
                        bool foundNumber6 = false;

                        bool foundNumber8 = false;
                        for (int k = 0; k < N; k++)
                        {
  
                            if (board[k][yi] == n1 && !foundNumber3)
                            {
                                differenceInScore -= 1;
                                foundNumber3 = true;
                            }

                            if (board[k][yj] == n2 && !foundNumber5)
                            {
                                differenceInScore -= 1;
                                foundNumber5 = true;
                            }

                            if (board[k][yi] == n2 && !foundNumber6)
                            {
                                differenceInScore += 1;
                                foundNumber6 = true;
                            }

                            if (board[k][yj] == n1 && !foundNumber8)
                            {
                                differenceInScore += 1;
                                foundNumber8 = true;
                            }
                        }
                    }else if(xi == xj &&yi == yj)
                    {

                    }
                    board[xi][yi] = n1;
                    board[xj][yj] = n2;
                    //Calculate the new score and check if it is better, in that case we save it
                    int sco2 = score + differenceInScore;

                    if (sco2 < bestscore)
                    {
                        xibest = xi;
                        yibest = yi;
                        xjbest = xj;
                        yjbest = yj;
                        bestscore = sco2;
                        foundimprov = true;
                        //If we found the best possible improvement skip ahead
                        if (differenceInScore < -3)
                        {
                            i = j = N + 1;
                        }
                    }
                    //If the score is the same and we havent found a different one yet save it else accept it with a random chance
                    if (sco2 == bestscore)
                    {
                        if (foundimprov == false)
                        {
                            xibest = xi;
                            yibest = yi;
                            xjbest = xj;
                            yjbest = yj;
                            bestscore = sco2;
                            foundimprov = true;
                        }
                        else
                        {
                            int p = random.Next(0, 5);
                            if (p % 1 == 0)
                            {
                                xibest = xi;
                                yibest = yi;
                                xjbest = xj;
                                yjbest = yj;
                                bestscore = sco2;
                                foundimprov = true;
                            }
                        }
                    }
                }
            }

            //If we found an improvement
            if (foundimprov)
            {
                if (stepbystep)
                {
                    Console.Clear();
                    Console.WriteLine("Found an improvement: " + bestscore + " vs " + score + " | " + " at: (" + xibest + "," + yibest + ") and (" + xjbest + "," + yjbest + ").");
                    print(xibest, yibest, xjbest, yjbest);
                    Console.ReadLine();
                }

                //Make the actual swap
                swap(xibest, yibest, xjbest, yjbest);

                if (stepbystep)
                {
                    Console.Clear();
                    Console.WriteLine("Found an improvement: " + bestscore + " vs " + score + " | " + " at: (" + xibest + "," + yibest + ") and (" + xjbest + "," + yjbest + ").");
                    print(xibest, yibest, xjbest, yjbest);
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

        //Method to apply the random swap operator a number of times
        private void walkRandomly(int amountOfTimes, ref Random random)
        {
            for (int i = 0; i < amountOfTimes; i++)
            {
                //Choose a random block
                int randomN = random.Next(0, N);

                //Get the coordinates for the block
                int xb = (randomN % B) * B;
                int yb = (randomN / B) * B;

                //Choose two random numbers
                int randomI = random.Next(0, N);
                int randomJ = random.Next(0, N);

                //Coordinates for the offset of the first number
                int xib = xb + randomI % B;
                int yib = yb + randomI / B;

                //Coordinates for the offset of the second number
                int xjb = xb + randomJ % B;
                int yjb = yb + randomJ / B;

                //If it is swappable
                if (!(xib == xjb && yib == yjb) && fixedboard[xib][yib] == 0 && fixedboard[xjb][yjb] == 0)
                {
                    if (stepbystep)
                    {
                        Console.Clear();
                        Console.WriteLine("Randomly taking some steps. Remaining " + (amountOfTimes - i));
                        print(xib, yib, xjb, yjb);
                        Console.ReadLine();
                    }

                    //Make the swap
                    swap(xib, yib, xjb, yjb);

                    if (stepbystep)
                    {
                        Console.Clear();
                        Console.WriteLine("Randomly taking some steps. Remaining " + (amountOfTimes - i));
                        print(xib, yib, xjb, yjb);
                        Console.ReadLine();
                    }
                }
            }
        }

        //Method for swapping two numbers in the sudoku
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void swap(int xi, int yi, int xj, int yj)
        {
            int t = board[xi][yi];
            board[xi][yi] = board[xj][yj];
            board[xj][yj] = t;
        }

        //Method to count the amount of missing numbers in a row, only used in the evaluation function
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CountMissingNumbersC(int i, ref int[][] board)
        {
            int total = 0;
            int bits = 0;
            //Go over the row and add the numbers to a their coresponding offset in a bit string
            for (int k = 0; k < N; k++)
            {
                bits |= 1 << board[k][i];
            }
            //Now count the bitstring bits that are 1
            for (int k = 1; k < N + 1; k++)
            {
                total += (bits >> k) & 1;
            }
            return N - total;
        }

        //Method to count the amount of missing numbers in a column, only used in the evaluation function
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CountMissingNumbersR(int i, ref int[][] board)
        {
            int total = 0;
            int bits = 0;
            //Go over the column and add the numbers to a their coresponding offset in a bit string
            for (int k = 0; k < N; k++)
            {
                bits |= 1 << board[i][k];
            }
            //Now count the bitstring bits that are 1
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

        //Print the current board, you can mark two numbers by the argument, they will be colored
        public void print(int markx = -1, int marky = -1, int markx2 = -1, int marky2 = -1)
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
                    Console.Write(board[i][j] + " ");
                    Console.ResetColor();
                    if (j == N - 1)
                        Console.Write("|");
                }
                Console.WriteLine();
                if ((i) % 3 == 2)
                    Console.WriteLine("-------------------------");
            }
        }

        //Method to fill the board initialy, just loop over all the blocks and add anything that isnt there yet
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
                    if (n == board[i][j])
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
                    if (board[i][j] == 0)
                    {
                        board[i][j] = n;
                        return;
                    }
                }
            }
        }
    }
}