using AncoraMVVM.Base.IoC;
using NUnit.Framework;
using Ocell.Library;
using System.Collections.Generic;

namespace Ocell.Tests
{
    [TestFixture]
    public class AutocompleterTests
    {
        [TestFixtureSetUp]
        public static void Setup()
        {
            Dependency.Provider = new MockProvider();
        }

        [TestFixtureTearDown]
        public static void Teardown()
        {
            Dependency.Provider = null;
        }

        public IEnumerable<TestCaseData> GetAutocompletingStateTestData
        {
            get
            {
                yield return new TestCaseData("", 0).Returns(false).SetName("GetAutocompletingState_EmptyText_IsFalse");

                yield return new TestCaseData("Lorem Ipsum Dolor Sit Amet", -1).Returns(false)
                    .SetName("GetAutocompletingState_TextWithNoTrigger_IsFalse");

                yield return new TestCaseData("Lorem Ipsum @Dolor Sit Amet", -1).Returns(false)
                    .SetName("GetAutocompletingState_TextWithTriggerOnPreviousPos_IsFalse");

                yield return new TestCaseData("Lorem Ipsum Dolor Sit Ame@", 3).Returns(false)
                    .SetName("GetAutocompletingState_TextWithTriggerOnLastPosSelStartNotLast_IsFalse");

                yield return new TestCaseData("Lorem Ipsum Dolor Sit Ame@ ", -1).Returns(false)
                    .SetName("GetAutocompletingState_TextWithTriggerBeforeSpace_IsFalse");

                yield return new TestCaseData("Lorem Ipsum Dolor Sit Ame@", -1).Returns(false)
                    .SetName("GetAutocompletingState_TextWithTriggerLastPos_IsFalse");

                yield return new TestCaseData("Lorem @ Ipsum Dolor Sit Ame", 7).Returns(true)
                    .SetName("GetAutocompletingState_TextWithTriggerMiddleText_IsTrue");

                yield return new TestCaseData("Lorem a@a Ipsum Dolor Sit Ame", 7).Returns(false)
                    .SetName("GetAutocompletingState_TextWithTriggerMiddleWord_IsFalse");

                yield return new TestCaseData("Lorem @a Ipsum Dolor Sit Ame", 8).Returns(true)
                    .SetName("GetAutocompletingState_TextWithTriggerAndLettersAfter_IsTrue");
            }
        }

        [TestCaseSource("GetAutocompletingStateTestData")]
        public bool GetAutocompletingStateTest(string text, int selectionStart)
        {
            var ac = new Autocompleter(null);
            ac.Trigger = '@';

            if (selectionStart == -1)
                selectionStart = text.Length;

            var actual = ac.GetAutocompletingState(text, selectionStart);

            return actual;
        }

        public IEnumerable<TestCaseData> GetTextWrittenByUserTestData
        {
            get
            {
                yield return new TestCaseData("Lorem @ipsu", -1).Returns("ipsu")
                    .SetName("GetTextWrittenByUser_AtEndOfString_ReturnsTextAfterTrigger");
                yield return new TestCaseData("Lorem @ipsu dolor", 11).Returns("ipsu")
                    .SetName("GetTextWrittenByUser_TextInMiddleOfString_ReturnsUntilSpace");
                yield return new TestCaseData("Lorem @ipsum", 13).Returns("ipsum")
                    .SetName("GetTextWrittenByUser_CursorBeyondEndOfString_ReturnsCorrectString");
            }
        }

        [TestCaseSource("GetTextWrittenByUserTestData")]
        public string GetTextWrittenByUserTest(string inputText, int selectionStart)
        {
            var ac = new Autocompleter(null);
            ac.Trigger = '@';

            if (selectionStart == -1)
                selectionStart = inputText.Length;

            var actual = ac.GetTextWrittenByUser(inputText, selectionStart);

            return actual;
        }

        public IEnumerable<TestCaseData> InsertSuggestionInTextTestData
        {
            get
            {
                yield return new TestCaseData("Test @", 5, "user").Returns("Test @user")
                    .SetName("InsertSuggestionInText_AtEndOfString_TextAdded");
                yield return new TestCaseData("Test @u", 5, "user").Returns("Test @user")
                    .SetName("InsertSuggestionInText_AtEndOfStringWithTextAfterTrigger_TextAdded");
                yield return new TestCaseData("Test @ a suggestion", 5, "user").Returns("Test @user a suggestion")
                     .SetName("InsertSuggestionInText_MiddleOfString_TextAdded");
                yield return new TestCaseData("Test @u a suggestion", 5, "user").Returns("Test @user a suggestion")
                     .SetName("InsertSuggestionInText_MiddleOfStringWithTextAfterTrigger_TextAdded");
            }
        }

        [TestCaseSource("InsertSuggestionInTextTestData")]
        public string InsertSuggestionInTextTest(string text, int triggerPosition, string toInsert)
        {
            return new Autocompleter(null).InsertSuggestionInText(text, triggerPosition, toInsert);
        }

        public IEnumerable<TestCaseData> GetWordBeingWrittenTestData
        {
            get
            {
                yield return new TestCaseData("Word", 4).Returns("Word")
                    .SetName("GetWordBeingWritten_OneWordCursorAtEnd_ReturnsWord");
                yield return new TestCaseData("Word", 2).Returns("Word")
                    .SetName("GetWordBeingWritten_OneWordCursorAtMiddle_ReturnsWord");
                yield return new TestCaseData("A Word", 4).Returns("Word")
                    .SetName("GetWordBeingWritten_TwoWordsCursorAtEnd_ReturnsWord");
                yield return new TestCaseData("More long Word with things", 10).Returns("Word")
                    .SetName("GetWordBeingWritten_MultipleWordsCursorAtStart_ReturnsWord");
                yield return new TestCaseData("Word", 0).Returns("Word")
                    .SetName("GetWordBeingWritten_OneWordCursorAtStart_ReturnsWord");
                yield return new TestCaseData("Word two", 4).Returns("Word")
                    .SetName("GetWordBeingWritten_WordAfterCursorAtEnd_ReturnsWord");
                yield return new TestCaseData("Tst a two", 3).Returns("Tst")
                    .SetName("GetWordBeingWritten_CursorBeforeOneLetterWord_ReturnsPreviousWord");
                yield return new TestCaseData("Tst a two", 4).Returns("a")
                    .SetName("GetWordBeingWritten_CursorOnOneLetterWord_ReturnsLetter");
                yield return new TestCaseData("Tst a two", 5).Returns("a")
                    .SetName("GetWordBeingWritten_CursorAfterOneLetterWord_ReturnsLetter");

            }
        }

        [TestCaseSource("GetWordBeingWrittenTestData")]
        public string GetWordBeingWrittenTest(string text, int cursor)
        {
            return new Autocompleter(null).GetWordBeingWritten(text, cursor);
        }
    }
}
