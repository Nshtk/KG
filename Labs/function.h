#ifndef KG_FUNCTION_H
#define KG_FUNCTION_H

#include "utility.h"

template<typename T>
uint8_t *permute(uint8_t *input_bytes, const uint8_t *p_box)
{
    uint64_t input_bits=joinBits<T>(input_bytes);
    uint64_t output_bits=0;
    uint8_t size_bits=sizeof(T)*8;
    
    for(uint8_t i=0 ; i<size_bits; i++)
        output_bits |= ((input_bits >> (size_bits - p_box[i]))&0x01) << ((size_bits-1) - i);
    
    return splitBitsTo8<T>(output_bits);
}

uint8_t *substitute(const uint8_t *bytes, const uint8_t s_boxes[8][4][16], uint8_t k)
{
    k/=6;
    uint8_t first_last, middle;
    uint8_t *bytes_s=new uint8_t[4]();
    thread threads[k];
    
    
    for(uint8_t i=0, j=0; j<k; i++, j+=2)           // TODO add additional vars first_last, middle to handle false sharing
    {
        threads[j]=thread([&](uint8_t i_t, uint8_t j_t)
        {
            first_last=((bytes[j_t] >> 6) & 0x2) | ((bytes[j_t] >> 2) & 0x1);
            middle=(bytes[j_t] >> 3) & 0xF;
            bytes_s[i_t]=s_boxes[j_t][first_last][middle];
        }, i, j);
    
        threads[j+1]=thread([&](uint8_t i_t, uint8_t j_t)
        {
            first_last=((bytes[j_t] >> 6) & 0x2) | ((bytes[j_t] >> 2) & 0x1);
            middle=(bytes[j_t] >> 3) & 0xF;
            bytes_s[i_t]=(bytes_s[i_t]<<4) | s_boxes[j_t][first_last][middle];
        }, i, j+1);
    }
    for(uint8_t w=0; w<k; w++)
        threads[w].join();
    
    return bytes_s;
}

template <typename T>
int8_t getJacobiSymbol(T a, T b)
{
    if(gcd_utility<T>(a, b) != 1)
        return 0;
    
    int8_t r=1;
    
    a%=b;
    for(T t; a!=0; a%=b)
    {
        while(a%2==0)
        {
            t=b%8;
            a/=2;
            if(t==3 || t==5)
                r*=-1;
        }
        swap<T>(a, b);
        if(a%4==3 && b%4==3)
            r*=-1;
    }

    return b==1 ? r : 0;
}

/*int8_t getJacobiSymbol(long long a, long long b)
{
    if(gcd_utility(a, b)!=1)
        return 0;
    
    unsigned t=0;
    int8_t r=1;
    
    if(a<0)
    {
        a=-a;
        if(b%4==3)
            r*=-1;
    }
    while(a!=0)
    {
        while(a%2==0)
        {
            t++;
            a/=2;
        }
        
        if(t%2==1)
            if(b%8==3 || b%8==5)
                r*=-1;
        
        if(a%4==3 && a%4==b%4)
            r*=-1;
    }
    
    return r;
}*/

/*int8_t getLegendreSymbol(int a, int b)
{
    if(b==2 && !isPrime(b))
        return 0;
    return getJacobiSymbol(a, b);
}*/

#endif