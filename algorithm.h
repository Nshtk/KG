#ifndef KG_ALGORITHM_H
#define KG_ALGORITHM_H

#include <initializer_list>
#include <vector>
#include "i_algorithm.h"
#include "key.h"

class FeistelNet : public Interface_AlgorithmSymmetric
{
protected:
    Interface_KeyExpansion *key_expansion;
    Interface_RoundCiphering *round_ciphering;

public:
    FeistelNet( Interface_KeyExpansion *key_expansion, Interface_RoundCiphering *round_ciphering) : key_expansion(key_expansion), round_ciphering(round_ciphering)
    {}
    ~FeistelNet()
    {
        delete key_expansion, delete round_ciphering;
    }
    
    void encrypt(uint8_t *bytes_input, uint8_t **bytes_output, uint8_t **keys_round) override
    {
        for(int8_t i=0; i<16; i++)
            bytes_input=round_ciphering->perform(bytes_input, keys_round[i]);
        *bytes_output=swapParts64(bytes_input);
    }
    void decrypt(uint8_t *bytes_input, uint8_t **bytes_output, uint8_t **keys_round) override
    {
        for(int8_t i=15; i>-1; i--)
            bytes_input=round_ciphering->perform(bytes_input, keys_round[i]);
        *bytes_output=swapParts64(bytes_input);
    }
};

class AlgorithmDES : public FeistelNet
{
public:
    AlgorithmDES(Interface_KeyExpansion *keyExpansion, Interface_RoundCiphering *roundCiphering) : FeistelNet(keyExpansion, roundCiphering)
    {}
    
    uint8_t **getKeysRound(uint8_t *key) override
    {
        return FeistelNet::key_expansion->generateKeysRound(key);
    }
    void encrypt(uint8_t *bytes_input, uint8_t **bytes_output, uint8_t **keys_round) override
    {
        uint8_t *p=permute<uint64_t>(bytes_input, PI);
        FeistelNet::encrypt(p, &p, keys_round);
        *bytes_output=permute<uint64_t>(p, PF);
        delete p;
    }
    void decrypt(uint8_t *bytes_input, uint8_t **bytes_output, uint8_t **keys_round) override
    {
        uint8_t *p=permute<uint64_t>(bytes_input, PI);
        FeistelNet::decrypt(p, &p, keys_round);
        *bytes_output=permute<uint64_t>(p, PF);
        delete p;
    }
};

template<class T>
class AlgorithmSymmetric : public Interface_AlgorithmSymmetric
{
private:
    Interface_AlgorithmSymmetric *algorithm;
    CipheringMode ciphering_mode;
    uint8_t *key;
    uint64_t init_vector;
    vector<T> args;
public:
    AlgorithmSymmetric(Interface_AlgorithmSymmetric *algorithm, CipheringMode ciphering_mode, uint8_t *key, uint64_t init_vector, initializer_list<T> args) : algorithm(algorithm), ciphering_mode(ciphering_mode), key(key), init_vector(init_vector), args(args)
    {}
    ~AlgorithmSymmetric() override
    {
        delete algorithm;
    }
    
    uint8_t **getKeysRound(uint8_t *key)
    {
        return algorithm->getKeysRound(key);
    }
    
    void encrypt(uint8_t *bytes_input, uint8_t **bytes_output, uint8_t **keys_round) override
    {
        unsigned i=0, j=0;
        //uint8_t *tmp=new uint8_t[8];
        uint64_t tmp_bits;
        
        switch(ciphering_mode)
        {
            case CipheringMode_ECB:
                for( ; i<args[0]; i+=args[1], j++)
                {
                    algorithm->encrypt(&bytes_input[i], &bytes_output[j], keys_round);
                }
                if(i-args[0]>0)
                {
                    i-=args[1];
                    algorithm->encrypt(pad(&bytes_input[i], args[0]-i, args[1]), &bytes_output[j], keys_round);
                }
                break;
                
            case CipheringMode_CBC:
                algorithm->encrypt(splitBitsTo8<uint64_t>(joinBits<uint64_t>(&bytes_input[i])^init_vector), &bytes_output[j], keys_round);
                tmp_bits=joinBits<uint64_t>(bytes_output[j]);
                i+=args[1]; j++;
                
                for( ; i<args[0]; i+=args[1], j++)
                {
                    algorithm->encrypt(splitBitsTo8<uint64_t>(tmp_bits^joinBits<uint64_t>(&bytes_input[i])), &bytes_output[j], keys_round);
                    tmp_bits=joinBits<uint64_t>(bytes_output[j]);
                }
                if(i-args[0]>0)
                {
                    i-=args[1];
                    algorithm->encrypt(splitBitsTo8<uint64_t>(tmp_bits^joinBits<uint64_t>(pad(&bytes_input[i], args[0]-i, args[1]))), &bytes_output[j], keys_round);
                }
                break;
                
            case CipheringMode_CFB:
                algorithm->encrypt(splitBitsTo8<uint64_t>(init_vector), &bytes_output[j], keys_round);
                bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(&bytes_input[i]));
                i+=args[1]; j++;
        
                for( ; i<args[0]; i+=args[1], j++)
                {
                    algorithm->encrypt(bytes_output[j-1], &bytes_output[j], keys_round);
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(&bytes_input[i]));
                }
                if(i-args[0]>0)
                {
                    i-=args[1];
                    algorithm->encrypt(bytes_output[j-1], &bytes_output[j], keys_round);
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(pad(&bytes_input[i], args[0]-i, args[1])));
                }
                break;
                
            case CipheringMode_OFB:
                break;
            
            case CipheringMode_CTR:
                break;
            
            case CipheringMode_RD:
                break;
            
            case CipheringMode_RD_H:
                break;
        }
    }
    void decrypt(uint8_t *bytes_input, uint8_t **bytes_output, uint8_t **keys_round) override
    {
        unsigned i=0, j=0;
        //uint8_t *tmp=new uint8_t[8];
        uint64_t tmp_bits;
        
        switch(ciphering_mode)
        {
            case CipheringMode_ECB:
                for( ; i<args[0]; i+=args[1], j++)
                {
                    algorithm->decrypt(&bytes_input[i], &bytes_output[j], keys_round);
                }
                if(i-args[0]>0)
                {
                    i-=args[1];
                    algorithm->decrypt(pad(&bytes_input[i], args[0]-i, args[1]), &bytes_output[j], keys_round);
                }
                break;
                
            case CipheringMode_CBC:
                tmp_bits=joinBits<uint64_t>(&bytes_input[i]);
                algorithm->decrypt(&bytes_input[i], &bytes_output[j], keys_round);
                bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^init_vector);
                i+=args[1]; j++;
        
                for( ; i<args[0]; i+=args[1], j++)
                {
                    algorithm->decrypt(&bytes_input[i], &bytes_output[j], keys_round);
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^tmp_bits);
                    tmp_bits=joinBits<uint64_t>(&bytes_input[i]);
                }
                if(i-args[0]>0)
                {
                    i-=args[1];
                    algorithm->decrypt(pad(&bytes_input[i], args[0]-i, args[1]), &bytes_output[j], keys_round);
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^tmp_bits);
                }
                break;
                
            case CipheringMode_CFB:
                algorithm->encrypt(splitBitsTo8<uint64_t>(init_vector), &bytes_output[j], keys_round);
                bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(&bytes_input[i]));
                i+=args[1]; j++;
                /*for(unsigned j=0; j<8; j++)
                    printf("%c", p[j]);*/
                for( ; i<args[0]; i+=args[1], j++)
                {
                    algorithm->encrypt(&bytes_input[i-args[1]], &bytes_output[j], keys_round);
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(&bytes_input[i]));
                }
                if(i-args[0]>0)
                {
                    i-=args[1];
                    algorithm->encrypt(&bytes_input[i-args[1]], &bytes_output[j], keys_round);
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(pad(&bytes_input[i], args[0]-i, args[1])));
                }
                break;
                
            case CipheringMode_OFB:
                break;
            
            case CipheringMode_CTR:
                break;
            
            case CipheringMode_RD:
                break;
            
            case CipheringMode_RD_H:
                break;
        }
    }
};

#endif
