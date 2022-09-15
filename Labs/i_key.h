#ifndef KG_I_KEY_H
#define KG_I_KEY_H

class Interface_KeyExpansion
{
public:
    Interface_KeyExpansion(){}
    virtual ~Interface_KeyExpansion(){}
    
    virtual uint8_t **generateKeysRound(uint8_t *key) = 0;
};

class Interface_RoundCiphering
{
public:
    Interface_RoundCiphering(){}
    virtual ~Interface_RoundCiphering(){}
    
    virtual uint8_t *performRound(uint8_t *bytes, uint8_t *key_round) = 0;
};

#endif
