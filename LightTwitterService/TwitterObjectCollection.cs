using System.Collections.Generic;
using System.Collections;

namespace Ocell.LightTwitterService
{
    public class TwitterObjectCollection : IEnumerable<TwitterObject>
    {
        public string Contents { get; protected set; }

        public TwitterObjectCollection(string contents)
        {
            Contents = contents;
        }

        public IEnumerator<TwitterObject> GetEnumerator()
        {
            return new TwitterObjectCollectionEnumerator(Contents);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsWellFormed()
        {
            int level = -1;

            for (int i = 0; i < Contents.Length; i++)
            {
                if (i == 0 || Contents[i - 1] != '\\')
                {
                    if (Contents[i] == '{')
                        level++;
                    else if (Contents[i] == '}')
                        level--;
                }
            }

            return level == -1;
        }
    }

    public class TwitterObjectCollectionEnumerator : IEnumerator<TwitterObject>
    {
        string _content;
        int _lastIndex;

        int _currentStart;
        int _currentEnd;

        public TwitterObjectCollectionEnumerator(string content)
        {
            _content = content;
            _lastIndex = -1;
            _currentStart = -1;
            _currentEnd = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public TwitterObject Current
        {
            get
            {
                return new TwitterObject(_content.Substring(_currentStart, _currentEnd - _currentStart));
            }
        }

        public bool MoveNext()
        {
            if (_lastIndex == -1)
                _lastIndex = 0;

            int start = _content.IndexOf('{', _lastIndex);
            int currentPosition = start;
            int level = 0;

            while ((currentPosition == start || level > 0) && currentPosition < _content.Length && currentPosition >= 0)
            {
                if (currentPosition == 0 || _content[currentPosition - 1] != '\\')
                {
                    if (_content[currentPosition] == '{')
                        level++;
                    else if (_content[currentPosition] == '}')
                        level--;
                }

                currentPosition++;
            }



            _currentStart = start;
            _currentEnd = currentPosition;
            _lastIndex = _currentEnd;

            if (_currentStart < _currentEnd && _currentStart >= 0 && _currentEnd < _content.Length)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Dispose()
        {
            _content = "";
        }

        public void Reset()
        {
            _lastIndex = 0;
        }
    }
}