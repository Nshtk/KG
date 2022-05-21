using System;
using System.Numerics;

namespace KP.Context
{
    public static class Primality
    {
        private static sbyte getJacobiSymbol(BigInteger a, BigInteger b)
        {
            if(BigInteger.GreatestCommonDivisor(a, b)!=1)
                return 0;
    
            sbyte r=1;
    
            a%=b;
            for(BigInteger t; a!=0; a%=b)
            {
                while(a%2==0)
                {
                    t=b%8;
                    a/=2;
                    if(t==3 || t==5)
                        r*=-1;
                }

                (a, b)=(b, a);
                if(a%4==3 && b%4==3)
                    r*=-1;
            }

            return b==1 ?r :(sbyte)0;
        }
        
        public static bool performFermatTest(BigInteger number, double probability_minimal)
        {
            if(number==0 || number==4)
                return false;
        
            ulong i=1;
            for(BigInteger a; (1-Math.Pow(0.5, i))<=probability_minimal; i++)
            {
                a=2+Utility.Rng.Next()%(number-4);
                if(BigInteger.GreatestCommonDivisor(a, number)!=1 || BigInteger.ModPow(a, number-1, number)!=1)
                    return false;
            }
        
            return true;
        }
        public static bool performSolovayStrassenTest(BigInteger number, double probability_minimal)
        {
            if(number==0)
                return false;
    
            ulong i=1;
            for(BigInteger a, j; (1-Math.Pow(0.5, i))<=probability_minimal; i++)
            {
                a=Utility.Rng.Next()%(number-1)+1;
                j=(number+getJacobiSymbol(a, number))%number;
                if(j==0 || BigInteger.ModPow(a, (number-1)/2, number)!=j)
                    return false;
            }
        
            return true;
        }
        public static bool performMillerRabinTest(BigInteger number, double probability_minimal)
        {
            if(number<=1 || number==4)
                return false;
            if(number<=3)
                return true;

            BigInteger d=number-1;
            while(d%2==0)
                d/=2;
    
            ulong i=1;
            for(BigInteger a, x; (1-Math.Pow(0.5, i))<=probability_minimal; i++)
            {
                a=2+Utility.Rng.Next()%(number-4);
                x=BigInteger.ModPow(a, d, number);
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