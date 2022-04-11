#ifndef KG_PRIME_H
#define KG_PRIME_H

#include "i_prime.h"
#include <cmath>

class Primality_PhermaTest : public Interface_Primality
{
public:
    Primality_PhermaTest()
    {}
    
    bool test(long long number, float probability_minimal) override
    {
        if(number==1)
            return true;
        
        for(long long i=1, a; (1-pow(0.5, i))<=probability_minimal; i++)
        {
            a=2+rand()%(number-4);
            if(gcd(a, number)!=1 || pow_long(a, number-1, number)!=1)
                return false;
        }
        
        return true;
    }
};

class Primality_SoloveiStrassenTest : public Interface_Primality
{
public:
    Primality_SoloveiStrassenTest()
    {}
    
    bool test(long long number, float probability_minimal) override
    {
        if(number==1)
            return true;

        for(long long i=1, a, j; (1-pow(0.5, i))<=probability_minimal; i++)
        {
            a=rand() % (number-1)+1;
            j=(number+getJacobiSymbol(a, number))%number;
            if(!j || pow_long(a, (number-1)/2, number)!=j)
                return false;
        }
        
        return true;
    }
};

class Primality_MillerRabinTest : public Interface_Primality
{
public:
    Primality_MillerRabinTest()
    {}
    
    bool test(long long number, float probability_minimal) override
    {
        if(number==1)
            return true;
        
        long long d=number-1;
        while(d%2==0)
            d/=2;
        
        for(long long i=1, a, x; (1-pow(0.5, i))<=probability_minimal; i++)
        {
            a=2+rand()%(number-4);
            x=pow_long(a, d, number);
            if(x==1 || x==number-1)
                return true;
            
            for( ; d!=number-1; d*=2)
            {
                x=(x*x)%number;
                if(x==1)
                    return false;
                if(x==number-1)
                    return true;
            }
        }
        
        return true;
    }
};

#endif
