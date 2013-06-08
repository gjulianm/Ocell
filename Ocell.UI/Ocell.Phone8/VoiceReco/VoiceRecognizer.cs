using Ocell.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Phone.Speech.Recognition;
using Windows.Phone.Speech.Synthesis;

namespace Ocell.VoiceReco
{
    public enum SendRepeatCancel { Send, Repeat, Cancel }

    public struct TextDictationResult
    {
        public string Text { get; set; }
        public bool UserCancelled { get; set; }
        public bool SuccesfulRecognition { get; set; }
    }

    public class VoiceRecognizer
    {
        public async Task<TextDictationResult> GetDictatedText()
        {
            var retval = new TextDictationResult();

            var recognizer = new SpeechRecognizerUI();

            var result = await recognizer.RecognizeWithUIAsync();

            if (result.ResultStatus == SpeechRecognitionUIStatus.Succeeded)
            {
                retval.SuccesfulRecognition = true;
                retval.Text = result.RecognitionResult.Text;

                var userConfirms = await AskSendRepeatCancelQuestion();

                if (userConfirms == SendRepeatCancel.Send)
                    retval.UserCancelled = false;
                else if (userConfirms == SendRepeatCancel.Repeat)
                    return await GetDictatedText();
                else if (userConfirms == SendRepeatCancel.Cancel)
                    retval.UserCancelled = true;
            }
            else
            {
                retval.SuccesfulRecognition = false;
                retval.UserCancelled = false;
                retval.Text = String.Empty;
            }

            return retval;
        }


        private async Task<SendRepeatCancel> AskSendRepeatCancelQuestion()
        {
            var recognizer = new SpeechRecognizerUI();
            string[] options = { Resources.Send, Resources.Repeat, Resources.Cancel };
            string exampleText =  String.Format("{0}, {1}, {2}", options[0], options[1], options[2]);
            recognizer.Recognizer.Grammars.AddGrammarFromList("SendRepeatCancel", options);
            recognizer.Settings.ExampleText = exampleText;


            SpeechSynthesizer synth = new SpeechSynthesizer();
            await synth.SpeakTextAsync(exampleText);

            var result = await recognizer.RecognizeWithUIAsync();

            if (result.ResultStatus == SpeechRecognitionUIStatus.Succeeded)
            {
                if (result.RecognitionResult.Text == Resources.Send)
                    return SendRepeatCancel.Send;
                else if (result.RecognitionResult.Text == Resources.Repeat)
                    return SendRepeatCancel.Repeat;
            }

            return SendRepeatCancel.Cancel; // In every other case.
        }
    }
}
