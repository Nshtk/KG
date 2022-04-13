#ifndef KG_I_PRIME_H
#define KG_I_PRIME_H

class Interface_Primality
{
public:
    Interface_Primality(){};
    virtual ~Interface_Primality(){};
    
    virtual bool performTest(long long number, float probability_minimal) = 0;
};

#endif
