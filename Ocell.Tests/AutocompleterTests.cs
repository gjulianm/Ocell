using NUnit.Framework;
using Ocell.Library;
using System.Collections.Generic;

namespace Ocell.Tests
{
    [TestFixture]
    public class AutocompleterTests
    {
        public IEnumerable<TestCaseData> GetAutocompletingStateTestData
        {
            get
            {
                yield return new TestCaseData("", "", 0).Returns(false).SetName("GetAutocompletingState_EmptyText_IsFalse");

                yield return new TestCaseData("Lorem Ipsum Dolor Sit Amet", "Lorem Ipsum Dolor Sit Ame", -1).Returns(false)
                    .SetName("GetAutocompletingState_TextWithNoTrigger_IsFalse");

                yield return new TestCaseData("Lorem Ipsum @Dolor Sit Amet", "Lorem Ipsum @Dolor Sit Ame", -1).Returns(false)
                    .SetName("GetAutocompletingState_TextWithTriggerOnPreviousPos_IsFalse");

                yield return new TestCaseData("Lorem Ipsum Dolor Sit Ame@", "Lorem Ipsum Dolor Sit Ame", 3).Returns(false)
                    .SetName("GetAutocompletingState_TextWithTriggerOnLastPosSelStartNotLast_IsFalse");

                yield return new TestCaseData("Lorem Ipsum Dolor Sit Ame@ ", "Lorem Ipsum Dolor Sit Ame@", -1).Returns(false)
                    .SetName("GetAutocompletingState_TextWithTriggerBeforeSpace_IsFalse");

                yield return new TestCaseData("Lorem Ipsum Dolor Sit Ame@", "Lorem Ipsum Dolor Sit Ame", -1).Returns(true)
                    .SetName("GetAutocompletingState_TextWithTriggerLastPos_IsTrue");

                yield return new TestCaseData("Lorem @ Ipsum Dolor Sit Ame", "Lorem  Ipsum Dolor Sit Ame", 7).Returns(true)
                    .SetName("GetAutocompletingState_TextWithTriggerMiddleText_IsTrue");

                yield return new TestCaseData("Lorem @a Ipsum Dolor Sit Ame", "Lorem a Ipsum Dolor Sit Ame", 7).Returns(false)
                    .SetName("GetAutocompletingState_TextWithTriggerMiddleWord_IsFalse");

                yield return new TestCaseData("Lorem @a Ipsum Dolor Sit Ame", "Lorem @ Ipsum Dolor Sit Ame", 8).Returns(true)
                    .SetName("GetAutocompletingState_TextWithTriggerAndLettersAfter_IsTrue");
            }
        }

        [TestCaseSource("GetAutocompletingStateTestData")]
        public bool GetAutocompletingStateTest(string text, string previousText, int selectionStart)
        {
            var ac = new Autocompleter(null);
            ac.Trigger = '@';

            if (selectionStart == -1)
                selectionStart = text.Length;

            var actual = ac.GetAutocompletingState(text, previousText, selectionStart);

            return actual;
        }

        public IEnumerable<TestCaseData> GetTextWrittenByUserTestData
        {
            get
            {
                yield return new TestCaseData("Lorem @ipsu", "Lorem @ips", -1, 6).Returns("ipsu")
                    .SetName("GetTextWrittenByUser_AtEndOfString_ReturnsTextAfterTrigger");

                yield return new TestCaseData("Lorem @ipsu dolor", "Lorem @ips dolor", 11, 6).Returns("ipsu")
                    .SetName("GetTextWrittenByUser_TextInMiddleOfString_ReturnsUntilSpace");
            }
        }

        [TestCaseSource("GetTextWrittenByUserTestData")]
        public string GetTextWrittenByUserTest(string inputText, string previousText, int selectionStart, int triggerPosition)
        {
            var ac = new Autocompleter(null);
            ac.Trigger = '@';

            if (selectionStart == -1)
                selectionStart = inputText.Length;

            var actual = ac.GetTextWrittenByUser(inputText, previousText, selectionStart, triggerPosition);

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
    }
}
