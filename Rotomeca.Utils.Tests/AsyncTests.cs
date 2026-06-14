using Rotomeca.Utils.Async;
using Rotomeca.Utils.Async.Helpers;
using Xunit;

namespace Rotomeca.Utils.Tests;

public class AsyncTests
{
    // ── Sleep ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Sleep_Ms_WaitsApproximatelyCorrectDuration()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await Asynchronous.Sleep(50u);
        sw.Stop();

        Assert.True(sw.ElapsedMilliseconds >= 40,
            $"Sleep trop court : {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task Sleep_WithCancellation_ThrowsWhenCancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(30);

        await Assert.ThrowsAsync<TaskCanceledException>(
            () => Asynchronous.Sleep(500u, cts.Token));
    }

    [Fact]
    public async Task Sleep_TimeSpan_WaitsCorrectly()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await Asynchronous.Sleep(TimeSpan.FromMilliseconds(50));
        sw.Stop();

        Assert.True(sw.ElapsedMilliseconds >= 40);
    }

    // ── Retry ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Retry_FirstAttemptSucceeds_ReturnsResult()
    {
        int callCount = 0;
        var result = await Asynchronous.Retry(
            () => { callCount++; return Task.FromResult(42); },
            attempts: 3,
            delay: 0u);

        Assert.Equal(42, result);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task Retry_FailsThenSucceeds_RetriesUntilSuccess()
    {
        int callCount = 0;
        var result = await Asynchronous.Retry(
            () =>
            {
                callCount++;
                if (callCount < 3) throw new InvalidOperationException("not yet");
                return Task.FromResult(99);
            },
            attempts: 5,
            delay: 0u);

        Assert.Equal(99, result);
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task Retry_AllAttemptsFail_ThrowsAggregateException()
    {
        await Assert.ThrowsAsync<AggregateException>(() =>
            Asynchronous.Retry(
                () => Task.FromException<int>(new InvalidOperationException("fail")),
                attempts: 3,
                delay: 0u));
    }

    [Fact]
    public async Task Retry_Cancelled_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            Asynchronous.Retry(
                () => Task.FromResult(1),
                attempts: 3,
                delay: 0u,
                cancellationToken: cts.Token));
    }

    [Fact]
    public async Task Retry_AttemptNumberClampsToOne_WhenZeroPassed()
    {
        // AttemptNumber(0) est clampé à 1 → exactement 1 tentative
        int callCount = 0;
        await Assert.ThrowsAsync<AggregateException>(() =>
            Asynchronous.Retry(
                () =>
                {
                    callCount++;
                    throw new InvalidOperationException("fail");
                    return Task.FromResult(0); // unreachable
                },
                attempts: (AttemptNumber)0u, // clampé à 1
                delay: 0u));

        Assert.Equal(1, callCount);
    }

    // ── Timeout ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Timeout_FastTask_ReturnsResult()
    {
        var result = await Asynchronous.Timeout(
            Task.FromResult(42),
            TimeSpan.FromSeconds(5));

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task Timeout_SlowTask_ThrowsTimeoutException()
    {
        await Assert.ThrowsAsync<TimeoutException>(() =>
            Asynchronous.Timeout(
                Task.Delay(500).ContinueWith(_ => 0),
                TimeSpan.FromMilliseconds(50)));
    }

    [Fact]
    public async Task Timeout_Ms_FastTask_ReturnsResult()
    {
        var result = await Asynchronous.Timeout(Task.FromResult(7), 5000u);
        Assert.Equal(7, result);
    }

    // ── Parallel ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Parallel_MultipleTasks_ReturnsAllResults()
    {
        var results = await Asynchronous.Parallel(
            () => Task.FromResult(1),
            () => Task.FromResult(2),
            () => Task.FromResult(3));

        Assert.Equal(3, results.Length);
        Assert.Contains(1, results);
        Assert.Contains(2, results);
        Assert.Contains(3, results);
    }

    // ── Sequential ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Sequential_Tasks_ReturnsResultsInOrder()
    {
        var order = new List<int>();

        int[] results = await Asynchronous.Sequential([
            async () => { await Task.Delay(10); order.Add(1); return 1; },
            async () => { await Task.Delay(5);  order.Add(2); return 2; },
            () => { order.Add(3); return Task.FromResult(3); }]);

        Assert.Equal((int[])[1, 2, 3], results);
        Assert.Equal([1, 2, 3], order); // exécution dans l'ordre
    }

    // ── SetTimeout / ClearTimeout ─────────────────────────────────────────────

    [Fact]
    public async Task SetTimeout_CallbackExecutedAfterDelay()
    {
        bool executed = false;
        Asynchronous.SetTimeout(() => executed = true, 50);

        await Task.Delay(200);

        Assert.True(executed);
    }

    [Fact]
    public async Task ClearTimeout_PreventsCallback()
    {
        bool executed = false;
        int id = Asynchronous.SetTimeout(() => executed = true, 200);
        Asynchronous.ClearTimeout(id);

        await Task.Delay(400);

        Assert.False(executed);
    }
}
