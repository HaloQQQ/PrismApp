using IceTea.Atom.BaseModels;
using IceTea.Atom.Extensions;
using Prism.Commands;
using System.Text;
using System.Windows.Input;

namespace MauiAppNet8.ViewModels
{
    internal class PhoneNumTranslateViewModel : BaseNotifyModel
    {
        public PhoneNumTranslateViewModel()
        {
            this.TranslateCommand = new DelegateCommand(() =>
            {
                var translatedNumber = this.ToNumber(this.Text);

                this.CallMessage = "Call " + (translatedNumber.IsNullOrBlank() ? string.Empty : (this.PhoneNum = translatedNumber));
            }, () => !this.Text.IsNullOrBlank())
                .ObservesProperty(() => this.Text);

            this.CallCommand = new DelegateCommand(() => { }, () => !this.PhoneNum.IsNullOrBlank())
                .ObservesProperty(() => this.PhoneNum);
        }

        private string ToNumber(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            raw = raw.ToUpperInvariant();

            var newNumber = new StringBuilder();
            foreach (var c in raw)
            {
                if (this.Contains(" -0123456789", c))
                    newNumber.Append(c);
                else
                {
                    var result = TranslateToNumber(c);
                    if (result != null)
                        newNumber.Append(result);
                    // Bad character?
                    else
                        return null;
                }
            }
            return newNumber.ToString();
        }

        private bool Contains(string keyString, char c)
        {
            return keyString.IndexOf(c) >= 0;
        }

        private readonly string[] digits = {
            "ABC", "DEF", "GHI", "JKL", "MNO", "PQRS", "TUV", "WXYZ"
        };

        private int? TranslateToNumber(char c)
        {
            for (int i = 0; i < digits.Length; i++)
            {
                if (digits[i].Contains(c))
                    return 2 + i;
            }
            return null;
        }

        #region Props
        private string _callMessage = "Call";

        public string CallMessage
        {
            get => this._callMessage;
            set => SetProperty<string>(ref _callMessage, value);
        }


        private string _phoneNum;

        public string PhoneNum
        {
            get => this._phoneNum;
            set => SetProperty<string>(ref _phoneNum, value);
        }

        private string _text = "1-555-NETMAUI";

        public string Text
        {
            get => this._text;
            set => SetProperty<string>(ref _text, value);
        }
        #endregion

        #region Commands
        public ICommand TranslateCommand { get; set; }
        public ICommand CallCommand { get; set; }
        #endregion
    }
}
