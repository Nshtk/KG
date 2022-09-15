#ifndef KG_KEY_H
#define KG_KEY_H

#include <boost/random.hpp>
#include <boost/integer/extended_euclidean.hpp>

class KeyExpansionFeistel : public Interface_KeyExpansion
{
private:
    uint32_t *getKeyParts28(uint64_t key)
    {
        uint32_t *key_parts = new uint32_t[2]();
        thread threads[28];
        
        for(uint8_t i=0; i<28; ++i)
        {
            threads[i]=thread([&](uint8_t i)
            {
                key_parts[0]|=((key >> (64-KP1[i]))&0x01) << (31-i);
                key_parts[1]|=((key >> (64-KP2[i]))&0x01) << (31-i);
            }, i);
        }
        for(uint8_t w=0; w<28; w++)
            threads[w].join();
        
        return key_parts;
    }
    void shiftKeyParts(uint32_t *key_parts, uint8_t bits_shifting)
    {
        key_parts[0]=(((key_parts[0]) << (bits_shifting)) | ((key_parts[0]) >> (-(bits_shifting)&27))) & (((uint64_t)1<<32)-1);
        key_parts[1]=(((key_parts[1]) << (bits_shifting)) | ((key_parts[1]) >> (-(bits_shifting)&27))) & (((uint64_t)1<<32)-1);
    }
public:
    KeyExpansionFeistel(){}
    ~KeyExpansionFeistel() override
    {}

    uint8_t **generateKeysRound(uint8_t *key) override
    {
        uint64_t bits56, key64_round;
        uint32_t *key_parts=getKeyParts28(joinBits<uint64_t>(key));
        uint8_t **keys_round=new uint8_t*[16];
        uint8_t j;
        
        for (uint8_t i=0; i<16; i++)
        {
            if(i==0 || i==1 || i==8 || i==15)
                shiftKeyParts(key_parts, 1);
            else
                shiftKeyParts(key_parts, 2);
            bits56=joinBitsParts28To56(key_parts);
            for(j=key64_round=0; j<48; j++)
                key64_round|=((bits56 >> (64-CP[j])) & 0x01) << (63-j);

            keys_round[i]=splitBitsTo8<uint64_t>(key64_round);
        }
        delete key_parts;

        return keys_round;
    }
};

class RoundCipheringFeistel : public Interface_RoundCiphering
{
private:
    uint32_t functionF(uint32_t bits_input_part, uint64_t key_round)
    {
        uint64_t bits_input_part_expanded48=0;
        
        for (uint8_t i=0 ; i<48; i++)
            bits_input_part_expanded48 |= (uint64_t)((bits_input_part >> (32-PE[i])) & 0x01) << (63-i);
        bits_input_part_expanded48^=key_round;

        return joinBits<uint32_t>(permute<uint32_t>(substitute(splitBits48To6(bits_input_part_expanded48), S_BOXES, 48), P_FF));
    }
    
public:
    uint8_t *performRound(uint8_t *bytes, uint8_t *key_round) override
    {
        uint32_t *bits_parts=splitBits64To32(joinBits<uint64_t>(bytes));
        uint32_t tmp=bits_parts[1];
        
        bits_parts[1]=functionF(bits_parts[1], joinBits<uint64_t>(key_round))^bits_parts[0];
        bits_parts[0]=tmp;
        
        return splitBitsTo8<uint64_t>(joinBitsParts32To64(bits_parts));
    }
};

class KeyExpansionRSA : public Interface_KeyExpansion
{
private:
    Interface_Primality<cpp_int> *primality_test;
    float probability_minimal;
    const size_t prime_numbers_length_bits;

public:
    KeyExpansionRSA(PrimalityTestingMode primality_testing_mode, float probability_minimal, const size_t prime_numbers_length_bits) : probability_minimal(probability_minimal), prime_numbers_length_bits(prime_numbers_length_bits)
    {
        switch(primality_testing_mode)
        {
            case PrimeTestingMode_Fermat:
                primality_test=new Primality_FermatTest<cpp_int>;
                break;
            case PrimeTestingMode_SolovayStrassen:
                primality_test=new Primality_SolovayStrassenTest<cpp_int>;
                break;
            case PrimeTestingMode_MillerRabin:
                primality_test=new Primality_MillerRabinTest<cpp_int>;
                break;
        }
    }
    ~KeyExpansionRSA() override
    {
        delete primality_test;
    }
    
    
    uint8_t **generateKeysRound(uint8_t *key) override
    {
        cpp_int prime_numbers[2];
        cpp_int n, euler_f, e, d;
        uint8_t **edn=new uint8_t*[3];
        
        for(uint8_t i=0; i<2; )
            if(primality_test->performTest((prime_numbers[i]=getNBitNumber<cpp_int>(prime_numbers_length_bits)), probability_minimal))
                i++;

        n=prime_numbers[0]*prime_numbers[1];
        euler_f=(prime_numbers[0]-1)*(prime_numbers[1]-1);
        
        do
        {
            e=rand()%(euler_f-2)+2;
        }
        while(gcd_utility<cpp_int>(e, euler_f)!=1);
        d=boost::integer::extended_euclidean<cpp_int>(e, euler_f).x;

        edn[0]=splitBitsTo8_cppInt(e);
        edn[1]=splitBitsTo8_cppInt(d);
        edn[2]=splitBitsTo8_cppInt(n);
        
        return edn;
    }
};

#endif