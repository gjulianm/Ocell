using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Ocell.Library
{
    public class Autocompleter
    {
        private TextBox _textbox;
        private string _text;
        private bool _isAutocompleting = false;
        public IEnumerable<string> Strings { get; set; }
        public char Trigger { get; set; }
        public TextBox Textbox
        {
            get
            {
                return _textbox;
            }
            set
            {
                _textbox = value;
                _textbox.TextChanged += new TextChangedEventHandler(OnTextChanged);
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_textbox == null)
                return;

            if (_textbox.Text.Length > 0 && _textbox.Text.Last() == Trigger)
                _isAutocompleting = true;

            if (_isAutocompleting)
                UpdateAutocomplete();
        }

        private void UpdateAutocomplete()
        {
            if (_text == _textbox.Text)
                return;

            _text = _textbox.Text;

            if (string.IsNullOrWhiteSpace(_text))
                return;

            if (_text.IndexOf(Trigger) == -1)
            {
                _isAutocompleting = false;
                return;
            }

            if (_text.LastIndexOf(' ') > _text.LastIndexOf(Trigger))
            {
                _isAutocompleting = false;
                if (_textbox.SelectionStart < _text.Length)
                {
                    _text = _text.Remove(_textbox.SelectionStart);
                    _textbox.Text = _text;
                }
                return;
            }
            if (_textbox.SelectionStart < _text.Length)
            {
                _text = _text.Remove(_textbox.SelectionStart);
            }

            string Written = _text.Substring(_textbox.Text.LastIndexOf(Trigger) + 1);
            string first;

            if (Strings == null)
                first = "";
            else
            {
                first = Strings.FirstOrDefault(item => item != null && item.IndexOf(Written) == 0);
            }

            if (Written.Length >= first.Length)
            {
                _textbox.Text = _text;
                _textbox.SelectionStart = _text.Length;
                return;
            }

            int selectStart;
            selectStart = _text.Length - 1;
            first = first.Substring(Math.Min(Written.Length, first.Length - 1));
            _text = _text.Insert(_textbox.SelectionStart, first);
            _textbox.Text = _text;
            _textbox.SelectionStart = Math.Min(selectStart + 1, _text.Length - 1);
            _textbox.SelectionLength = 0;
        }
    }
}
