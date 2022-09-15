#include <iostream>
#include <fstream>
#include <ctime>
#include "algorithm.h"

int main()
{
    ifstream fstream_input; ofstream fstream_output;
    string file_name;
    size_t message_length=218, message_block_length=8, message_length_blocks;
    //uint8_t message[message_length];
    uint8_t message[]="Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin laoreet in lorem quis pretium. Aliquam maximus commodo augue, vitae pharetra risus. Ut imperdiet sapien id molestie pellentesque. Etiam vitae orci sapien.";
    uint8_t **message_encrypted=nullptr, **message_decrypted=nullptr;
    //thread threads[1];
    
    //srand((unsigned long)time(nullptr));
    
    if(message_length%8==0)
        message_length_blocks=message_length/8;
    else
        message_length_blocks=message_length/8+1;
    
    //=====================Pbox=======================
/*
    unsigned int p_box = 241635;
    uint8_t b_numbers[10] = {50, 63, 0, 1, 5, 8, 13, 32, 29, 45};
    uint8_t *bp_numbers;

    cout<<"\nPermutation:\n";
    bp_numbers=permute(b_numbers, p_box);

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
/*
    uint8_t key[8]={'D', 'E', 'S', 'k', 'e', 'y', '5', '6'};
    uint8_t **keys_round;
    
    Ciphering_Mode=CipheringMode_CTR;
    AlgorithmSymmetric algorithm_symmetric(new AlgorithmDES(new KeyExpansionFeistel, new RoundCipheringFeistel), Ciphering_Mode, key, 54735745*//*(unsigned long)rand()*//*, {message_length, message_block_length});
    keys_round=algorithm_symmetric.getKeysRound(key);
    message_encrypted=new uint8_t*[message_length_blocks](); message_decrypted=new uint8_t*[message_length_blocks]();
    
    for(unsigned i=0; i<message_length; i++)
            printf("%c", message[i]);
    cout<<"\n\n";
    algorithm_symmetric.encrypt(message, message_encrypted, keys_round);
    *//*threads[0]=thread(&Interface_Algorithm::encrypt, algorithm_symmetric, message, ref(message_encrypted), keys_round);
    threads[0].join();*//*
    for(unsigned i=0; i<message_length_blocks; i++)
        for(unsigned j=0; j<8; j++)
            printf("%c", message_encrypted[i][j]);
    cout<<"\n\n";
    algorithm_symmetric.decrypt(twoToOneDimArray(message_encrypted, message_length_blocks), message_decrypted, keys_round);
    for(unsigned i=0; i<message_length_blocks; i++)
        for(unsigned j=0; j<8; j++)
            printf("%c", message_decrypted[i][j]);
*/
    //=====================RSA=======================
    AlgorithmRSA algorithm_rsa(PrimeTestingMode_MillerRabin, message_length, 0.999, 1024);
    uint8_t **keys_round=algorithm_rsa.getKeysRound(nullptr);
    
    message_encrypted=new uint8_t*[message_length](); message_decrypted=new uint8_t*[message_length]();
    
    for(unsigned i=0; i<message_length; i++)
        printf("%c", message[i]);
    cout<<"\n\n";
    
    algorithm_rsa.encrypt(message, message_encrypted, keys_round);
    for(unsigned i=0; i<message_length; i++)
        for(unsigned j=0; j<2; j++)
            printf("%c", message_encrypted[i][j]);
    cout<<"\n\n";
    
    algorithm_rsa.decrypt(twoToOneDimArray(message_encrypted, message_length), message_decrypted, keys_round);
    for(unsigned i=0; i<message_length; i++)
        for(unsigned j=0; j<1; j++)
            printf("%c", message_decrypted[i][j]);
    /*cin>>file_name;
    
    if(1)
    {
        fstream_input.open(file_name); fstream_output.open(file_name+".encrypted", ofstream::out | ofstream::trunc);
        if(!fstream_input.is_open() || !fstream_output.is_open())
            return 1;
        while(!fstream_input.eof())
        {
            for(size_t i=0; i<message_length; i++)
            {
                if(fstream_input.eof())
                    break;
                fstream_input>>message[i];
            }
            
            algorithm_rsa.encrypt(message, message_encrypted, keys_round);
            *//*for(unsigned i=0; i<message_length; i++)
                for(unsigned j=0; j<8; j++)
                    fstream_output<<message_encrypted[i][j];*//*
            uint8_t *buf=twoToOneDimArray(message_encrypted, message_length);
            for(size_t i=0; i<message_length; i++)
                fstream_output<<buf[i];
            delete[] buf;
        }
    }
    else
    {
        fstream_input.close(); fstream_output.close();
        fstream_input.open(file_name+".encrypted"); fstream_output.open(file_name+".decrypted", ofstream::out | ofstream::trunc);
        if(!fstream_input.is_open() || !fstream_output.is_open())
            return 1;
        while(!fstream_input.eof())
        {
            for(size_t i=0; i<message_length; i++)
                fstream_input>>message[i];
            algorithm_rsa.decrypt(twoToOneDimArray(message_encrypted, message_length), message_decrypted, keys_round);
            for(unsigned i=0; i<message_length; i++)
                for(unsigned j=0; j<1; j++)
                    cout<<message_decrypted[i][j];
            *//*uint8_t *buf=twoToOneDimArray(message_decrypted, message_length);
            for(size_t i=0; i<message_length; i++)
                fstream_output<<buf[i];
            delete[] buf;*//*
        }
    }
    fstream_input.close(); fstream_output.close();*/
    
    return 0;
}
