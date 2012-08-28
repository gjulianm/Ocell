﻿using System;
using System.Collections.Generic;
using System.Linq;
using Ocell.Library;
using Ocell.Library.Twitter;

#if !METRO
using System.Windows.Controls;
#else
using Windows.UI.Xaml.Controls;
#endif

namespace Ocell.Library
{
    public class Autocompleter
    {
        private UsernameProvider _provider = new UsernameProvider();
        private TextBox _textbox;
        private string _text;
        private bool _isAutocompleting = false;
        private int _triggerPosition;
        public UserToken User { get { return _provider.User; } set { _provider.User = value; } }
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

            if (_textbox.Text.Length > 0 && _textbox.SelectionStart > 0 && _textbox.SelectionStart <= _textbox.Text.Length && (Trigger != '\0' && _textbox.Text[_textbox.SelectionStart - 1] == Trigger))
            {
                _isAutocompleting = true;
                _triggerPosition = _textbox.SelectionStart - 1;
            }

            if (_textbox.SelectionStart > 0 && 
                _textbox.SelectionStart < _textbox.Text.Length &&
                _textbox.Text[_textbox.SelectionStart - 1] == ' ' && _text != null && 
                _textbox.SelectionStart < _text.Length && _text[_textbox.SelectionStart] != '@' )
                _isAutocompleting = false;

            if (_isAutocompleting)
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

            RemovePreviousAutocompleted();

            if (ShouldStopAutocompleting())
            {
                UpdateTextbox();
                _isAutocompleting = false;
                return;
            }

            string written = GetTextWrittenByUser();
            string firstUser = GetFirstUserCoincidentWith(written);

            if (firstUser != null)
                AutocompleteText(firstUser.Substring(written.Length));

            UpdateTextbox();
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
#if !METRO
                item.IndexOf(chunk, StringComparison.InvariantCultureIgnoreCase) == 0);
#else
                item.IndexOf(chunk, StringComparison.OrdinalIgnoreCase) == 0);
#endif
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
    }
}