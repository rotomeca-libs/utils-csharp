using Rotomeca.Utils;
using Rotomeca.Utils.Functional;
using Xunit;

namespace Rotomeca.Utils.Tests;

public class PipelineTests
{
    // ── Pipeline.Start ────────────────────────────────────────────────────────

    [Fact]
    public void Pipeline_Start_WrapsValueCorrectly()
    {
        var pipe = Pipeline.Start(42);
        int result = pipe.Unpipe();
        Assert.Equal(42, result);
    }

    // ── PipeObject.Pipe ───────────────────────────────────────────────────────

    [Fact]
    public void PipeObject_Pipe_TransformsValue()
    {
        var result = Pipeline.Start(10)
            .Pipe(x => x * 2)
            .Unpipe();

        Assert.Equal(20, result);
    }

    [Fact]
    public void PipeObject_MultiplePipes_ChainsCorrectly()
    {
        var result = Pipeline.Start("  hello world  ")
            .Pipe(s => s.Trim())
            .Pipe(s => s.ToUpper())
            .Unpipe();

        Assert.Equal("HELLO WORLD", result);
    }

    [Fact]
    public void PipeObject_TypeChange_WorksAcrossTypes()
    {
        var result = Pipeline.Start("hello")
            .Pipe(s => s.Length)
            .Pipe(n => n * 2)
            .Unpipe();

        Assert.Equal(10, result);
    }

    // ── PipeObject implicit conversion ────────────────────────────────────────

    [Fact]
    public void PipeObject_ImplicitConversion_ReturnsValue()
    {
        int result = Pipeline.Start(42).Pipe(x => x + 1);
        Assert.Equal(43, result);
    }

    // ── Function.Pipe (static) ────────────────────────────────────────────────

    [Fact]
    public void Function_Pipe_1Fn_AppliesFunction()
    {
        var result = Function.Pipe(5, x => x * 2);
        Assert.Equal(10, result);
    }

    [Fact]
    public void Function_Pipe_2Fns_ChainsCorrectly()
    {
        var result = Function.Pipe(5,
            x => x * 2,
            x => x + 1);

        Assert.Equal(11, result);
    }

    [Fact]
    public void Function_Pipe_3Fns_ChainsCorrectly()
    {
        var result = Function.Pipe("hello",
            s => s.ToUpper(),
            s => s.Trim(),
            s => s.Length);

        Assert.Equal(5, result);
    }

    // ── Extension .Pipe() ─────────────────────────────────────────────────────

    [Fact]
    public void ExtensionPipe_OnAnyValue_WorksCorrectly()
    {
        int result = 10
            .Pipe(x => x * 3)
            .Pipe(x => x - 5)
            .Unpipe();

        Assert.Equal(25, result);
    }

    [Fact]
    public void ExtensionPipe_String_WorksCorrectly()
    {
        string result = "hello"
            .Pipe(s => s.ToUpper())
            .Pipe(s => s + "!")
            .Unpipe();

        Assert.Equal("HELLO!", result);
    }
}
