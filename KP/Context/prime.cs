using System;
using System.Numerics;
using KP.Context.Interface;

namespace KP.Context
{
    public sealed class Primality
    {
        private readonly Random _random = new Random();
        
        public bool performPhermaTest(BigInteger number, float probability_minimal)
        {
            if(number == 0)
                return false;
        
            ulong i=1;
            for(BigInteger a; (1-pow(0.5, i))<=probability_minimal; i++)
            {
                a=2+_random.Next()%(number-4);
                if(gcd_utility(a, number) != 1 || pow_big_modulo<T>(a, number - 1, number) != 1)
                    return false;
            }
        
            return true;
        }
        public bool performSolovayStrassenTest(BigInteger number, float probability_minimal)
        {
            if(number==0)
                return false;
    
            ulong i=1;
            for(BigInteger a, j; (1-pow(0.5, i))<=probability_minimal; i++)
            {
                a=_random.Next()%(number-1)+1;
                j=(number+getJacobiSymbol<T>(a, number))%number;
                if(j==0 || pow_big_modulo<T>(a, (number - 1) / 2, number) != j)
                    return false;
            }
        
            return true;
        }
        public bool performMillerRabinTest(T number, float probability_minimal)
        {
            if(number<=1 || number==4)
                return false;
            if(number<=3)
                return true;

            BigInteger d=number-1;
            while(d%2==0)
                d/=2;
    
            ulong i=1;
            for(BigInteger a, x; (1-pow(0.5, i))<=probability_minimal; i++)
            {
                a=2+_random.Next()%(number-4);
                x=pow_big_modulo<T>(a, d, number);
                if(x==1 || x==number-1)
                    continue;
            
                for( ; d!=number-1; d*=2)
                {
                    x=(x*x)%number;
                    if(x==1)
                        return false;
                    if(x==number-1)
                        break;
                }
                if(x==number-1)
                    continue;
            
                return false;
            }
        
            return true;
        }
    }
}