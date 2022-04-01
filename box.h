#ifndef KG_BOX_H
#define KG_BOX_H

#include "utility.h"

template<typename T>
uint8_t *permute(uint8_t *input_bytes, const uint8_t *p_box)
{
    uint64_t input_bits=joinBits<T>(input_bytes);
    uint64_t output_bits=0;
    uint8_t size_bits=sizeof(T)*8;
    
    for(uint8_t i=0 ; i<size_bits; i++)
        output_bits |= ((input_bits >> (size_bits - p_box[i])) & 0x01) << ((size_bits-1) - i);
    
    return splitBitsTo8<T>(output_bits);
}

uint8_t *substitute(const uint8_t *bytes, const uint8_t s_boxes[8][4][16], uint8_t k)
{
    k/=6;
    uint8_t first_last, middle;
    uint8_t *bytes_s = new uint8_t[4]();

    for(uint8_t i=0, j=0, w; j<k; i++, j+=2)
    {
        first_last=((bytes[j] >> 6) & 0x2) | ((bytes[j] >> 2) & 0x1);
        middle=(bytes[j] >> 3) & 0xF;
        bytes_s[i]=s_boxes[j][first_last][middle];
    
        first_last=((bytes[j+1] >> 6) & 0x2) | ((bytes[j+1] >> 2) & 0x1);
        middle=(bytes[j+1] >> 3) & 0xF;
        bytes_s[i]=(bytes_s[i]<<4) | s_boxes[j+1][first_last][middle];
        
        /*first_last=getBit(bytes[j], 0) << 1 | getBit(bytes[j], 5); middle = 0;
        for(w=1; w<5; w++)
            middle=(middle | getBit(bytes[j], w)) << 1;
        bytes_s[i]=s_boxes[j][first_last][middle];

        first_last=getBit(bytes[j+1], 0) << 1 | getBit(bytes[j+1], 5); middle = 0;
        for(w=1; w<5; w++)
            middle=(middle | getBit(bytes[j+1], w)) << 1;
        bytes_s[i]=(bytes_s[i]<<4) | s_boxes[j+1][first_last][middle];*/
    }
    /*cout<<'\n';
    for(int i=0; i<4; i++)
        printf("%d ", bytes_s[i]);
    cout<<'\n';*/
    return bytes_s;
    /*const unsigned length = k / 8;
    uint8_t *bytes_s = new uint8_t[length];
    uint8_t first_last;
    uint8_t middle;
    vector<bool> bits(k);

    for (int i = 0; i < length; i++)
        for (int j = 7; j > -1; j--)
            bits[i * 8 + j] = getBit(bytes[i], j);

    unsigned i_t, i_next = 0, i_mod, num = 0;
    uint8_t marker = 0, marker_t, tmp;

    for (int i = 0, num = 0; i < k; i += 6)
    {
        first_last=middle=0;
        i_next += 6;
        i_mod = i % 8;
        if (i_next < 8)
        {
            first_last = (first_last + getBit(bytes[num], i_mod)) << 1;
            int j = i_mod + 1;
            for (; j < i_mod + 5; j++)
                middle = (middle << 1) + getBit(bytes[num], j);
            first_last += getBit(bytes[num], j);
            bytes_s[num] += s_box[first_last][middle];
        }
        else
        {
            i_next = i_mod;
            first_last = (first_last + getBit(bytes[num], i)) << 1;
            int j = i_mod + 1, c = 0;
            for (; j % 8 != 0; j++, c++)
                middle = (middle << 1) + getBit(bytes[num], j);
            num += 1;
            for (j = 0; j < c; j++)
                middle = (middle << 1) + getBit(bytes[num], j);
            first_last += getBit(bytes[num], j);
            int w = 5;
            for (j = i_mod; j < 8; j++, w--)
                if (getBit(s_box[first_last][middle], w))
                    bytes_s[num] ^= 1U << j;
        }
    }
    return bytes_s;*/
}

#endif