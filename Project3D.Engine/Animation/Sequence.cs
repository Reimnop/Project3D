using Project3D.Engine.Exception;

namespace Project3D.Engine.Animation;

public delegate T Interpolator<T>(T a, T b, double t);

public class Sequence<T>
{
    private readonly List<Keyframe<T>> keyframes = new List<Keyframe<T>>();
    private readonly Interpolator<T> interpolator;

    private float lastTime = 0.0f;
    private int index = 0;

    public Sequence(Interpolator<T> interpolator)
    {
        this.interpolator = interpolator;
    }

    public Sequence(IEnumerable<Keyframe<T>> keyframes, Interpolator<T> interpolator) : this(interpolator)
    {
        this.keyframes.AddRange(keyframes);
        this.keyframes.Sort();
    }

    public void AddKeyframe(Keyframe<T> keyframe)
    {
        keyframes.InsertSorted(keyframe);
    }

    public T GetValue(float time)
    {
        // Can't get value without any keyframe
        if (keyframes.Count == 0)
        {
            throw new NoKeyframeException();
        }
        
        // There's only one possible outcome out of one keyframe
        if (keyframes.Count == 1)
        {
            return keyframes[0].Value;
        }
        
        // If it's out of range (before first keyframe), return first keyframe value
        if (time < keyframes[0].Time)
        {
            return keyframes[0].Value;
        }
        
        // Similarly for last keyframe
        if (time >= keyframes[^1].Time)
        {
            return keyframes[^1].Value;
        }

        // Find the correct index, then interpolate
        int step = time - lastTime >= 0 ? 1 : -1;
        lastTime = time;

        while (!(time >= keyframes[index].Time && time < keyframes[index + 1].Time))
        {
            index += step;
        }

        return GetValueBetweenKeyframes(keyframes[index], keyframes[index + 1], time);
    }

    private T GetValueBetweenKeyframes(Keyframe<T> a, Keyframe<T> b, double t)
    {
        return interpolator(a.Value, b.Value, (t - a.Time) / (b.Time - a.Time));
    }
}