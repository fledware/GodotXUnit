// using Godot;
// using GodotXUnitApi;
// using Xunit;
//
// namespace GodotCSUnitTest.Tests
// {
//     public class SomeTestSceneTest
//     {
//         [FactOnScene("res://test_scenes/SomeTestScene.tscn")]
//         public void IsOnCorrectScene()
//         {
//             var scene = GodotXUnitEvents.Instance.GetTree().CurrentScene;
//             Assert.Equal(typeof(SomeTestSceneRoot), scene?.GetType());
//         }
//
//         [FactOnProcess]
//         public void IsNotInSomeTestScene()
//         {
//             var scene = GodotXUnitEvents.Instance.GetTree().CurrentScene;
//             Assert.NotEqual(typeof(SomeTestSceneRoot), scene?.GetType());
//         }
//     }
// }