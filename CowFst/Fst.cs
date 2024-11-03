namespace CowFst;

public class Fst<TState, TCommand, TEvent, TError, TContext>(
    Func<TState, TCommand, TContext, Result<TEvent, TError>> commandHandler,
    Func<TState, TEvent, TState> transition,
    TContext context, 
    TState initialState
)
{
    private TState _currentState = initialState;

    public Result<TEvent, TError> HandleCommand(TCommand c)
    {
        var result = commandHandler(_currentState, c, context);
        
        switch (result)
        {
            case Result<TEvent, TError>.Success s:
                ApplyEvent(s.Value);
                break;
        }

        return result;
    }

    public void ApplyEvent(TEvent e)
    {
        _currentState = transition(_currentState, e);
    }

    public TState GetState()
    {
        return _currentState;
    }
}

public abstract record Result<T, E>
{
    private Result() {}

    public sealed record Success(T Value) : Result<T, E>();
    public sealed record Err(E Error) : Result<T, E>();
}