using Godot;
[Tool]
public partial class Test : Node3D {

    public override void _Process(double delta) {

        var spaceState = GetWorld3D().DirectSpaceState;
        var rayParams = PhysicsRayQueryParameters3D.Create(new(GlobalPosition.X, 9000000, GlobalPosition.Z), new(GlobalPosition.X, -9000000, GlobalPosition.Z));
        var hitPointsDictionary = spaceState.IntersectRay(rayParams);
        // if (hitPointsDictionary.Count != 0)
        // GD.Print($"HIT!!!");
    }
}
