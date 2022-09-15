#ifndef KG_UTILITY_H
#define KG_UTILITY_H

#include <thread>
#include <boost/multiprecision/cpp_int.hpp>

using namespace boost::multiprecision;

enum CipheringMode
{
    CipheringMode_ECB,     // arguments: message size (bits), block size (bits)
    CipheringMode_CBC,
    CipheringMode_CFB,
    CipheringMode_OFB,
    CipheringMode_CTR,
    CipheringMode_RD,
    CipheringMode_RD_H
} Ciphering_Mode;

enum PrimalityTestingMode
{
    PrimeTestingMode_Fermat,
    PrimeTestingMode_SolovayStrassen,
    PrimeTestingMode_MillerRabin
} Primality_Testing_Mode;

const uint8_t S_BOXES[8][4][16]={
       {{14, 4,  13, 1,  2,  15, 11, 8,  3,  10, 6,  12, 5,  9,  0,  7},
       {0,  15, 7,  4,  14, 2,  13, 1,  10, 6, 12, 11, 9,  5,  3,  8},
       {4,  1,  14, 8,  13, 6,  2,  11, 15, 12, 9,  7,  3,  10, 5,  0},
       {15, 12, 8,  2,  4,  9,  1,  7,  5,  11, 3,  14, 10, 0, 6,  13}},
       
       {{15, 1,  8,  14, 6,  11, 3,  4,  9,  7,  2,  13, 12, 0,  5,  10},
       {3,  13, 4,  7,  15, 2,  8,  14, 12, 0, 1,  10, 6,  9,  11, 5},
       {0,  14, 7,  11, 10, 4,  13, 1,  5,  8,  12, 6,  9,  3,  2,  15},
       {13, 8,  10, 1,  3,  15, 4,  2,  11, 6,  7,  12, 0,  5, 14, 9}},
       
       {{10, 0,  9,  14, 6,  3,  15, 5,  1,  13, 12, 7,  11, 4,  2,  8},
       {13, 7,  0,  9,  3,  4,  6,  10, 2,  8, 5,  14, 12, 11, 15, 1},
       {13, 6,  4,  9,  8,  15, 3,  0,  11, 1,  2,  12, 5,  10, 14, 7},
       {1,  10, 13, 0,  6,  9,  8,  7,  4,  15, 14, 3,  11, 5, 2,  12}},
       
       {{7,  13, 14, 3,  0,  6,  9,  10, 1,  2,  8,  5,  11, 12, 4,  15},
       {13, 8,  11, 5,  6,  15, 0,  3,  4,  7, 2,  12, 1,  10, 14, 9},
       {10, 6,  9,  0,  12, 11, 7,  13, 15, 1,  3,  14, 5,  2,  8,  4},
       {3,  15, 0,  6,  10, 1,  13, 8,  9,  4,  5,  11, 12, 7, 2,  14}},
       
       {{2,  12, 4,  1,  7,  10, 11, 6,  8,  5,  3,  15, 13, 0,  14, 9},
       {14, 11, 2,  12, 4,  7,  13, 1,  5,  0, 15, 10, 3,  9,  8,  6},
       {4,  2,  1,  11, 10, 13, 7,  8,  15, 9,  12, 5,  6,  3,  0,  14},
       {11, 8,  12, 7,  1,  14, 2,  13, 6,  15, 0,  9,  10, 4, 5,  3}},
       
       {{12, 1,  10, 15, 9,  2,  6,  8,  0,  13, 3,  4,  14, 7,  5,  11},
       {10, 15, 4,  2,  7,  12, 9,  5,  6,  1, 13, 14, 0,  11, 3,  8},
       {9,  14, 15, 5,  2,  8,  12, 3,  7,  0,  4,  10, 1,  13, 11, 6},
       {4,  3,  2,  12, 9,  5,  15, 10, 11, 14, 1,  7,  6,  0, 8,  13}},
       
       {{4,  11, 2,  14, 15, 0,  8,  13, 3,  12, 9,  7,  5,  10, 6,  1},
       {13, 0,  11, 7,  4,  9,  1,  10, 14, 3, 5,  12, 2,  15, 8,  6},
       {1,  4,  11, 13, 12, 3,  7,  14, 10, 15, 6,  8,  0,  5,  9,  2},
       {6,  11, 13, 8,  1,  4,  10, 7,  9,  5,  0,  15, 14, 2, 3,  12}},
       
       {{13, 2,  8,  4,  6,  15, 11, 1,  10, 9,  3,  14, 5,  0,  12, 7},
       {1,  15, 13, 8,  10, 3,  7,  4,  12, 5, 6,  11, 0,  14, 9,  2},
       {7,  11, 4,  1,  9,  12, 14, 2,  0,  6,  10, 13, 15, 3,  5,  8},
       {2,  1,  14, 7,  4,  10, 8,  13, 15, 12, 9,  0,  3,  5, 6,  11}}
};

const uint8_t PI[64]={
        58, 50, 42, 34, 26, 18, 10, 2, 60, 52, 44, 36, 28, 20, 12, 4,
        62, 54, 46, 38, 30, 22, 14, 6, 64, 56, 48, 40, 32, 24, 16, 8,
        57, 49, 41, 33, 25, 17, 9, 1, 59, 51, 43, 35, 27, 19, 11, 3,
        61, 53, 45, 37, 29, 21, 13, 5, 63, 55, 47, 39, 31, 23, 15, 7
};

const uint8_t PF[64]={
        40, 8, 48, 16, 56, 24, 64, 32, 39, 7, 47, 15, 55, 23, 63, 31,
        38, 6, 46, 14, 54, 22, 62, 30, 37, 5, 45, 13, 53, 21, 61, 29,
        36, 4, 44, 12, 52, 20, 60, 28, 35, 3, 43, 11, 51, 19, 59, 27,
        34, 2, 42, 10, 50, 18, 58, 26, 33, 1, 41, 9, 49, 17, 57, 25
};

const uint8_t PE[48]={
        32, 1, 2, 3, 4, 5, 4, 5, 6, 7, 8, 9,
        8, 9, 10, 11, 12, 13, 12, 13, 14, 15, 16, 17,
        16, 17, 18, 19, 20, 21, 20, 21, 22, 23, 24, 25,
        24, 25, 26, 27, 28, 29, 28, 29, 30, 31, 32, 1
};

const uint8_t P_FF[32]={
        16, 7, 20, 21, 29, 12, 28, 17, 1, 15, 23, 26, 5, 18, 31, 10,
        2, 8, 24, 14, 32, 27, 3, 9, 19, 13, 30, 6, 22, 11, 4, 25
};

const uint8_t KP1[28]={
        57, 49, 41, 33, 25, 17, 9, 1, 58, 50, 42, 34, 26, 18,
        10, 2, 59, 51, 43, 35, 27, 19, 11, 3, 60, 52, 44, 36
};

const uint8_t KP2[28]={
        63, 55, 47, 39, 31, 23, 15, 7, 62, 54, 46, 38, 30, 22,
        14, 6, 61, 53, 45, 37, 29, 21, 13, 5, 28, 20, 12, 4
};

const uint8_t CP[48]={
        14, 17, 11, 24, 1, 5, 3, 28, 15, 6, 21, 10,
        23, 19, 12, 4, 26, 8, 16, 7, 27, 20, 13, 2,
        41, 52, 31, 37, 47, 55, 30, 40, 51, 45, 33, 48,
        44, 49, 39, 56, 34, 53, 46, 42, 50, 36, 29, 32
};

using namespace std;

template<typename T>
T joinBits(const uint8_t *bytes, size_t size=0)
{
    if(!size)
        size=sizeof(T);
    
    T bits=0;
    //bits=*(T *)bytes;
    for(int i=0; i<size; i++)
        bits=(bits<<8)|bytes[i];
    
    return bits;
}

/*template<typename T>
T joinBits(const uint8_t *bytes)
{
    T bits=0;
    //bits=*(T *)bytes;
    for(int i=0; i<sizeof(T); i++)
        bits = (bits << 8) | bytes[i];

    return bits;
}*/

uint64_t joinBitsParts32To64(const uint32_t *bits_parts)
{
    uint64_t bits;
    
    bits=(uint64_t) bits_parts[0];
    bits=(uint64_t) (bits<<32)|bits_parts[1];
    
    return bits;
}

uint64_t joinBitsParts28To56(const uint32_t *bits_parts)
{
    uint64_t bits;
    
    bits=bits_parts[0]>>4;
    bits=((bits<<32)|bits_parts[1])<<4;
    
    return bits;
}

uint32_t *splitBits64To32(uint64_t bits)
{
    uint32_t *bits_parts=new uint32_t[2]();
    
    bits_parts[0]=(uint32_t) (bits>>32);
    bits_parts[1]=(uint32_t) bits;
    
    return bits_parts;
}

uint8_t *splitBits48To6(uint64_t bits)
{
    uint8_t *bits_parts=new uint8_t[8];
    
    for(uint8_t i=0; i<8; i++)
        bits_parts[i]=(bits>>(58-(i*6)))<<2;
    
    return bits_parts;
}

template<typename T>
uint8_t *splitBitsTo8(T bits, size_t size=0)
{
    if(!size)
        size=sizeof(T);
    
    uint8_t *bits_parts=new uint8_t[size];
    //std::copy(static_cast<const char*>(static_cast<const void*>(&bits)), static_cast<const char*>(static_cast<const void*>(&bits)) + sizeof bits, bits_parts);
    for(size_t i=0; i<size; i++)
        bits_parts[i]=(uint8_t) ((bits>>(8*((size-1)-i)))&0xFF);
    
    return bits_parts;
}

size_t getLength_bytes(cpp_int value)
{
    if(value.is_zero())
        return 1;
    if(value.sign()<0)
        value=~value;
    
    size_t length=0;
    uint8_t lastByte;
    
    do
    {
        lastByte=value.convert_to<uint8_t>();
        value>>=8;
        length++;
    }
    while(!value.is_zero());
    if(lastByte>255)
        length++;
    
    return length;
}

cpp_int joinBits_cppInt(uint8_t *array)
{
    cpp_int bits=0;
    size_t i=0;
    
    for(uint8_t *bytes=array+1; i<array[0]; i++)
        bits=(bits<<8)|bytes[i];
    
    return bits;
}

uint8_t *splitBitsTo8_cppInt(cpp_int bits)
{
    size_t length_bytes=getLength_bytes(bits);
    uint8_t *array=new uint8_t[1+length_bytes];
    uint8_t *bytes=array+1;
    
    array[0]=length_bytes;
    
    //cout<<"length_bytes:"<<+array[0]<<endl;
    for(size_t i=0; i<length_bytes; i++)
    {
        bytes[i]=(uint8_t)((bits>>(8*((length_bytes-1)-i)))&0xFF);
        //cout<<"bytes:"<<+bytes[i]<<endl;
    }
    //cout<<"res:"<<+joinBits_cppInt(array)<<endl;
    //cout<<"res:"<<+joinBits<uint8_t>(bytes, length_bytes)<<endl;
    //cout<<"res2:"<<+joinBits<uint8_t>(splitBitsTo8(bits, getLength_bytes(bits)), length)<<endl;
    
    return array;
}

/*template<typename T>                  // old
uint8_t *splitBitsTo8(uint64_t bits)
{
    uint8_t size=sizeof(T);
    uint8_t *bits_parts=new uint8_t[size];
    //std::copy(static_cast<const char*>(static_cast<const void*>(&bits)), static_cast<const char*>(static_cast<const void*>(&bits)) + sizeof bits, bits_parts);
    for (uint8_t i=0; i<size; i++)
        bits_parts[i]=(bits >> (8*((size-1)-i))) & 0xFF;
    
    return bits_parts;
}*/

uint8_t *swapParts64(uint8_t *bytes)
{
    for(uint8_t i=0; i<4; i++)
        swap(bytes[i], bytes[4+i]);
    return bytes;
}

uint8_t *pad(const uint8_t *bytes, size_t bytes_length, size_t block_length)
{
    uint8_t *bytes_padded=new uint8_t[block_length];
    uint8_t value=block_length-bytes_length;
    
    for(uint8_t i=0; i<bytes_length; i++)
        bytes_padded[i]=bytes[i];
    for(; bytes_length<block_length; bytes_length++)
        bytes_padded[bytes_length]=value;
    
    return bytes_padded;
}

uint8_t *twoToOneDimArray(uint8_t **bytes, size_t bytes_length_blocks)
{
    uint8_t *bytes_joined=new uint8_t[bytes_length_blocks*8];
    
    for(unsigned i=0, k=0; i<bytes_length_blocks; i++)
        for(unsigned j=0; j<8; j++, k++)
            bytes_joined[k]=bytes[i][j];
    
    return bytes_joined;
}

/*uint8_t *twoToOneDimArray_rsa(uint8_t **bytes, size_t bytes_length_blocks)
{
    uint8_t *bytes_joined=new uint8_t[bytes_length_blocks];
    
    for(size_t i=0; i<bytes_length_blocks; i++)
        bytes_joined[i]=joinBits_cppInt(bytes[i]);
        
    return bytes_joined;
}*/

template<typename T>
T gcd_utility(T a, T b)
{
    return b ? gcd_utility<T>(b, a%b) : a;
}

template<typename T>
T gcd_extended_utility(T a, T b, T *x, T *y)
{
    if(a==0)
    {
        *x=0;
        *y=1;
        return b;
    }
    
    T x1, y1;
    T result=gcd_extended_utility<T>(b%a, a, &x1, &y1);
    
    
    *x=y1-(b/a)*x1;
    *y=x1;
    
    return result;
}

template<typename T>
T pow_big(T number, T power)
{
    T result=1;
    
    for( ; power>0; power/=2, number*=number)
        if(power%2==1)
            result*=number;
    
    return result;
}

template<typename T>
T pow_big_modulo(T number, T power, T modulo)
{
    T result=1;
    
    for(number=number%modulo; power>0; power/=2)            // TODO check if wrong and reverse какая цифра читается первой?
    {
        if(power%2==1)
            result=(result*number)%modulo;
        number=(number*number)%modulo;
    }
    
    return result;
}

template<typename T>
T getNBitNumber(size_t n)
{
    T t=1;
    n--;
    //T res=(t<<n)|(rand()%(t<<n));;
    //cout<<"res:"<<res<<"aaaa";
    return (t<<n)|(rand()%(t<<n));
}



/*
cpp_int joinBits_cppInt(uint8_t *array, size_t bits_length)
{
    size_t bytes_length=bits_length/8;
    if(bits_length%8)
        bytes_length++;
    size_t number_of_bytes_reserved=bytes_length/255;
    if(bytes_length%255)
        number_of_bytes_reserved++;
    
    cpp_int bits=0;
    size_t length_bytes=0;
    size_t i=0;
    //cout<<"arrayin:"<<+array[0]<<" "<<number_of_bytes_reserved<<endl;
    for( ; i<number_of_bytes_reserved; i++)
        length_bytes+=array[i];
    //cout<<"length_bytes:"<<length_bytes<<" "<<array[i]<<endl;
    i=0;
    for(uint8_t *bytes=array+number_of_bytes_reserved; i<length_bytes; i++)
        bits=(bits<<8)|bytes[i];
    
    return bits;
}

uint8_t *splitBitsTo8_cppInt(cpp_int bits, size_t bits_length)
{
    size_t bytes_length=bits_length/8;
    if(bits_length%8)
        bytes_length++;
    size_t number_of_bytes_reserved=bytes_length/255;
    size_t remainder=bytes_length%255;
    if(remainder)
        number_of_bytes_reserved++;

    size_t length_bytes=getLength_bytes(bits);
    uint8_t *array=new uint8_t[number_of_bytes_reserved+length_bytes];
    uint8_t *bytes=array+number_of_bytes_reserved;
    
    if(remainder)
    {
        for(size_t i=0; i<number_of_bytes_reserved-1; i++)
            array[i]=255;
        array[number_of_bytes_reserved-1]=remainder;
    }
    else
        for(size_t i=0; i<number_of_bytes_reserved; i++)
            array[i]=255;

    cout<<"array:"<<+array[0]<<endl;
    for(size_t i=0; i<length_bytes; i++)
    {
        bytes[i]=(uint8_t)((bits>>(8*((length_bytes-1)-i)))&0xFF);
        cout<<"bytes:"<<+bytes[i]<<endl;
    }
    cout<<"array:"<<+array[0]<<endl;
    cout<<"res:"<<+joinBits_cppInt(array, 4)<<endl;
    //cout<<"res:"<<+joinBits<uint8_t>(bytes, length_bytes)<<endl;
    //cout<<"res2:"<<+joinBits<uint8_t>(splitBitsTo8(bits, getLength_bytes(bits)), length)<<endl;
    
    return array;
}*/






/*void binEncode(cpp_int value, uint8_t *output, size_t length)
{
    if (value.is_zero())
        *output=0;
    else if(value.sign()>0)
    {
        while (length-->0)
        {
            *(output++)=value.convert_to<uint8_t>();
            value>>=8;
        }
    }
    else
    {
        value=~value;
        while(length-->0)
        {
            *(output++)=~value.convert_to<uint8_t>();
            value>>=8;
        }
    }
}

cpp_int binDecode(uint8_t* input, size_t length)
{
    cpp_int result( 0 );
    uint8_t a;
    int bits=-8;
    
    while(length>=1)
        result|=(cpp_int)*(input++)<<(bits+=8);
    a=*(input++);
    result|=(cpp_int)a<<(bits+=8);
    if(a>=0x80)
        result|=(cpp_int)-1<<(bits+8);
    
    return result;
}
*/


#endif