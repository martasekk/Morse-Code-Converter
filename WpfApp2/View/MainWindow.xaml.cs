using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Windows.Markup;

namespace MorseCodeConverter.MainWindow
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            _synth.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
            _synth.Rate = 1;     
            _synth.Volume = 100;
        }

        private static readonly string[] morseCodeTransferTable = 
        {
            ".-",    // A
            "-...",  // B
            "-.-.",  // C
            "-..",   // D
            ".",     // E
            "..-.",  // F
            "--.",   // G
            "....",  // H
            "..",    // I
            ".---",  // J
            "-.-",   // K
            ".-..",  // L
            "--",    // M
            "-.",    // N
            "---",   // O
            ".--.",  // P
            "--.-",  // Q
            ".-.",   // R
            "...",   // S
            "-",     // T
            "..-",   // U
            "...-",  // V
            ".--",   // W
            "-..-",  // X
            "-.--",  // Y
            "--..",  // Z
            "-----", // 0
            ".----", // 1
            "..---", // 2
            "...--", // 3
            "....-", // 4
            ".....", // 5
            "-....", // 6
            "--...", // 7
            "---..", // 8
            "----."  // 9
        };

        bool textToMorse = true;

        private readonly SpeechSynthesizer _synth = new SpeechSynthesizer();

        static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString();
        }

        private void TextToMorseButton_Click(object sender, RoutedEventArgs e)
        {
            textToMorse = !textToMorse;
            var button = sender as Button;
            if (button != null)
            {
                if (textToMorse == true)
                    button.Content = "Text";
                else
                    button.Content = "Morse";
            }

            var temp = InputArea.Text;
            InputArea.Text = OutputArea.Text;
            OutputArea.Text = temp;
        }



        private void ClearButton_Click(object sender, RoutedEventArgs e) => InputArea.Clear();

        private CancellationTokenSource _cts;
        private void TTSButton_Click(object sender, RoutedEventArgs e)
        {

            string text = OutputArea.Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                _synth.SpeakAsyncCancelAll();
                if (textToMorse == false)
                {
                    _synth.SpeakAsync(text);
                }
                else
                {
                    _cts?.Cancel();
                    _cts = new CancellationTokenSource();
                    var token = _cts.Token;
                    try
                    {
                        _ = PlayMorseAsync(text, token);
                    }
                    catch (OperationCanceledException)
                    {
                       
                    }
                }
            }
        }
        private async Task PlayMorseAsync(string morse, CancellationToken token)
        {

            token.ThrowIfCancellationRequested();
            int beepTime = 100;

            var tasks = morse.Select(async c =>
            {
                switch (c)
                {
                    case '.':
                        await Task.Run(() => Console.Beep(900, beepTime), token);
                        break;
                    case '-':
                        await Task.Run(() => Console.Beep(900, beepTime * 3), token);
                        break;
                    case ' ':
                        await Task.Delay(beepTime * 2, token);
                        break;
                }
                await Task.Delay(beepTime / 2, token);
            });

            foreach (var t in tasks)
                await t;
        }



        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox == null)
                return;

            string newText = ReadInput(textBox);
            string outText;

            if (textToMorse == false)
            {
                outText = MorseToText(newText);
            }
            else
            {
                outText = TextToMorse(newText);
            }

            WriteOutput(outText);
        }

        private string TextToMorse(string input)
        {
            input = RemoveDiacritics(input.ToUpper());
            StringBuilder output = new StringBuilder();

            foreach (char c in input)
            {
                if (c >= 'A' && c <= 'Z')
                {
                    output.Append(morseCodeTransferTable[c - 'A'] + " ");
                }
                else if (c >= '0' && c <= '9')
                {
                    output.Append(morseCodeTransferTable[c - '0' + 26] + " ");
                }
                else if (c == ' ')
                {
                    output.Append("/ ");
                }
            }
            return output.ToString().Trim();
        }

        private string MorseToText(string input)
        {
            StringBuilder output = new StringBuilder();
            string[] words = input.Split(new string[] { " / " }, StringSplitOptions.None);

            foreach (string word in words)
            {
                string[] letters = word.Split(' ');
                foreach (string letter in letters)
                {
                    int index = Array.IndexOf(morseCodeTransferTable, letter);
                    if (index != -1)
                    {
                        if (index < 26)
                        {
                            output.Append((char)('A' + index));
                        }
                        else
                        {
                            output.Append((char)('0' + (index - 26)));
                        }
                    }
                }
                output.Append(' ');
            }
            return output.ToString().Trim();
        }

        private string ReadInput(TextBox textBox) => textBox.Text;

        private void WriteOutput(string output) => OutputArea.Text = output;

        // Přidejte tuto metodu, pokud není generována automaticky:
        private void InitializeComponent()
        {
            // Tato metoda je obvykle generována automaticky při kompilaci XAML.
            // Pokud není, můžete ji vygenerovat ručně nebo zkontrolovat, zda je soubor View.xaml správně propojen.
            Uri resourceLocater = new Uri("/WpfApp2;component/View.xaml", UriKind.Relative);
            Application.LoadComponent(this, resourceLocater);
        }
    }
}