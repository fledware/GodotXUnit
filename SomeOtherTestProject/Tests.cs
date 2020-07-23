// using Godot;
// using GodotXUnitApi;
// using Xunit;
//
// namespace SomeOtherTestProject
// {
//     public class Tests
//     {
//         [Fact]
//         public async void AllThreadContextInOne()
//         {
//             Assert.False(Engine.IsInPhysicsFrame());
//             
//             await GodotXUnitEvents.OnPhysicsProcessAwaiter;
//             Assert.True(Engine.IsInPhysicsFrame());
//             
//             await GodotXUnitEvents.OnProcessAwaiter;
//             Assert.False(Engine.IsInPhysicsFrame());
//         }
//     }
// }