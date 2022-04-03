#include <iostream>
#include <ctime>
#include "algorithm.h"

int main()
{
/*
    //=====================Pbox=======================
    unsigned int p_box = 241635;
    uint8_t b_numbers[10] = {50, 63, 0, 1, 5, 8, 13, 32, 29, 45};
    uint8_t *bp_numbers;

    printNumbers(b_numbers);

    cout<<"\nPermutation:\n";
    bp_numbers=permute(b_numbers, p_box);
    printNumbers(bp_numbers);

    //=====================Sbox=======================
    uint8_t *bs_numbers;


    cout<<"\nSubstitution:\n";
    bs_numbers=substitute(b_numbers, S_BOX, 8*NUM_COUNT);
    printNumbers(bs_numbers);
*/
    //=====================DES=======================
/*

    AlgorithmDES des(new KeyExpansionFeistel, new RoundCipheringFeistel);
    uint8_t message[8]={'q', 'w', 'e', 'r', 't', 'y', 'u', 'i'};
    uint8_t key[8]={'D', 'E', 'S', 'k', 'e', 'y', '5', '6'};
    uint8_t **keys_round=des.getKeysRound(key);
    uint8_t *message_encrypted, *message_decrypted;

    des.encrypt(message, &message_encrypted, keys_round);
    for(int i=0; i<8; i++)
        printf("%d ", message_encrypted[i]);
    cout<<'\n';
    des.decrypt(message_encrypted, &message_decrypted, keys_round);
    for(int i=0; i<8; i++)
        printf("%d ", message_decrypted[i]);
    
    for(uint8_t i=0; i<16; i++)
        delete keys_round[i];
    delete message_encrypted, delete message_decrypted; delete[] keys_round;
*/

    //=====================SymmetricAlgorithm=======================
    uint8_t key[8]={'D', 'E', 'S', 'k', 'e', 'y', '5', '6'};
    uint8_t **keys_round;
    size_t message_length=218, message_block_length=8, message_length_blocks;
    uint8_t message[]="Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin laoreet in lorem quis pretium. Aliquam maximus commodo augue, vitae pharetra risus. Ut imperdiet sapien id molestie pellentesque. Etiam vitae orci sapien.";
    uint8_t **message_encrypted, **message_decrypted;
    
    srand((unsigned long)time(nullptr));
    Ciphering_Mode=CipheringMode_RD_H;
    AlgorithmSymmetric algorithm_symmetric(new AlgorithmDES(new KeyExpansionFeistel, new RoundCipheringFeistel), Ciphering_Mode, key, /*54735745*/(unsigned long)rand(), {message_length, message_block_length});
    keys_round=algorithm_symmetric.getKeysRound(key);
    
    if(message_length%8==0)
        message_length_blocks=message_length/8;
    else
        message_length_blocks=message_length/8+1;
    message_encrypted=new uint8_t*[message_length_blocks]; message_decrypted=new uint8_t*[message_length_blocks];
    
    for(unsigned i=0; i<message_length; i++)
            printf("%c", message[i]);
    cout<<"\n\n";
    algorithm_symmetric.encrypt(message, message_encrypted, keys_round);
    for(unsigned i=0; i<message_length_blocks; i++)
        for(unsigned j=0; j<8; j++)
            printf("%c", message_encrypted[i][j]);
    cout<<"\n\n";
    algorithm_symmetric.decrypt(twoDimToOneDimArray(message_encrypted, message_length_blocks), message_decrypted, keys_round);
    for(unsigned i=0; i<message_length_blocks; i++)
        for(unsigned j=0; j<8; j++)
            printf("%c", message_decrypted[i][j]);
            

    return 0;
}
