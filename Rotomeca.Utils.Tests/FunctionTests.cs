using Rotomeca.Utils.Functional;
using Xunit;

namespace Rotomeca.Utils.Tests;

public class FunctionTests
{
    // ── Noop ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Noop_DoesNotThrow()
    {
        var ex = Record.Exception(Function.Noop);
        Assert.Null(ex);
    }

    // ── Identity ──────────────────────────────────────────────────────────────

    [Fact]
    public void Identity_Int_ReturnsUnchangedValue()
        => Assert.Equal(42, Function.Identity(42));

    [Fact]
    public void Identity_String_ReturnsUnchangedValue()
        => Assert.Equal("hello", Function.Identity("hello"));

    [Fact]
    public void Identity_Null_ReturnsNull()
        => Assert.Null(Function.Identity<string?>(null));

    // ── Memoize (0 arg) ───────────────────────────────────────────────────────

    [Fact]
    public void Memoize_NoArgs_FnCalledOnlyOnce()
    {
        int callCount = 0;
        var memoized = Function.Memoize(() => { callCount++; return 42; });

        memoized();
        memoized();
        memoized();

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Memoize_NoArgs_ReturnsCachedResult()
    {
        var memoized = Function.Memoize(() => 42);
        Assert.Equal(42, memoized());
        Assert.Equal(42, memoized());
    }

    // ── Memoize (1 arg) ───────────────────────────────────────────────────────

    [Fact]
    public void Memoize_1Arg_SameArg_FnCalledOnlyOnce()
    {
        int callCount = 0;
        var memoized = Function.Memoize((int x) => { callCount++; return x * 2; });

        memoized(5);
        memoized(5);
        memoized(5);

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Memoize_1Arg_DifferentArgs_FnCalledForEach()
    {
        int callCount = 0;
        var memoized = Function.Memoize((int x) => { callCount++; return x * 2; });

        memoized(1);
        memoized(2);
        memoized(3);

        Assert.Equal(3, callCount);
    }

    [Fact]
    public void Memoize_1Arg_ReturnsCachedResult()
    {
        var memoized = Function.Memoize((int x) => x * 2);
        Assert.Equal(10, memoized(5));
        Assert.Equal(10, memoized(5)); // depuis le cache
    }

    [Fact]
    public void Memoize_1Arg_NullArg_Cached()
    {
        int callCount = 0;
        var memoized = Function.Memoize((string? s) => { callCount++; return s?.Length ?? -1; });

        memoized(null);
        memoized(null);

        Assert.Equal(1, callCount);
    }

    // ── Memoize (2 args) ──────────────────────────────────────────────────────

    [Fact]
    public void Memoize_2Args_SameArgs_FnCalledOnlyOnce()
    {
        int callCount = 0;
        var memoized = Function.Memoize((int a, int b) => { callCount++; return a + b; });

        memoized(1, 2);
        memoized(1, 2);

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Memoize_2Args_PartiallyDifferent_CalledAgain()
    {
        int callCount = 0;
        var memoized = Function.Memoize((int a, int b) => { callCount++; return a + b; });

        memoized(1, 2);
        memoized(1, 3); // b différent

        Assert.Equal(2, callCount);
    }

    // ── Memoize (3 args) ──────────────────────────────────────────────────────

    [Fact]
    public void Memoize_3Args_SameArgs_FnCalledOnlyOnce()
    {
        int callCount = 0;
        var memoized = Function.Memoize((int a, int b, int c) =>
        {
            callCount++;
            return a + b + c;
        });

        memoized(1, 2, 3);
        memoized(1, 2, 3);

        Assert.Equal(1, callCount);
    }

    // ── Once (0 arg) ──────────────────────────────────────────────────────────

    [Fact]
    public void Once_NoArgs_ExecutesOnlyOnce()
    {
        int callCount = 0;
        var once = Function.Once(() => { callCount++; return 42; });

        once();
        once();
        once();

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Once_NoArgs_AlwaysReturnsSameResult()
    {
        var once = Function.Once(() => 42);
        Assert.Equal(42, once());
        Assert.Equal(42, once());
    }

    // ── Once (1 arg) ──────────────────────────────────────────────────────────

    [Fact]
    public void Once_1Arg_ExecutesOnlyOnFirstCall()
    {
        int callCount = 0;
        var once = Function.Once((int x) => { callCount++; return x * 2; });

        once(5);
        once(10); // ignoré
        once(15); // ignoré

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Once_1Arg_SubsequentCallsReturnFirstResult()
    {
        var once = Function.Once((int x) => x * 2);

        var first = once(5);
        var second = once(100); // 100 * 2 = 200 ne sera PAS utilisé

        Assert.Equal(10, first);
        Assert.Equal(10, second); // toujours le premier résultat
    }

    // ── Once (2 args) ─────────────────────────────────────────────────────────

    [Fact]
    public void Once_2Args_ExecutesOnlyOnFirstCall()
    {
        int callCount = 0;
        var once = Function.Once((int a, int b) => { callCount++; return a + b; });

        once(1, 2);
        once(3, 4);

        Assert.Equal(1, callCount);
    }

    // ── Debounce ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Debounce_RapidCalls_ExecutesOnlyOnce()
    {
        int callCount = 0;
        var debounced = Function.Debounce(() => callCount++, 50);

        debounced();
        debounced();
        debounced();

        await Task.Delay(150); // attendre que le timer se déclenche

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task Debounce_SpacedCalls_ExecutesMultipleTimes()
    {
        int callCount = 0;
        var debounced = Function.Debounce(() => callCount++, 50);

        debounced();
        await Task.Delay(150);
        debounced();
        await Task.Delay(150);

        Assert.Equal(2, callCount);
    }

    // ── Throttle ──────────────────────────────────────────────────────────────

    [Fact]
    public void Throttle_RapidCalls_ExecutesOnlyOnce()
    {
        int callCount = 0;
        var throttled = Function.Throttle(() => callCount++, 500);

        throttled();
        throttled(); // ignoré dans la fenêtre
        throttled(); // ignoré dans la fenêtre

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task Throttle_AfterWindow_ExecutesAgain()
    {
        int callCount = 0;
        var throttled = Function.Throttle(() => callCount++, 50);

        throttled();
        await Task.Delay(150);
        throttled();

        Assert.Equal(2, callCount);
    }
}
