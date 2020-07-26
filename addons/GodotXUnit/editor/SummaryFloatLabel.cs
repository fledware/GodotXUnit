using Godot;

namespace GodotXUnit.editor
{
    [Tool]
    public class SummaryFloatLabel : Label
    {
        public string StringFormat { get; private set; }

        private float _value = 0;

        public float TextValue
        {
            get => _value;
            set
            {
                _value = value;
                Text = string.Format(StringFormat, (int) (_value * 1000));
            }
        }

        public override void _Ready()
        {
            StringFormat = Text;
        }
    }
}