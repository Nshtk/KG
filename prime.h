#ifndef KG_PRIME_H
#define KG_PRIME_H

#include "i_prime.h"
#include "utility.h"
#include "function.h"
#include <cmath>
//#include <boost/random.hpp>

template<typename T>
class Primality_FermatTest : public Interface_Primality<T>
{
public:
    Primality_FermatTest()
    {}
    
    bool performTest(T number, float probability_minimal) override
    {
        if(number==0)
            return false;
        if(number==1)
            return true;
        
        long long i=1;
        for(T a; (1-pow(0.5, i))<=probability_minimal; i++)
        {
            a=2+rand()%(number-4);
            if(gcd_utility(a, number) != 1 || pow_big_modulo<T>(a, number - 1, number) != 1)
                return false;
        }
        
        return true;
    }
};

template<typename T>
class Primality_SolovayStrassenTest : public Interface_Primality<T>
{
public:
    Primality_SolovayStrassenTest()
    {}
    
    bool performTest(T number, float probability_minimal) override
    {
        if(number==0)
            return false;
        if(number==1)
            return true;
    
        long long i=1;
        for(T a, j; (1-pow(0.5, i))<=probability_minimal; i++)
        {
            a=rand()%(number-1)+1;
            j=(number+getJacobiSymbol<T>(a, number))%number;
            if(!j || pow_big_modulo<T>(a, (number - 1) / 2, number) != j)
                return false;
        }
        
        return true;
    }
};

template<typename T>
class Primality_MillerRabinTest : public Interface_Primality<T>
{
public:
    Primality_MillerRabinTest()
    {}
    
    bool performTest(T number, float probability_minimal) override
    {
        if(number<=1 || number==4)
            return false;
        if(number<=3)
            return true;

        T d=number-1;
        while(d%2==0)
            d/=2;
    
        long long i=1;
        for(T a, x; (1-pow(0.5, i))<=probability_minimal; i++)
        {
            a=2+rand()%(number-4);
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
};

#endif
