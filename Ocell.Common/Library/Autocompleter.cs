using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Ocell.Library;
using Ocell.Library.Twitter;
using DanielVaughan.ComponentModel;
using System.Windows;
using DanielVaughan;

namespace Ocell.Library
{
    public class Autocompleter : ObservableObject
    {
        private UsernameProvider _provider = new UsernameProvider();
        private TextBox _textbox;
        private string _text;
        
        bool isAutocompleting;
        public bool IsAutocompleting
        {
            get { return isAutocompleting; }
            set { Assign("IsAutocompleting", ref isAutocompleting, value); }
        }
        private int _triggerPosition;
        public UserToken User { get { return _provider.User; } set { _provider.User = value; } }
        public char Trigger { get; set; }
        public SafeObservable<string> Suggestions { get; protected set; }
        string written;
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

        public Autocompleter()
        {
            Suggestions = new SafeObservable<string>();
            IsAutocompleting = false;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_textbox == null)
                return;

            if (_textbox.Text.Length > 0 && _textbox.SelectionStart > 0 && _textbox.SelectionStart <= _textbox.Text.Length && (Trigger != '\0' && _textbox.Text[_textbox.SelectionStart - 1] == Trigger))
            {
                IsAutocompleting = true;
                _triggerPosition = _textbox.SelectionStart - 1;
            }

            if (_textbox.SelectionStart > 0 && 
                _textbox.SelectionStart < _textbox.Text.Length &&
                _textbox.Text[_textbox.SelectionStart - 1] == ' ' && _text != null && 
                _textbox.SelectionStart < _text.Length && _text[_textbox.SelectionStart] != '@' )
                IsAutocompleting = false;

            if (IsAutocompleting)
                UpdateAutocomplete();
        }

        private bool ShouldStopAutocompleting()
        {
            if (string.IsNullOrWhiteSpace(_text))
                return true;

            if (!_text.Contains(Trigger) && Trigger != '\0')
                return true;

            if (_textbox.SelectionStart <= _text.Length && _textbox.SelectionStart > 0 && _text[_textbox.SelectionStart - 1] == ' ')
                return true;

            return false;
        }

        private void UpdateAutocomplete()
        {
            // There's an strange reason which causes TextChanged to fire indefinitely, although the text has not changed really.
            // To avoid this, if the text we stored is the same, just return.
            if (_text == _textbox.Text)
                return;

            _text = _textbox.Text;

            if (ShouldStopAutocompleting())
            {
                IsAutocompleting = false;
                return;
            }

            written = GetTextWrittenByUser();

            Suggestions.Clear();

            foreach (var user in _provider.Usernames
                .Where(x => x.IndexOf(written, StringComparison.InvariantCultureIgnoreCase) != -1)
                .Take(20)
                .OrderBy(x => x))
            {
                Suggestions.Add(user);
            }
        }

        private void RemovePreviousAutocompleted()
        {
            if (string.IsNullOrWhiteSpace(_text))
                return;

            int firstSpaceAfterSelStart = _text.Substring(_textbox.SelectionStart).IndexOf(' ');
            if (firstSpaceAfterSelStart == -1 && _textbox.SelectionStart < _text.Length)
                _text = _text.Remove(_textbox.SelectionStart);
            else if (firstSpaceAfterSelStart != -1 && firstSpaceAfterSelStart + _textbox.SelectionStart < _text.Length &&
                firstSpaceAfterSelStart != 1)
                _text = _text.Remove(_textbox.SelectionStart, firstSpaceAfterSelStart);
        }

        private string GetTextWrittenByUser()
        {
            if (_textbox.SelectionStart < _text.Length)
                return _text.Substring(_triggerPosition + 1, _textbox.SelectionStart - _triggerPosition -1);
            else if (_triggerPosition + 1 < _text.Length)
                return _text.Substring(_triggerPosition + 1);
            else
                return "";
        }

        private string GetFirstUserCoincidentWith(string chunk)
        {
            return _provider.Usernames.FirstOrDefault(item => 
                item.IndexOf(chunk, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        private void AutocompleteText(string text)
        {
            int insertPosition;
            if (_textbox.SelectionStart > _text.Length)
                insertPosition = _text.Length;
            else
                insertPosition = _textbox.SelectionStart;

            _text = _text.Insert(insertPosition, text);
        }

        private void UpdateTextbox()
        {
            int oldSelStart = _textbox.SelectionStart;
            _textbox.Text = _text;
            _textbox.SelectionStart = oldSelStart;
        }

        public void UserChoseElement(string name)
        {
            IsAutocompleting = false;
            written = "";

            // Remove the user text written until now.
            var nextSpace = _text.IndexOf(' ', _triggerPosition);


            var newText = _text.Substring(0, _triggerPosition + 1) + name;
            
            if(nextSpace != -1)
                newText += _text.Substring(nextSpace);

            _text = newText;

            Deployment.Current.Dispatcher.InvokeIfRequired(() => _textbox.Text = _text);
        }
    }
}
