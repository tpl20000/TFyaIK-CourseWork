using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Windows.Forms.VisualStyles;
using static Problemmogram.Problemmogram;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.LinkLabel;

namespace Problemmogram
{
    public partial class Problemmogram : Form
    {
        private string file = "";

        List<string> changes = new List<string>();

        StreamWriter stream = new StreamWriter("Results.txt");

        int index = 0;
        bool do_change = false;
        bool just_save = false;
        bool new_file = false;
        bool logicTrigger = false;
        bool spaceTrigger = false;
        bool blocker = false;

        public Problemmogram()
        {
            InitializeComponent();

            stream.AutoFlush = true;
            button4.Enabled = false;
            button3.Enabled = false;

            changes.Add(inputTextBox.Text);
        }

        private void newFile(object sender, EventArgs e)
        {
            if (!just_save || !new_file)
            {
                DialogResult result = MessageBox.Show("Сохранить?", "Сохранение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Cancel)
                    return;

                if (result == DialogResult.Yes)
                {
                    saveFile(sender, e);
                }
            }

            new_file = true;
            do_change = true;

            inputTextBox.Text = "";
            file = "";
            this.Text = "Новый файл";
        }

        private void openFile(object sender, EventArgs e)
        {
            if (inputTextBox.Text != "")
            {
                if (!just_save || !new_file)
                {
                    DialogResult result = MessageBox.Show("Сохранить?", "Сохранение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (result == DialogResult.Cancel)
                        return;

                    if (result == DialogResult.Yes)
                    {
                        saveFile(sender, e);
                    }
                }
            }

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                new_file = true;
                do_change = true;

                file = openFileDialog1.FileName;
                using (StreamReader reader = new StreamReader(openFileDialog1.FileName))
                    inputTextBox.Text = reader.ReadToEnd();

                this.Text = file;
            }
        }

        private void saveFile(object sender, EventArgs e)
        {
            if (file == "")
                saveFileAs(sender, e);
            else
            {
                File.WriteAllText(file, inputTextBox.Text);

                changes.RemoveRange(0, index);
                index = 0;

                just_save = true;
                button3.Enabled = false;

                if (index == changes.Count - 1)
                    button4.Enabled = false;
                else
                    button4.Enabled = true;
            }
        }

        private void saveFileAs(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
            if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
                return;

            changes.RemoveRange(0, index);
            index = 0;

            just_save = true;
            button3.Enabled = false;

            if (index == changes.Count - 1)
                button4.Enabled = false;
            else
                button4.Enabled = true;

            string filename = saveFileDialog.FileName;
            file = filename;
            this.Text = file;
            File.WriteAllText(filename, inputTextBox.Text);
        }

        private void exit(object sender, EventArgs e)
        {
            this.Close();
        }

        private void help(object sender, EventArgs e)
        {
            Help.ShowHelp(this, "Help.chm");
        }

        private void cut(object sender, EventArgs e)
        {
            inputTextBox.Cut();
        }

        private void copy(object sender, EventArgs e)
        {
            inputTextBox.Copy();
        }

        private void paste(object sender, EventArgs e)
        {
            inputTextBox.Paste();
        }

        private void undo(object sender, EventArgs e)
        {
            if (index != -1 && index != 0)
            {
                index--;

                inputTextBox.Text = changes[index];
            }
        }

        private void redo(object sender, EventArgs e)
        {
            if (index != -1 && index != changes.Count - 1)
            {
                index++;

                inputTextBox.Text = changes[index];
            }
        }

        private void inputTextChanged(object sender, EventArgs e)
        {
            if (((TextBox)sender).Modified || do_change)
            {
                if (index != changes.Count - 1)
                    changes.RemoveRange(index + 1, changes.Count - 1 - index);

                if (new_file)
                {
                    changes.Clear();
                    index = -1;
                }

                do_change = false;
                new_file = false;
                just_save = false;

                changes.Add(inputTextBox.Text);
                index++;
            }

            if (index == changes.Count - 1)
                button4.Enabled = false;
            else
                button4.Enabled = true;

            if (index == 0)
                button3.Enabled = false;
            else
                button3.Enabled = true;
        }

        private void eraseSelected(object sender, EventArgs e)
        {
            do_change = true;

            int a = inputTextBox.SelectionLength;
            inputTextBox.Text = inputTextBox.Text.Remove(inputTextBox.SelectionStart, a);
        }

        private void selectEverything(object sender, EventArgs e)
        {
            inputTextBox.SelectAll();
        }

        private void formClosing(object sender, FormClosingEventArgs e)
        {
            if (inputTextBox.Text != "")
            {
                if (!just_save || !new_file)
                {
                    DialogResult result = MessageBox.Show("Сохранить?", "Сохранение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        if (file != "")
                            File.WriteAllText(file, inputTextBox.Text);
                        else
                            saveFileAs(sender, e);
                    }
                }
            }
        }

        private void eraseEverything(object sender, EventArgs e)
        {
            inputTextBox.SelectAll();

            eraseSelected(sender, e);
        }

        private void start(object sender, EventArgs e)
        {
            outputTextBox.Clear();


            string result = ParserStart(inputTextBox.Text);

            if (result != null)
                outputTextBox.Text = result;

        }

        private void changeFontSize(object sender, EventArgs e)
        {
            inputTextBox.Font = new Font("Segoe UI", fontSizeSlider.Value * 3 + 1);
            outputTextBox.Font = new Font("Segoe UI", fontSizeSlider.Value * 3 + 1);
            label2.Text = (fontSizeSlider.Value * 3 + 1).ToString();
        }

        private void resetFontSizeButton_Click(object sender, EventArgs e)
        {
            inputTextBox.Font = new Font("Segoe UI", 16);
            outputTextBox.Font = new Font("Segoe UI", 16);
            label2.Text = 16.ToString();
            fontSizeSlider.Value = 5;
        }

        public enum token
        {
            do_,
            end_do,
            id,
            num,
            eq,
            com,
            error,
            idError,
            numError,
            comError,
            eqError,
            doError,
            spaceError,
            noEndDoError,
            afterEndDoTextError,
            end
        }

        public class token_inf
        {
            public token_inf(token s, int p, int l, string _tokenText)
            {
                state = s;
                position = p;
                line = l;
                tokenText = _tokenText;
                size = 1;
            }

            public token state { get; }
            public int position { get; }
            public int line { get; }
            public int size { get; set; }
            public string tokenText { get; set; }

            public void size_change()
            {
                size++;
            }
        }

        List<token_inf> scanner_tokens = new List<token_inf>();

        public class error_info
        {
            public error_info(int p, int l, int s)
            {
                position = p;
                line = l;
                size = s;
            }

            public int position { get; }
            public int line { get; }
            public int size { get; }
        }

        private void HandleToken(List<token_inf> tokenInfoList, token tk, int i, int line, string txt)
        {
            if (tk == token.do_) tokenInfoList.Add(new token_inf(token.do_, i, line, "do"));
            else if (tk == token.doError && !checkLastToken(token.doError)) { tokenInfoList.Add(new token_inf(token.doError, i, line, txt)); }
            else if (tokenInfoList.Count > 0)
            if (tokenInfoList[tokenInfoList.Count - 1].state != tk)
                tokenInfoList.Add(new token_inf(tk, i, line, txt));
            else tokenInfoList[tokenInfoList.Count - 1].tokenText += txt;
            return;
        }

        private bool checkLastToken(token tk)
        {

            if (scanner_tokens.Count > 1)
                if (scanner_tokens[scanner_tokens.Count - 1].state == tk) return true;
                else return false;
            else return false;

        }

        public string ParserStart(string input)
        {
            if (input.Length == 0)
                return null;

            List<string> text_lines = input.Split('\n').ToList();

            //get rid of multiple spaces
            for(int k = 0; k < text_lines.Count; k++) text_lines[k] = Regex.Replace(text_lines[k], @"\s+", " ");

            string s = "";

            int state = 0;

            scanner_tokens.Clear();

            for (int i = 0; i < text_lines.Count; i++)
            {

                if (text_lines[i].Length < 1) continue;

                for (int j = 0; j < text_lines[i].Length; j++)
                {
                    Scanner(text_lines[i], i, ref j, ref state);
                }

            }

            //if no end_do token is found - add noEndDoError token
            if (scanner_tokens.Count == 0) s += "Циклы не обнаружены";
            else if (scanner_tokens[scanner_tokens.Count - 1].state != token.end_do) scanner_tokens.Add(new token_inf(token.noEndDoError, 0, text_lines.Count, ""));

            //foreach (token_inf k in scanner_tokens)
            //{
            //    s += $"{k.state} ({k.tokenText}); ";
            //}

            s += '\n';

            //tokens_index = 0;
            //tokensToErrorHandles();

            //foreach (recursive_state k in recursive_states)
            //{
            //    s += k + "; ";
            //}

            s += handleErrors(text_lines);

            return s;
        }

        public void Scanner(string input, int line, ref int i, ref int state)
        {

            if(i > input.Length) return;
            //if(i < input.Length) if ((input[i] == ' ' || input[i] == '\t') && state != 1 && state != 5) return;

            //MessageBox.Show($"Scanning: {input[i]}");

            switch (state)
            {
                case 0: //d
                    if (input[i] == ' ') return;
                    if (input.Length > 0) if (input[i] == 'd') state = 1;
                        else i = input.Length - 1;
                    return;

                case 1: //o
                    if (input[i] != 'o')
                    {
                        //scanner_tokens.Add(new token_inf(token.doError, i, line, input[i].ToString()));
                        HandleToken(scanner_tokens, token.doError, i, line, input[i].ToString());
                        i--;
                        state = 2;
                    }
                    else if (i == input.Length - 1) // end of line error
                    {
                        HandleToken(scanner_tokens, token.doError, i, line, input[i].ToString());
                        HandleToken(scanner_tokens, token.idError, i, line, input[i].ToString());
                        HandleToken(scanner_tokens, token.eqError, i, line, input[i].ToString());
                        HandleToken(scanner_tokens, token.numError, i, line, input[i].ToString());
                        HandleToken(scanner_tokens, token.comError, i, line, input[i].ToString());
                        HandleToken(scanner_tokens, token.numError, i, line, input[i].ToString());
                    }
                    else
                    {
                        HandleToken(scanner_tokens, token.do_, i, line, "do");
                        state = 2;
                    }
                    return;

                case 2: //space

                    if (i < input.Length - 1) if (input[i] != ' ')
                    {
                        HandleToken(scanner_tokens, token.spaceError, i, line, input[i].ToString());
                        state = 3;
                    }
                    else if (i == input.Length - 1) // end of line error
                    {
                        HandleToken(scanner_tokens, token.idError, i, line, "");
                        HandleToken(scanner_tokens, token.eqError, i, line, "");
                        HandleToken(scanner_tokens, token.numError, i, line, "");
                        HandleToken(scanner_tokens, token.comError, i, line, "");
                        HandleToken(scanner_tokens, token.numError, i, line, "");
                        state = 9;
                    }
                    else
                    {
                        state = 3;
                    }
                    return;

                case 3: //id

                    if (i < input.Length - 1) if (Char.IsDigit(input[i]) && !logicTrigger) 
                        { 
                            HandleToken(scanner_tokens, token.idError, i, line, input[i].ToString());
                            blocker = true;
                        }
                    else if (Char.IsLetter(input[i]) && !logicTrigger && !blocker)
                        {
                            logicTrigger = true;
                            HandleToken(scanner_tokens, token.id, i, line, input[i].ToString());
                        }
                    else if (input[i] == ' ' || input[i] == ',' || i == input.Length - 1)
                    {
                        if (checkLastToken(token.id) || checkLastToken(token.idError)) { logicTrigger = false; blocker = false; state = 4; }
                        else
                        {
                            HandleToken(scanner_tokens, token.idError, i, line, "");
                            HandleToken(scanner_tokens, token.eqError, i, line, "");
                            HandleToken(scanner_tokens, token.numError, i, line, "");
                            HandleToken(scanner_tokens, token.comError, i, line, "");
                            HandleToken(scanner_tokens, token.numError, i, line, "");
                            blocker = false; 
                            state = 9;
                        }
                    }
                    else if (i == input.Length - 1) // end of line error
                    {
                        HandleToken(scanner_tokens, token.idError, i, line, "");
                        HandleToken(scanner_tokens, token.eqError, i, line, "");
                        HandleToken(scanner_tokens, token.numError, i, line, "");
                        HandleToken(scanner_tokens, token.comError, i, line, "");
                        HandleToken(scanner_tokens, token.numError, i, line, "");
                        blocker = false;
                        state = 9;
                    }
                    
                    else if (input[i] == '=') { logicTrigger = false; blocker = false; state = 4; i--; }
                    else if (logicTrigger && !blocker)
                        {
                            HandleToken(scanner_tokens, token.id, i, line, input[i].ToString());
                        }
                    else if (blocker)
                        {
                            HandleToken(scanner_tokens, token.idError, i, line, input[i].ToString());
                        }
                    return;

                case 4: //equals

                    if (input[i] == ' ') return;
                    if (input[i] == '=') { HandleToken(scanner_tokens, token.eq, i, line, "="); state = 5; }
                    else { HandleToken(scanner_tokens, token.eqError, i, line, ""); i--; state = 5; }
                    return;

                case 5: //num 1

                    if (input[i] == ' ' && !spaceTrigger) return;
                    if (Char.IsDigit(input[i]) && !logicTrigger) { spaceTrigger = true; HandleToken(scanner_tokens, token.num, i, line, input[i].ToString()); }
                    else if (Char.IsLetter(input[i])) if (checkLastToken(token.num))
                        {
                            logicTrigger = true;
                            spaceTrigger = true;
                            HandleToken(scanner_tokens, token.numError,
                        scanner_tokens[scanner_tokens.Count - 1].position, scanner_tokens[scanner_tokens.Count - 1].line,
                        scanner_tokens[scanner_tokens.Count - 1].tokenText + input[i]);
                            scanner_tokens.RemoveAt(scanner_tokens.Count - 2);
                        }
                        else { logicTrigger = true; spaceTrigger = true; HandleToken(scanner_tokens, token.numError, i, line, input[i].ToString()); }
                    else if (input[i] == ',' || input[i] == ' ')
                    { state = 6; i--; logicTrigger = false; spaceTrigger = false; }
                    else
                    {
                        HandleToken(scanner_tokens, token.numError, i, line, input[i].ToString());
                    }

                    return;

                case 6: //comma

                    if (input[i] == ' ') return;
                    if (input[i] == ',') { HandleToken(scanner_tokens, token.com, i, line, input[i].ToString()); state = 7; }
                    else
                    {

                        if (i < 1) return;
                        HandleToken(scanner_tokens, token.comError, i - 1, line, input[i - 1].ToString());
                        i--;
                        state = 7;

                    }

                    return;

                case 7: //num 2

                    if (input[i] == ' ') if (!checkLastToken(token.num) && !logicTrigger) return; else { state = 8; logicTrigger = false; return; }
                    if (Char.IsDigit(input[i]) && !logicTrigger) { HandleToken(scanner_tokens, token.num, i, line, input[i].ToString()); }
                    else if (Char.IsLetter(input[i])) if (checkLastToken(token.num))
                        {
                            logicTrigger = true;
                            HandleToken(scanner_tokens, token.numError,
                        scanner_tokens[scanner_tokens.Count - 1].position, scanner_tokens[scanner_tokens.Count - 1].line,
                        scanner_tokens[scanner_tokens.Count - 1].tokenText + input[i]);
                            scanner_tokens.RemoveAt(scanner_tokens.Count - 2);
                        }
                        else { logicTrigger = true; HandleToken(scanner_tokens, token.numError, i, line, input[i].ToString()); }
                    else if (i == input.Length - 1)
                    {
                        state = 8;
                    }
                    else
                    {
                        HandleToken(scanner_tokens, token.numError, i, line, input[i].ToString());
                    }

                    return;

                case 8:
                    if (input.Length >= 6)
                    {
                        if (input[i] == 'e' && input[i + 1] == 'n' && input[i + 2] == 'd'
                            && input[i + 3] == ' ' && input[i + 4] == 'd' && input[i + 5] == 'o')
                        {
                            scanner_tokens.Add(new token_inf(token.end_do, i, line, "end do"));
                            i += 5;
                            state = 0;
                        }

                    }
                    return;
            }
        }

        int tokens_index;

        private string handleErrors(List<string> text_lines)
        {
            string outputString = "";

            for (int i = 0; i < scanner_tokens.Count; i++)
            {

                if (scanner_tokens[i].state == token.eqError ||
                    scanner_tokens[i].state == token.numError ||
                    scanner_tokens[i].state == token.comError ||
                    scanner_tokens[i].state == token.noEndDoError ||
                    scanner_tokens[i].state == token.afterEndDoTextError ||
                    scanner_tokens[i].state == token.idError ||
                    scanner_tokens[i].state == token.doError)
                {
                    switch (scanner_tokens[i].state)
                    {
                        case token.eqError:
                            outputString += $"Ошибка -> с{scanner_tokens[i].line + 1}п{scanner_tokens[i].position + 1} \"{scanner_tokens[i].tokenText}\". Ожидался оператор присваивания!\n";
                            break;
                        case token.numError:
                            outputString += $"Ошибка -> с{scanner_tokens[i].line + 1}п{scanner_tokens[i].position + 1} \"{scanner_tokens[i].tokenText}\". Неожиданное выражение!\n";
                            break;
                        case token.comError:
                            outputString += $"Ошибка -> с{scanner_tokens[i].line + 1}п{scanner_tokens[i].position + 1} \"{scanner_tokens[i].tokenText}\". Ожидался разделитель ','!\n";
                            break;
                        case token.noEndDoError:
                            outputString += $"Ошибка -> с{scanner_tokens[i].line + 1}п{scanner_tokens[i].position + 1} \"{scanner_tokens[i].tokenText}\". Ожидалось \"end do\"!\n";
                            break;
                        case token.afterEndDoTextError:
                            outputString += $"Ошибка -> с{scanner_tokens[i].line + 1}п{scanner_tokens[i].position + 1} \"{scanner_tokens[i].tokenText}\". Неожиданное выражение!\n";
                            break;
                        case token.idError:
                            outputString += $"Ошибка -> с{scanner_tokens[i].line + 1}п{scanner_tokens[i].position + 1} \"{scanner_tokens[i].tokenText}\". Ожидался идентификатор!\n";
                            break;
                        case token.doError:
                            outputString += $"Ошибка -> с{scanner_tokens[i].line + 1}п{scanner_tokens[i].position + 1} \"{scanner_tokens[i].tokenText}\". Ожидалось \"do\"!\n";
                            break;
                    }
                }

                if (i > 0) if (scanner_tokens[i].state == token.end && scanner_tokens[i - 1].state == token.com) 
                    { outputString += $"Ошибка -> с{scanner_tokens[i - 1].line + 1}п{scanner_tokens[i].position + 1}. Ожидался идентификатор!\n"; }
                if (i > 0) if (scanner_tokens[i].state == token.com && scanner_tokens[i - 1].state == token.eq)
                    { outputString += $"Ошибка -> с{scanner_tokens[i - 1].line + 1}п{scanner_tokens[i].position + 1}. Ожидался идентификатор!\n"; }

            }
            if (outputString == "") return "Ошибки не найдены.";
            else return outputString;
        }
    }
}