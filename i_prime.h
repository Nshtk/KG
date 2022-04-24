#ifndef KG_I_PRIME_H
#define KG_I_PRIME_H

#include <boost/multiprecision/cpp_int.hpp>

template<typename T>
class Interface_Primality
{
public:
    Interface_Primality(){};
    virtual ~Interface_Primality(){};

    virtual bool performTest(T number, float probability_minimal) = 0;
};

#endif
