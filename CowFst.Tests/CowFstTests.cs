namespace CowFst.Tests;

public class CowFstTests
{
    [Fact]
    public void SuccessfulCommand_CausesTransition()
    {
        var fst = GivenAnFst();

        var result = fst.HandleCommand(new TestCommand.ChangeName("cow"));

        ThenResultIsSuccess(result, new TestEvent.NameChanged("cow"));
        ThenStateIs(fst.GetState(), new TestState("cow", 1));
    }

    [Fact]
    public void UnsuccessfulCommand_ReturnsError()
    {
        var fst = GivenAnFst();
        var originalState = fst.GetState();

        var result = fst.HandleCommand(new TestCommand.ChangeName("nameless"));

        ThenResultIsError(result, new TestError.CannotChangeName("result cannot be nameless"));
        ThenStateIs(fst.GetState(), originalState);
    }

    static Fst<TestState, TestCommand, TestEvent, TestError, TestContext> GivenAnFst()
    {
        return new Fst<TestState, TestCommand, TestEvent, TestError, TestContext>(
            (s, c, ctx) => 
            {
                return c switch
                {
                    TestCommand.ChangeName cn when cn.Name == "nameless" => new Result<TestEvent, TestError>.Err(new TestError.CannotChangeName("result cannot be nameless")),
                    TestCommand.ChangeName cn => new Result<TestEvent, TestError>.Success(new TestEvent.NameChanged(cn.Name)),
                    TestCommand.ChangeValue cv when cv.Value > 10 => new Result<TestEvent, TestError>.Err(new TestError.CannotChangeValue("value must be <= 10")),
                    TestCommand.ChangeValue cv => new Result<TestEvent, TestError>.Success(new TestEvent.ValueChanged(cv.Value)),
                    _ => new Result<TestEvent, TestError>.Err(new TestError.UnknownCommand($"unknown command")),
                };
            },
            (s, e) =>
            {
                return e switch
                {
                    TestEvent.NameChanged n => s with { Name = n.Name },
                    TestEvent.ValueChanged v => s with { Value = v.Value },
                    _ => s,
                };
            },
            new TestContext(),
            new TestState("a", 1)
        );
    }

    private static void ThenStateIs(TestState actual, TestState expected)
    {
        Assert.Equal(expected, actual);
    }

    private static void ThenResultIsSuccess(Result<TestEvent, TestError> result, TestEvent.NameChanged expected)
    {
        switch (result)
        {
            case Result<TestEvent, TestError>.Success s:
                Assert.Equal(expected, s.Value);
                break;
            default:
                Assert.Fail("expected Result.Success but found Result.Err");
                break;
        }
    }

    private void ThenResultIsError(Result<TestEvent, TestError> result, TestError.CannotChangeName expected)
    {
        switch (result)
        {
            case Result<TestEvent, TestError>.Err e:
                Assert.Equal(expected, e.Error);
                break;
            default:
                Assert.Fail("expected Result.Success but found Result.Err");
                break;
        }
    }
}

internal record TestState(string Name, int Value);

internal abstract record TestCommand
{
    private TestCommand() {}

    public sealed record ChangeName(string Name) : TestCommand;
    public sealed record ChangeValue(int Value) : TestCommand;
}

internal abstract record TestEvent
{
    private TestEvent() {}
    public sealed record NameChanged(string Name) : TestEvent;
    public sealed record ValueChanged(int Value) : TestEvent;
}

internal abstract record TestError
{
    private TestError() {}

    public sealed record CannotChangeName(string Error) : TestError;
    public sealed record CannotChangeValue(string Error) : TestError;
    public sealed record UnknownCommand(string Error) : TestError;
}

internal record TestContext();