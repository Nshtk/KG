using System;
using System.Collections.ObjectModel;
using KP.Context;
using KP.Context.Interface;
using Soldatov.Wpf.MVVM.Core;

namespace KP.Models
{
    public class AlgorithmModel : ViewModelBase
    {
        private IAlgorithm _algorithm;
        private Utility.CipheringMode _cipheringMode;
        private byte[] _key;
        private byte[] _content_bytes;
        private ulong _init_vector;

        public ObservableCollection<IAlgorithm> Algorithms
        {
            get;
            set;
        }
        public IAlgorithm Algorithm
        {
            get { return _algorithm; }
            set { _algorithm = value; OnPropertyChanged("Algorithm"); }
        }
        public Utility.CipheringMode Ciphering
        {
            get {return _cipheringMode;}
            set {_cipheringMode=value; OnPropertyChanged("Ciphering");}
        }
        public byte[] Key
        {
            get {return _key;}
            set {_key=value; OnPropertyChanged("Key");}
        }

        public AlgorithmModel()
        {
            Ciphering=Utility.CipheringMode.CipheringMode_OFB;
            Algorithms = new ObservableCollection<IAlgorithm>
            {
                new Camellia(),
                new ElGamal()
            };
        }
        
        public void encrypt()
        {
            
        }
        public void decrypt()
        {
            
        }
    }
}