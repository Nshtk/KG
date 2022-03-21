#include <iostream>
#include "box.h"
#include "key.h"

void printNumbers(uint8_t *numbers)
{
    for(int i=0; i<10; i++)
    {
        cout<<bitset<8>(numbers[i])<<' ';
        cout<<+numbers[i]<<'\n';
    }
}

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
    AlgorithmDES des(new KeyExpansionFeistel);
    uint8_t message[7]={'q', 'w', 'e', 'r', 't', 'y'};
    uint8_t key[8] = {'D', 'E', 'S', 'k', 'e', 'y', '5', '6'};
    uint8_t *message_encrypted, *message_decrypted;

    des.encrypt(message, &message_encrypted, key);
    des.decrypt(message_encrypted, &message_decrypted, key);
    for(int i=0; i<7; i++)
        printf("%c ", message_decrypted[i]);

    return 0;
}
