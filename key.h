#ifndef KG_KEY_H
#define KG_KEY_H

#include "utility.h"

class Interface_KeyExpansion
{
public:
    Interface_KeyExpansion(){}
    virtual ~Interface_KeyExpansion(){}

    virtual uint64_t *generateKeysRound(uint8_t *key) = 0;
};

class Interface_KeyEncryption
{
public:
    Interface_KeyEncryption(){}
    virtual ~Interface_KeyEncryption(){}

    virtual uint8_t **encrypt(uint8_t *bytes, uint8_t *key_round) = 0;
};

class Interface_AlgorithmSymmetric
{
public:
    Interface_AlgorithmSymmetric(){}
    virtual ~Interface_AlgorithmSymmetric(){}

    virtual uint8_t *adjustKeyRound(uint8_t *key) = 0;
    virtual void encrypt(uint8_t *bytes_input, uint8_t **bytes_output, uint8_t *key) = 0;
    virtual void decrypt(uint8_t *bytes_input, uint8_t **bytes_output, uint8_t *key) = 0;
};

class KeyExpansionFeistel : public Interface_KeyExpansion
{
private:
    uint32_t *getKeyParts28(uint64_t key)
    {
        uint32_t *key_parts = new uint32_t[2]();

        for (uint8_t i = 0; i < 28; ++i)
        {
            key_parts[0] |= ((key >> (64-KP1[i])) & 0x01) << (31-i);
            key_parts[1] |= ((key >> (64-KP2[i])) & 0x01) << (31-i);
        }

        return key_parts;
    }
    void shiftKeyParts(uint32_t *key_parts, uint8_t bits_shifting)
    {
        key_parts[0]=(((key_parts[0]) << (bits_shifting)) | ((key_parts[0]) >> (-(bits_shifting)&27))) & (((uint64_t)1 << 32)-1);
        key_parts[1]=(((key_parts[1]) << (bits_shifting)) | ((key_parts[1]) >> (-(bits_shifting)&27))) & (((uint64_t)1 << 32)-1);
    }
public:
    KeyExpansionFeistel(){}
    ~KeyExpansionFeistel(){}

    uint64_t *generateKeysRound(uint8_t *key) override
    {
        uint32_t *key_parts=getKeyParts28(joinBits<uint64_t>(key, sizeof(uint64_t)));
        uint64_t *keys_round=new uint64_t[16]();
        uint64_t bits56;

        for (uint8_t i=0; i<16; i++)
        {
            if(i==0 || i==1 || i==8 || i==15)
                shiftKeyParts(key_parts, 1);
            else
                shiftKeyParts(key_parts, 2);
            bits56=joinBitsParts28To56(key_parts);
            for(uint8_t j=0 ; j<48; j++)
                keys_round[i] |= ((bits56 >> (64-CP[j])) & 0x01) << (63-j);
        }
        delete key_parts;

        return keys_round;
    }
};

class AlgorithmDES : public Interface_AlgorithmSymmetric
{
private:
    Interface_KeyExpansion *key_expansion;

    uint8_t *adjustKeyRound(uint8_t *key) override
    {
        return nullptr;
    }

    uint32_t functionF(uint32_t bits_input_part, uint64_t key_round)
    {
        uint64_t bits_input_part_expanded48=0;

        for (uint8_t i=0 ; i<48; i++)
            bits_input_part_expanded48 |= (uint64_t)((bits_input_part >> (32 - PE[i])) & 0x01) << (63 - i);
        bits_input_part_expanded48^=key_round;

        return joinBits<uint32_t>(permute(substitute(splitBits48To6(bits_input_part_expanded48), S_BOXES, 48), PFF), sizeof(uint32_t));
    }
    void doFeistelRound(uint32_t *bits_input_parts, uint64_t key_round)
    {
        uint32_t tmp=bits_input_parts[1];

        bits_input_parts[1]=functionF(bits_input_parts[1], key_round)^bits_input_parts[0];
        bits_input_parts[0]=tmp;
    }
public:
    AlgorithmDES(Interface_KeyExpansion *key_expansion) : key_expansion(key_expansion)
    {

    }

    void encrypt(uint8_t *bytes_input, uint8_t **bytes_output, uint8_t *key) override
    {
        uint64_t *keys_round=key_expansion->generateKeysRound(key);
        uint32_t *bits_input_parts;

        bits_input_parts=splitBits64To32(joinBits<uint64_t>(permute(bytes_input, PI), sizeof(uint64_t)));
        for(uint8_t i=0; i<16; i++)
            doFeistelRound(bits_input_parts, keys_round[i]);
        swap(bits_input_parts[0], bits_input_parts[1]);

        *bytes_output=permute(splitBits64To8(joinBitsParts32To64(bits_input_parts)), PF);

        delete keys_round;
    }
    void decrypt(uint8_t *bytes_input, uint8_t **bytes_output, uint8_t *key) override
    {
        uint64_t *keys_round=key_expansion->generateKeysRound(key);
        uint32_t *bits_input_parts;

        bits_input_parts=splitBits64To32(joinBits<uint64_t>(permute(bytes_input, PI), sizeof(uint64_t)));
        for(int8_t i=15; i>-1; i--)
            doFeistelRound(bits_input_parts, keys_round[i]);
        swap(bits_input_parts[0], bits_input_parts[1]);

        *bytes_output=permute(splitBits64To8(joinBitsParts32To64(bits_input_parts)), PF);

        delete keys_round;
    }
};

class FeistelNet : public Interface_AlgorithmSymmetric
{
private:
    void encrypt(uint8_t *bytes_input, uint8_t **bytes_output, uint8_t *key) override
    {

    }
public:
    FeistelNet(Interface_KeyEncryption *key_encrypton, Interface_KeyExpansion *key_expansion)
    {}
};

#endif
