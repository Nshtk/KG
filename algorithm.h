#ifndef KG_ALGORITHM_H
#define KG_ALGORITHM_H

#include <initializer_list>
#include <vector>
#include <random>
#include <chrono>
#include <algorithm>
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
        uint8_t *ofb_tmp_bytes;//=new uint8_t[8];
        uint64_t cbc_tmp_bits;
        //vector<uint64_t> ctr_numbers; uint64_t ctr_number;
        hash<uint64_t> h;
        switch(ciphering_mode)
        {
            case CipheringMode_ECB:
                for( ; j<args[0]/args[1]; i+=args[1], j++)
                    algorithm->encrypt(&bytes_input[i], &bytes_output[j], keys_round);
                if(i-args[0]>0)
                    algorithm->encrypt(pad(&bytes_input[i], args[0]-i, args[1]), &bytes_output[j], keys_round);
                break;
                
            case CipheringMode_CBC:
                algorithm->encrypt(splitBitsTo8<uint64_t>(joinBits<uint64_t>(&bytes_input[i])^init_vector), &bytes_output[j], keys_round);
                cbc_tmp_bits=joinBits<uint64_t>(bytes_output[j]);
                i+=args[1]; j++;
                
                for( ; j<args[0]/args[1]; i+=args[1], j++)
                {
                    algorithm->encrypt(splitBitsTo8<uint64_t>(cbc_tmp_bits^joinBits<uint64_t>(&bytes_input[i])), &bytes_output[j], keys_round);
                    cbc_tmp_bits=joinBits<uint64_t>(bytes_output[j]);
                }
                if(i-args[0]>0)
                    algorithm->encrypt(splitBitsTo8<uint64_t>(cbc_tmp_bits^joinBits<uint64_t>(pad(&bytes_input[i], args[0]-i, args[1]))), &bytes_output[j], keys_round);
                break;
                
            case CipheringMode_CFB:
                algorithm->encrypt(splitBitsTo8<uint64_t>(init_vector), &bytes_output[j], keys_round);
                bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(&bytes_input[i]));
                i+=args[1]; j++;
        
                for( ; j<args[0]/args[1]; i+=args[1], j++)
                {
                    algorithm->encrypt(bytes_output[j-1], &bytes_output[j], keys_round);
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(&bytes_input[i]));
                }
                if(i-args[0]>0)
                {
                    algorithm->encrypt(bytes_output[j-1], &bytes_output[j], keys_round);
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(pad(&bytes_input[i], args[0]-i, args[1])));
                }
                break;
                
            case CipheringMode_OFB:
                algorithm->encrypt(splitBitsTo8<uint64_t>(init_vector), &bytes_output[j], keys_round);
                ofb_tmp_bytes=bytes_output[j];
                bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(&bytes_input[i]));
                i+=args[1]; j++;
        
                for( ; j<args[0]/args[1]; i+=args[1], j++)
                {
                    algorithm->encrypt(ofb_tmp_bytes, &bytes_output[j], keys_round);
                    ofb_tmp_bytes=bytes_output[j];
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(&bytes_input[i]));
                }
                if(i-args[0]>0)
                {
                    algorithm->encrypt(ofb_tmp_bytes, &bytes_output[j], keys_round);
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(pad(&bytes_input[i], args[0]-i, args[1])));
                }
                break;
            
            case CipheringMode_CTR:
                /*ctr_number=args[0]/args[1];
                cbc_tmp_bits=rand();
                cbc_tmp_bits<=ctr_number ? cbc_tmp_bits+=ctr_number : cbc_tmp_bits-=ctr_number;
                for(ctr_number=cbc_tmp_bits-ctr_number; ctr_number<cbc_tmp_bits; ctr_number++)
                    ctr_numbers.push_back(ctr_number);
                shuffle(ctr_numbers.begin(), ctr_numbers.end(), default_random_engine(chrono::system_clock::now().time_since_epoch().count()));*/
        
                for( ; j<args[0]/args[1]; i+=args[1], j++)
                {
                    algorithm->encrypt(splitBitsTo8<uint64_t>(init_vector^j), &bytes_output[j], keys_round);
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(&bytes_input[i]));
                }
                if(i-args[0]>0)
                {
                    algorithm->encrypt(splitBitsTo8<uint64_t>(init_vector^j), &bytes_output[j], keys_round);
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(pad(&bytes_input[i], args[0]-i, args[1])));
                }
                break;
            
            case CipheringMode_RD:
                bytes_output[args[0]/args[1]+1]=new uint8_t[args[1]];
                cbc_tmp_bits=init_vector;
                algorithm->encrypt(splitBitsTo8<uint64_t>(init_vector), &bytes_output[j], keys_round);
                j++;
                
                for(uint64_t delta=init_vector&0xFFFFFFFFFFFFFFFF; j<args[0]/args[1]+1; i+=args[1], j++, cbc_tmp_bits+=delta)
                {
                    algorithm->encrypt(splitBitsTo8<uint64_t>(joinBits<uint64_t>(&bytes_input[i])^cbc_tmp_bits), &bytes_output[j], keys_round);
                }
                if(i-args[0]>0)
                {
                    algorithm->encrypt(splitBitsTo8<uint64_t>(joinBits<uint64_t>(pad(&bytes_input[i], args[0]-i, args[1]))^cbc_tmp_bits), &bytes_output[j], keys_round);
                }
                break;
            
            case CipheringMode_RD_H:
                bytes_output[args[0]/args[1]+1]=new uint8_t[args[1]];
                bytes_output[args[0]/args[1]+2]=new uint8_t[args[1]];
                cbc_tmp_bits=init_vector;
                algorithm->encrypt(splitBitsTo8<uint64_t>(init_vector), &bytes_output[j], keys_round); j++;
                algorithm->encrypt(splitBitsTo8<uint64_t>(h(init_vector)), &bytes_output[j], keys_round); j++;
        
                for(uint64_t delta=init_vector&0xFFFFFFFFFFFFFFFF; j<args[0]/args[1]+1; i+=args[1], j++, cbc_tmp_bits+=delta)
                {
                    algorithm->encrypt(splitBitsTo8<uint64_t>(joinBits<uint64_t>(&bytes_input[i])^cbc_tmp_bits), &bytes_output[j], keys_round);
                }
                if(i-args[0]>0)
                {
                    algorithm->encrypt(splitBitsTo8<uint64_t>(joinBits<uint64_t>(pad(&bytes_input[i], args[0]-i, args[1]))^cbc_tmp_bits), &bytes_output[j], keys_round);
                }
                break;
        }
    }
    void decrypt(uint8_t *bytes_input, uint8_t **bytes_output, uint8_t **keys_round) override
    {
        unsigned i=0, j=0;
        uint8_t *ofb_tmp_bytes;
        uint64_t cbc_tmp_bits;
        hash<uint64_t> h;
        
        switch(ciphering_mode)
        {
            case CipheringMode_ECB:
                for( ; j<args[0]/args[1]; i+=args[1], j++)
                    algorithm->decrypt(&bytes_input[i], &bytes_output[j], keys_round);
                if(i-args[0]>0)
                    algorithm->decrypt(&bytes_input[i], &bytes_output[j], keys_round);
                break;
                
            case CipheringMode_CBC:
                cbc_tmp_bits=joinBits<uint64_t>(&bytes_input[i]);
                algorithm->decrypt(&bytes_input[i], &bytes_output[j], keys_round);
                bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^init_vector);
                i+=args[1]; j++;
        
                for( ; j<args[0]/args[1]; i+=args[1], j++)
                {
                    algorithm->decrypt(&bytes_input[i], &bytes_output[j], keys_round);
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^cbc_tmp_bits);
                    cbc_tmp_bits=joinBits<uint64_t>(&bytes_input[i]);
                }
                if(i-args[0]>0)
                {
                    algorithm->decrypt(&bytes_input[i], &bytes_output[j], keys_round);
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^cbc_tmp_bits);
                }
                break;
                
            case CipheringMode_CFB:
                algorithm->encrypt(splitBitsTo8<uint64_t>(init_vector), &bytes_output[j], keys_round);
                bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(&bytes_input[i]));
                i+=args[1]; j++;
                
                for( ; j<args[0]/args[1]; i+=args[1], j++)
                {
                    algorithm->encrypt(&bytes_input[i-args[1]], &bytes_output[j], keys_round);
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(&bytes_input[i]));
                }
                if(i-args[0]>0)
                {
                    algorithm->encrypt(&bytes_input[i-args[1]], &bytes_output[j], keys_round);
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(&bytes_input[i]));
                }
                break;
                
            case CipheringMode_OFB:
                algorithm->encrypt(splitBitsTo8<uint64_t>(init_vector), &bytes_output[j], keys_round);
                ofb_tmp_bytes=bytes_output[j];
                bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(&bytes_input[i]));
                i+=args[1]; j++;
        
                for( ; j<args[0]/args[1]; i+=args[1], j++)
                {
                    algorithm->encrypt(ofb_tmp_bytes, &bytes_output[j], keys_round);
                    ofb_tmp_bytes=bytes_output[j];
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(&bytes_input[i]));
                }
                if(i-args[0]>0)
                {
                    algorithm->encrypt(ofb_tmp_bytes, &bytes_output[j], keys_round);
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(&bytes_input[i]));
                }
                break;
            
            case CipheringMode_CTR:
                for( ; j<args[0]/args[1]; i+=args[1], j++)
                {
                    algorithm->encrypt(splitBitsTo8<uint64_t>(init_vector^j), &bytes_output[j], keys_round);
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(&bytes_input[i]));
                }
                if(i-args[0]>0)
                {
                    algorithm->encrypt(splitBitsTo8<uint64_t>(init_vector^j), &bytes_output[j], keys_round);
                    bytes_output[j]=splitBitsTo8<uint64_t>(joinBits<uint64_t>(bytes_output[j])^joinBits<uint64_t>(&bytes_input[i]));
                }
                break;
            
            case CipheringMode_RD:
                bytes_output[args[0]/args[1]+1]=new uint8_t[args[1]];
                cbc_tmp_bits=init_vector;
                algorithm->encrypt(splitBitsTo8<uint64_t>(init_vector), &bytes_output[j], keys_round);
                j++;

                for(uint64_t delta=init_vector&0xFFFFFFFFFFFFFFFF; j<args[0]/args[1]+1; i+=args[1], j++, cbc_tmp_bits+=delta)
                {
                    algorithm->encrypt(splitBitsTo8<uint64_t>(joinBits<uint64_t>(&bytes_input[i])^cbc_tmp_bits), &bytes_output[j], keys_round);
                }
                if(i-args[0]>0)
                {
                    algorithm->encrypt(splitBitsTo8<uint64_t>(joinBits<uint64_t>(&bytes_input[i])^cbc_tmp_bits), &bytes_output[j], keys_round);
                }
                break;
            
            case CipheringMode_RD_H:
                bytes_output[args[0]/args[1]+1]=new uint8_t[args[1]];
                bytes_output[args[0]/args[1]+2]=new uint8_t[args[1]];
                cbc_tmp_bits=init_vector;
                algorithm->encrypt(splitBitsTo8<uint64_t>(init_vector), &bytes_output[j], keys_round); j++;
                algorithm->encrypt(splitBitsTo8<uint64_t>(h(init_vector)), &bytes_output[j], keys_round); j++;
        
                for(uint64_t delta=init_vector&0xFFFFFFFFFFFFFFFF; j<args[0]/args[1]+1; i+=args[1], j++, cbc_tmp_bits+=delta)
                {
                    algorithm->encrypt(splitBitsTo8<uint64_t>(joinBits<uint64_t>(&bytes_input[i])^cbc_tmp_bits), &bytes_output[j], keys_round);
                }
                if(i-args[0]>0)
                {
                    algorithm->encrypt(splitBitsTo8<uint64_t>(joinBits<uint64_t>(&bytes_input[i])^cbc_tmp_bits), &bytes_output[j], keys_round);
                }
                break;
        }
    }
};

#endif
