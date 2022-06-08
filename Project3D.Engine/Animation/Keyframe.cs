namespace Project3D.Engine.Animation;

public struct Keyframe<T> : IComparable<Keyframe<T>>
{
    public double Time { get; set; }
    public T Value { get; set; }

    public Keyframe(double time, T value)
    {
        Time = time;
        Value = value;
    }

    public int CompareTo(Keyframe<T> other)
    {
        return Time.CompareTo(other.Time);
    }
}