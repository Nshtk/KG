#ifndef KG_I_ALGORITHM_H
#define KG_I_ALGORITHM_H

#include "i_key.h"

class Interface_Algorithm
{
public:
    Interface_Algorithm(){}
    virtual ~Interface_Algorithm(){};
    
    virtual uint8_t **getKeysRound(uint8_t *key) = 0;
    virtual void encrypt(uint8_t *bytes_input, uint8_t **bytes_output, uint8_t **keys_round) = 0;
    virtual void decrypt(uint8_t *bytes_input, uint8_t **bytes_output, uint8_t **keys_round) = 0;
};

#endif
