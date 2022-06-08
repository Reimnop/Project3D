namespace Project3D.Engine.Exception;

public class NoKeyframeException : System.Exception
{
    public override string Message => "No keyframe in sequence!";
}