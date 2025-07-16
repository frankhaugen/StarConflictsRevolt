using System.Windows.Controls;

namespace StarConflictsRevolt.Clients.Wpf;

public class MyTextBox : GroupBox
{
    private readonly TextBox _textBox = new();

    public MyTextBox(string header, Action<object, TextChangedEventArgs> textChanged, string defaultText = "")
    {
        _textBox = new TextBox();
        Header = header;
        _textBox.Text = defaultText;
        _textBox.TextChanged += textChanged.Invoke;

        base.Content = _textBox;
    }

    public new string Content => _textBox.Text;
}