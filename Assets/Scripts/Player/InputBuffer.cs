
using System.Collections.Generic;

public class InputBuffer
{
    private readonly List<PlayerInputData> inputs = new();

    public void Add(PlayerInputData input)
    {
        inputs.Add(input);
    }

    public void RemoveConfirmed(uint tick)
    {
        inputs.RemoveAll(input => input.Tick <= tick);
    }

    public IReadOnlyList<PlayerInputData> GetPending()
    {
        return inputs;
    }

    public void Clear()
    {
        inputs.Clear();
    }
}
