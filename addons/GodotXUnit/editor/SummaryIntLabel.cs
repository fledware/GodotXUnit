using Godot;

namespace GodotXUnit.editor
{
    [Tool]
    public class SummaryIntLabel : Label
    {
        public string StringFormat { get; private set; }

        private int _value = 0;

        public int TextValue
        {
            get => _value;
            set
            {
                _value = value;
                Text = string.Format(StringFormat, _value);
            }
        }

        public override void _Ready()
        {
            StringFormat = Text;
        }
    }
}