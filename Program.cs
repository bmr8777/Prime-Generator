using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Numerics;

// Name: Brennan Reed <@bmr8777@rit.edu>
// Date: 2/17/20
// Class: CSCI-251.01-02
// Assignment: Prime Gen Project

namespace PrimeGen
{
    /// <summary>
    ///     Static class which contains the BigInteger extension method IsProbablyPrime()
    /// </summary>
    static class BigIntegerExtension
    {
        /// <summary>
        ///     Extension method for BigIntegers that determines the primality of value
        /// </summary>
        /// <param name="value">
        ///     Value of the BigInteger
        /// </param>
        /// <param name="witnesses"></param>
        /// <returns>
        ///     Whether value is prime
        /// </returns>

        public static Boolean IsProbablyPrime(this BigInteger value, int witnesses = 10)
        {
            if (value <= 1) return false;

            if (witnesses <= 0) witnesses = 10;

            BigInteger d = value - 1;
            int s = 0;

            while (d % 2 == 0)
            {
                d /= 2;
                s += 1;
            }

            Byte[] bytes = new Byte[value.ToByteArray().LongLength];
            BigInteger a;

            for (int i = 0; i < witnesses; i++)
            {
                do
                {
                    var Gen = new Random();
                    Gen.NextBytes(bytes);
                    a = new BigInteger(bytes);
                }
                while (a < 2 || a >= value - 2);
            BigInteger x = BigInteger.ModPow(a, d, value);
                if (x == 1 || x == value - 1) continue;
                for (int r = 1; r < s; r++)
                {
                    x = BigInteger.ModPow(x, 2, value);
                    if (x == 1) return false;
                    if (x == value - 1) break;
                }
                if (x != value - 1) return false;
            }
            return true;
        }
    }

    /// <summary>
    ///     The class which holds the core logic, and necessary attributes
    ///     to generate and display the specified prime numbers
    /// </summary>
    
    class Program
    {
        private long count; // the desired number of random numbers
        private long currentCount; // counter for the number of prime numbers generated
        private long bits; // specified number of bits
        object sync = new object(); // lock used for maintaining thread output correctness/order

        /// <summary>
        ///     Class which is responsible for generating and displaying the prime numbers
        /// </summary>
        /// <param name="bits_">
        ///     specified number of bits
        /// </param>
        /// <param name="count_">
        ///     amount of prime numbers to generate
        /// </param>
        /// <returns>
        ///     new instance of the Program Class
        /// </returns>

        Program(long bits_, long count_)
        {
            count = count_;
            bits = bits_;
            currentCount = 0;
        }

        /// <summary>
        ///     Uses a Parallel.For loop to generate numbers and determines their primality using IsProbablyPrime()
        /// </summary>
        
        void GeneratePrimes()
        {
            while (currentCount < count) // continue until the desired number of primes have been generated
            {
                Parallel.For(0, 100000, (i, loopState) => 
                {
                    var rng = new RNGCryptoServiceProvider();
                    byte[] bytes = new byte[bits / 8];
                    rng.GetBytes(bytes);
                    var bi = new BigInteger(bytes); // randomly generated number
                    if (bi.IsProbablyPrime()) // checks if the number is prime
                    {
                        if (currentCount >= count) // stop the loop if enough numbers have been generated
                        {
                            loopState.Stop();
                        } 
                        else
                        {
                            lock (sync) // keeps the output from being distorted/numbers being printed out of order
                            {
                                Interlocked.Increment(ref currentCount); // increment currentCount
                                OutputNumber(bi, currentCount); // display the prime number
                                if (currentCount >= count) // stop the loop if enough numbers have been displayed
                                    loopState.Stop();
                            }
                        }
                    }
                });
            }
        }
        
        /// <summary>
        ///     Outputs the prime number
        /// </summary>
        /// <param name="bi">
        ///     prime number being displayed
        /// </param>
        /// <param name="temp">
        ///     Current prime number count
        /// </param>
        
        void OutputNumber(BigInteger bi, long temp)
        {
            if (temp <= count) // prevents any additional numbers from being displayed once desired number has been reached
            {
                if (temp == count) // done to match desired output format
                    Console.WriteLine(temp + ": " + bi);
                else
                    Console.WriteLine(temp + ": " + bi + "\n"); 
            }
        }

        /// <summary>
        ///     Prints the usage summary for the program to the user when it gets invalid input
        /// </summary>
        
        static void printUsage()
        {
            Console.WriteLine("Usage: dotnet run PrimeGen <bits> <count=1>");                
            Console.WriteLine("\t  - bits - the number of bits of the prime number, this must be a\n\t    multiple of 8, and at least 32 bits.");
            Console.WriteLine("\t  - count - the number of prime numbers to generate, defaults to 1");
        }

        /// <summary>
        ///     Main program which validates user input, creates an instance of the Program Object, 
        ///     then instructs it to generate, and output the desired number of prime numbers 
        /// </summary>
        /// <param name="args">
        ///     Command Line arguments
        /// </param>
        
        static void Main(string[] args)
        {
            long bits, count;
            if (args.Length < 1 || args.Length > 2) // Validate the number of command line arguments
            {
                printUsage(); // display usage message to user
                Console.WriteLine("Invalid Input: Illegal number of arguments.");
                System.Environment.Exit(0); // exit the program
            }
            if (args.Length == 1) // only <bits> was given
            {
                if (!Int64.TryParse(args[0], out bits)) // check whether <bits> is a valid number
                {
                    printUsage();
                    Console.WriteLine("Invalid Input: '" + args[0] + "' is not a valid number.");
                    System.Environment.Exit(0);
                }
                count = 1; // count defaults to 1
            } 
            else
            {
                if (!Int64.TryParse(args[0], out bits)) // check whether <bits> is a valid number
                {
                    printUsage();
                    Console.WriteLine("Invalid Input: '" + args[0] + "' is not a valid number.");
                    System.Environment.Exit(0);
                }
                if (!Int64.TryParse(args[1], out count)) // check whether <count> is a valid number
                {
                    printUsage();
                    Console.WriteLine("Invalid Input: '" + args[1] + "' is not a valid number.");
                    System.Environment.Exit(0);
                }
            }
            if (bits < 32 || bits % 8 != 0) // bits must be a multiple of 8 and >= 32
            {
                printUsage(); // display usage message to user
                Console.WriteLine("Invalid Input: <bits> must be a multiple of 8, and at least 32.");
                System.Environment.Exit(0); // exit the program
            }
            if (count < 1) // count must be at least 1...wasn't sure if it should just set it to 1, but went this route instead
            {
                printUsage();
                Console.WriteLine("Invalid Input: <count> must be at least 1.");
                System.Environment.Exit(0);
            }
            Stopwatch stopwatch = new Stopwatch(); // Keep track of time elapsed
            Console.WriteLine("BitLength: " + bits + " bits");
            Program program = new Program(bits, count); // create an instance of the Program Class
            stopwatch.Start(); // begin timing before generating prime numbers
            program.GeneratePrimes(); // Generate the desired prime numbers
            stopwatch.Stop(); // stop timing
            Console.WriteLine("Time to Generate: {0}", stopwatch.Elapsed); // outputs time elapsed
        }
    }
}