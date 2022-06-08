using Project3D.Engine.Exception;

namespace Project3D.Engine.Animation;

public class Sequence<T>
{
    private readonly List<Keyframe<T>> keyframes = new List<Keyframe<T>>();

    private float lastTime = 0.0f;
    private int index = 0;
    
    public Sequence()
    {
    }

    public Sequence(IEnumerable<Keyframe<T>> keyframes)
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
        if (keyframes.Count == 0)
        {
            throw new NoKeyframeException();
        }

        if (keyframes.Count == 1)
        {
            return keyframes[0].Value;
        }

        if (time < keyframes[0].Time)
        {
            return keyframes[0].Value;
        }

        if (time >= keyframes[^1].Time)
        {
            return keyframes[^1].Value;
        }

        if (time >= keyframes[index].Time && time < keyframes[index + 1].Time)
        {
            return keyframes[index].Value;
        }

        int step = time - lastTime >= 0 ? 1 : -1;
        lastTime = time;

        while (!(time >= keyframes[index].Time && time < keyframes[index + 1].Time))
        {
            index += step;
        }

        return keyframes[index].Value;
    }
}