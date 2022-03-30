#ifndef KG_I_ALGORITHM_H
#define KG_I_ALGORITHM_H

#include "i_key.h"

class Interface_AlgorithmSymmetric
{
public:
    Interface_AlgorithmSymmetric(){}
    virtual ~Interface_AlgorithmSymmetric(){}
    
    virtual uint8_t **getKeysRound(uint8_t *key) = 0;
    virtual void encrypt(uint8_t *bytes_input, uint8_t **bytes_output, uint8_t **keys_round) = 0;
    virtual void decrypt(uint8_t *bytes_input, uint8_t **bytes_output, uint8_t **keys_round) = 0;
};

#endif
