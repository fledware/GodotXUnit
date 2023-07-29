using Godot;
using GodotXUnitApi;
using Xunit;

namespace GodotXUnitTest.Tests;

public partial class ClickTest
{
    [GodotFact(Scene = "res://test_scenes/ClickTestScene.tscn")]
    public async void TestButtonGetsClicked()
    {
        var checkBox = (Button)GDU.CurrentScene.FindChild("ImAButton");
        Assert.NotNull(checkBox);
        Assert.False(checkBox.ButtonPressed);

        await GDU.RequestDrawing(60, drawer =>
        {
            var position = GDI.PositionByScreenPercent(0.5f, 0.5f);
            drawer.DrawCircle(position, 30f, Colors.Azure);
        });

        await GDI.SimulateMouseClick(0.5f, 0.5f);
        Assert.True(checkBox.ButtonPressed);
    }
}