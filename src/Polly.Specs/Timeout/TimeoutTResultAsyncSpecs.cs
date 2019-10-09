﻿using FluentAssertions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Polly.Specs.Helpers;
using Polly.Timeout;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Timeout
{
    [Collection(Polly.Specs.Helpers.Constants.SystemClockDependentTestCollection)]
    public class TimeoutTResultAsyncSpecs : TimeoutSpecsBase
    {
        #region Configuration

        [Fact]
        public void Should_throw_when_timeout_is_zero_by_timespan()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.Zero);

            policy.Should().Throw<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("timeout");
        }

        [Fact]
        public void Should_throw_when_timeout_is_zero_by_seconds()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(0);

            policy.Should().Throw<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("seconds");
        }

        [Fact]
        public void Should_throw_when_timeout_is_less_than_zero_by_timespan()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(-TimeSpan.FromHours(1));

            policy.Should().Throw<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("timeout");
        }

        [Fact]
        public void Should_throw_when_timeout_is_less_than_zero_by_seconds()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(-10);

            policy.Should().Throw<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("seconds");
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_greater_than_zero_by_timespan()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromMilliseconds(1));

            policy.Should().NotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_greater_than_zero_by_seconds()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(3);

            policy.Should().NotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_maxvalue()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.MaxValue);

            policy.Should().NotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_seconds_is_maxvalue()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(int.MaxValue);

            policy.Should().NotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_infinitetimespan()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(System.Threading.Timeout.InfiniteTimeSpan);

            policy.Should().NotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_infinitetimespan_with_timeoutstrategy()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(System.Threading.Timeout.InfiniteTimeSpan, TimeoutStrategy.Optimistic);

            policy.Should().NotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_infinitetimespan_with_ontimeout()
        {
            Func<Context, TimeSpan, Task, Task> doNothingAsync = (_, __, ___) => TaskHelper.EmptyTask;
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(System.Threading.Timeout.InfiniteTimeSpan, doNothingAsync);

            policy.Should().NotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_infinitetimespan_with_timeoutstrategy_and_ontimeout()
        {
            Func<Context, TimeSpan, Task, Task> doNothingAsync = (_, __, ___) => TaskHelper.EmptyTask;
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(System.Threading.Timeout.InfiniteTimeSpan, TimeoutStrategy.Optimistic, doNothingAsync);

            policy.Should().NotThrow();
        }

        [Fact]
        public void Should_throw_when_onTimeout_is_null_with_timespan()
        {
            Func<Context, TimeSpan, Task, Task> onTimeout = null;
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromMinutes(0.5), onTimeout);

            policy.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("onTimeoutAsync");
        }

        [Fact]
        public void Should_throw_when_onTimeout_is_null_with_timespan_with_full_argument_list_onTimeout()
        {
            Func<Context, TimeSpan, Task, Exception, Task> onTimeout = null;
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromMinutes(0.5), onTimeout);

            policy.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("onTimeoutAsync");
        }

        [Fact]
        public void Should_throw_when_onTimeout_is_null_with_seconds()
        {
            Func<Context, TimeSpan, Task, Task> onTimeout = null;
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(30, onTimeout);

            policy.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("onTimeoutAsync");
        }


        [Fact]
        public void Should_throw_when_onTimeout_is_null_with_seconds_with_full_argument_list_onTimeout()
        {
            Func<Context, TimeSpan, Task, Exception, Task> onTimeout = null;
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(30, onTimeout);

            policy.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("onTimeoutAsync");
        }

        [Fact]
        public void Should_throw_when_timeoutProvider_is_null()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>((Func<TimeSpan>)null);

            policy.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("timeoutProvider");
        }

        [Fact]
        public void Should_throw_when_onTimeout_is_null_with_timeoutprovider()
        {
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = null;
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(() => TimeSpan.FromSeconds(30), onTimeoutAsync);

            policy.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("onTimeoutAsync");
        }

        [Fact]
        public void Should_throw_when_onTimeout_is_null_with_timeoutprovider_for_full_argument_list_onTimeout()
        {
            Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync = null;
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(() => TimeSpan.FromSeconds(30), onTimeoutAsync);

            policy.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("onTimeoutAsync");
        }

        [Fact]
        public void Should_be_able_to_configure_with_timeout_func()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(() => TimeSpan.FromSeconds(1));

            policy.Should().NotThrow();
        }

        #endregion

        #region Timeout operation - pessimistic

        [Fact]
        public void Should_throw_when_timeout_is_less_than_execution_duration__pessimistic()
        {
            TimeSpan timeout = TimeSpan.FromMilliseconds(50);

            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic);

            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }).ConfigureAwait(false)).Should().Throw<TimeoutRejectedException>();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_greater_than_execution_duration__pessimistic()
        {
            var policy = Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromSeconds(1), TimeoutStrategy.Pessimistic);

            ResultPrimitive result = ResultPrimitive.Undefined;

            Func<Task> act = async () => result = await policy.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good))
                    .ConfigureAwait(false);

            act.Should().NotThrow();
            result.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public void Should_throw_timeout_after_correct_duration__pessimistic()
        {
            Stopwatch watch = new Stopwatch();

            TimeSpan timeout = TimeSpan.FromSeconds(1);
            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic);

            TimeSpan tolerance = TimeSpan.FromSeconds(3); // Consider increasing tolerance, if test fails transiently in different test/build environments.

            watch.Start();
            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(10), CancellationToken.None).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }))
                .Should().Throw<TimeoutRejectedException>();
            watch.Stop();

            watch.Elapsed.Should().BeCloseTo(timeout, ((int)tolerance.TotalMilliseconds));
        }

        [Fact]
        public void Should_rethrow_exception_from_inside_delegate__pessimistic()
        {
            var policy = Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromSeconds(10), TimeoutStrategy.Pessimistic);

            policy.Awaiting(p => p.ExecuteAsync(() => { throw new NotImplementedException(); })).Should().Throw<NotImplementedException>();
        }

        #endregion

        #region Timeout operation - optimistic

        [Fact]
        public void Should_throw_when_timeout_is_less_than_execution_duration__optimistic()
        {
            var policy = Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromMilliseconds(50), TimeoutStrategy.Optimistic);
            var userCancellationToken = CancellationToken.None;

            policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }, userCancellationToken).ConfigureAwait(false)).Should().Throw<TimeoutRejectedException>();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_greater_than_execution_duration__optimistic()
        {
            var policy = Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromSeconds(1), TimeoutStrategy.Optimistic);

            ResultPrimitive result = ResultPrimitive.Undefined;
            var userCancellationToken = CancellationToken.None;

            Func<Task> act = async () => result = await policy.ExecuteAsync(ct => Task.FromResult(ResultPrimitive.Good), userCancellationToken)
                .ConfigureAwait(false);
            
            act.Should().NotThrow();
            result.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public void Should_throw_timeout_after_correct_duration__optimistic()
        {
            Stopwatch watch = new Stopwatch();

            TimeSpan timeout = TimeSpan.FromSeconds(1);
            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Optimistic);
            var userCancellationToken = CancellationToken.None;

            TimeSpan tolerance = TimeSpan.FromSeconds(3); // Consider increasing tolerance, if test fails transiently in different test/build environments.

            watch.Start();
            policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(10), ct).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }, userCancellationToken).ConfigureAwait(false))
                .Should().Throw<TimeoutRejectedException>();
            watch.Stop();

            watch.Elapsed.Should().BeCloseTo(timeout, ((int)tolerance.TotalMilliseconds));
        }

        [Fact]
        public void Should_rethrow_exception_from_inside_delegate__optimistic()
        {
            var policy = Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromSeconds(10), TimeoutStrategy.Optimistic);

            policy.Awaiting(p => p.ExecuteAsync(() => { throw new NotImplementedException(); })).Should().Throw<NotImplementedException>();
        }

        #endregion

        #region Non-timeout cancellation - pessimistic (user-delegate does not observe cancellation)

        [Fact]
        public void Should_not_be_able_to_cancel_with_unobserved_user_cancellation_token_before_timeout__pessimistic()
        {
            int timeout = 5;
            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic);

            using (CancellationTokenSource userTokenSource = new CancellationTokenSource())
            {
                policy.Awaiting(async p => await p.ExecuteAsync(async
                    _ => {
                        userTokenSource.Cancel(); // User token cancels in the middle of execution ...
                        await SystemClock.SleepAsync(TimeSpan.FromSeconds(timeout * 2),
                            CancellationToken.None // ... but if the executed delegate does not observe it
                            ).ConfigureAwait(false);
                        return ResultPrimitive.WhateverButTooLate;
                    }, userTokenSource.Token)
                   ).Should().Throw<TimeoutRejectedException>(); // ... it's still the timeout we expect.
            }
        }

        [Fact]
        public void Should_not_execute_user_delegate_if_user_cancellationtoken_cancelled_before_delegate_reached__pessimistic()
        {
            var policy = Policy.TimeoutAsync<ResultPrimitive>(10, TimeoutStrategy.Pessimistic);

            bool executed = false;

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Cancel();

                policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
                {
                    executed = true;
                    await TaskHelper.EmptyTask.ConfigureAwait(false);
                    return ResultPrimitive.WhateverButTooLate;
                }, cts.Token))
                .Should().Throw<OperationCanceledException>();
            }

            executed.Should().BeFalse();
        }

        #endregion

        #region Non-timeout cancellation - optimistic (user-delegate observes cancellation)

        [Fact]
        public void Should_be_able_to_cancel_with_user_cancellation_token_before_timeout__optimistic()
        {
            int timeout = 10;
            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Optimistic);
            using (CancellationTokenSource userTokenSource = new CancellationTokenSource())
            {
                policy.Awaiting(async p => await p.ExecuteAsync( 
                    ct => {
                        userTokenSource.Cancel(); ct.ThrowIfCancellationRequested();   // Simulate cancel in the middle of execution
                        return Task.FromResult(ResultPrimitive.WhateverButTooLate);
                    }, userTokenSource.Token) // ... with user token.
                   ).Should().Throw<OperationCanceledException>();
            }
        }

        [Fact]
        public void Should_not_execute_user_delegate_if_user_cancellationToken_cancelled_before_delegate_reached__optimistic()
        {
            var policy = Policy.TimeoutAsync<ResultPrimitive>(10, TimeoutStrategy.Optimistic);

            bool executed = false;

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Cancel();

                policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
                {
                    executed = true;
                    await TaskHelper.EmptyTask.ConfigureAwait(false);
                    return ResultPrimitive.WhateverButTooLate;
                }, cts.Token))
                .Should().Throw<OperationCanceledException>();
            }

            executed.Should().BeFalse();
        }

        #endregion

        #region onTimeout overload - pessimistic

        [Fact]
        public void Should_call_ontimeout_with_configured_timeout__pessimistic()
        {
            TimeSpan timeoutPassedToConfiguration = TimeSpan.FromMilliseconds(250);

            TimeSpan? timeoutPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                timeoutPassedToOnTimeout = span;
                return TaskHelper.EmptyTask;
            };

            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutPassedToConfiguration, TimeoutStrategy.Pessimistic, onTimeoutAsync);

            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }))
            .Should().Throw<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutPassedToConfiguration);
        }

        [Fact]
        public void Should_call_ontimeout_with_passed_context__pessimistic()
        {
            string operationKey = "SomeKey";
            Context contextPassedToExecute = new Context(operationKey);

            Context contextPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                contextPassedToOnTimeout = ctx;
                return TaskHelper.EmptyTask;
            };

            TimeSpan timeout = TimeSpan.FromMilliseconds(250);
            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic, onTimeoutAsync);

            policy.Awaiting(async p => await p.ExecuteAsync(async ctx =>
                {
                    await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);
                    return ResultPrimitive.WhateverButTooLate;
                }, contextPassedToExecute))
                .Should().Throw<TimeoutRejectedException>();

            contextPassedToOnTimeout.Should().NotBeNull();
            contextPassedToOnTimeout.OperationKey.Should().Be(operationKey);
            contextPassedToOnTimeout.Should().BeSameAs(contextPassedToExecute);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func__pessimistic(int programaticallyControlledDelay)
        {
            Func<TimeSpan> timeoutFunc = () => TimeSpan.FromMilliseconds(25* programaticallyControlledDelay);

            TimeSpan? timeoutPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                timeoutPassedToOnTimeout = span;
                return TaskHelper.EmptyTask;
            };

            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutFunc, TimeoutStrategy.Pessimistic, onTimeoutAsync);

            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
                {
                    await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);
                    return ResultPrimitive.WhateverButTooLate;
                }))
                .Should().Throw<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutFunc());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func_influenced_by_context__pessimistic(int programaticallyControlledDelay)
        {
            Func<Context, TimeSpan> timeoutProvider = ctx => (TimeSpan)ctx["timeout"];

            TimeSpan? timeoutPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                timeoutPassedToOnTimeout = span;
                return TaskHelper.EmptyTask;
            };

            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutProvider, TimeoutStrategy.Pessimistic, onTimeoutAsync);

            // Supply a programatically-controlled timeout, via the execution context.
            Context context = new Context("SomeOperationKey") { ["timeout"] = TimeSpan.FromMilliseconds(25 * programaticallyControlledDelay) };

            policy.Awaiting(async p => await p.ExecuteAsync(async ctx =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }, context))
            .Should().Throw<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutProvider(context));
        }

        [Fact]
        public void Should_call_ontimeout_with_task_wrapping_abandoned_action__pessimistic()
        {
            Task taskPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                taskPassedToOnTimeout = task;
                return TaskHelper.EmptyTask;
            };

            TimeSpan timeout = TimeSpan.FromMilliseconds(250);
            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic, onTimeoutAsync);

            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }))
            .Should().Throw<TimeoutRejectedException>();

            taskPassedToOnTimeout.Should().NotBeNull();
        }

        [Fact]
        public async Task Should_call_ontimeout_with_task_wrapping_abandoned_action_allowing_capture_of_otherwise_unobserved_exception__pessimistic()
        {
            SystemClock.Reset(); // This is the only test which cannot work with the artificial SystemClock of TimeoutSpecsBase.  We want the invoked delegate to continue as far as: throw exceptionToThrow, to genuinely check that the walked-away-from task throws that, and that we pass it to onTimeoutAsync.  
            // That means we can't use the SystemClock.SleepAsync(...) within the executed delegate to artificially trigger the timeout cancellation (as for example the test above does).
            // In real execution, it is the .WhenAny() in the timeout implementation which throws for the timeout.  We don't want to go as far as abstracting Task.WhenAny() out into SystemClock, so we let this test run at real-world speed, not abstracted-clock speed.

            Exception exceptionToThrow = new DivideByZeroException();

            Exception exceptionObservedFromTaskPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                task.ContinueWith(t => exceptionObservedFromTaskPassedToOnTimeout = t.Exception.InnerException); // Intentionally not awaited: we want to assign the continuation, but let it run in its own time when the executed delegate eventually completes.
                return TaskHelper.EmptyTask;
            };

            TimeSpan shimTimespan = TimeSpan.FromSeconds(1); // Consider increasing shimTimeSpan if test fails transiently in different environments.
            TimeSpan thriceShimTimeSpan = shimTimespan + shimTimespan + shimTimespan;
            var policy = Policy.TimeoutAsync<ResultPrimitive>(shimTimespan, TimeoutStrategy.Pessimistic, onTimeoutAsync);

            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
            {
                await SystemClock.SleepAsync(thriceShimTimeSpan, CancellationToken.None).ConfigureAwait(false);
                throw exceptionToThrow;
            }))
            .Should().Throw<TimeoutRejectedException>();

            await SystemClock.SleepAsync(thriceShimTimeSpan, CancellationToken.None).ConfigureAwait(false);
            exceptionObservedFromTaskPassedToOnTimeout.Should().NotBeNull();
            exceptionObservedFromTaskPassedToOnTimeout.Should().Be(exceptionToThrow);

        }

        [Fact]
        public void Should_call_ontimeout_with_timing_out_exception__pessimistic()
        {
            TimeSpan timeoutPassedToConfiguration = TimeSpan.FromMilliseconds(250);

            Exception exceptionPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync = (ctx, span, task, exception) =>
            {
                exceptionPassedToOnTimeout = exception;
                return TaskHelper.EmptyTask;
            };

            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutPassedToConfiguration, TimeoutStrategy.Pessimistic, onTimeoutAsync);

            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
                {
                    await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);
                    return ResultPrimitive.WhateverButTooLate;
                }))
                .Should().Throw<TimeoutRejectedException>();

            exceptionPassedToOnTimeout.Should().NotBeNull();
            exceptionPassedToOnTimeout.Should().BeOfType(typeof(OperationCanceledException));
        }

        #endregion

        #region onTimeout overload - optimistic

        [Fact]
        public void Should_call_ontimeout_with_configured_timeout__optimistic()
        {
            TimeSpan timeoutPassedToConfiguration = TimeSpan.FromMilliseconds(250);

            TimeSpan? timeoutPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                timeoutPassedToOnTimeout = span;
                return TaskHelper.EmptyTask;
            };

            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutPassedToConfiguration, TimeoutStrategy.Optimistic, onTimeoutAsync);
            var userCancellationToken = CancellationToken.None;

            policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }, userCancellationToken).ConfigureAwait(false))
            .Should().Throw<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutPassedToConfiguration);
        }

        [Fact]
        public void Should_call_ontimeout_with_passed_context__optimistic()
        {
            string operationKey = "SomeKey";
            Context contextPassedToExecute = new Context(operationKey);

            Context contextPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                contextPassedToOnTimeout = ctx;
                return TaskHelper.EmptyTask;
            };

            TimeSpan timeout = TimeSpan.FromMilliseconds(250);
            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Optimistic, onTimeoutAsync);
            var userCancellationToken = CancellationToken.None;

            policy.Awaiting(async p => await p.ExecuteAsync(async (ctx, ct) =>
                {
                    await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);
                    return ResultPrimitive.WhateverButTooLate;
                }, contextPassedToExecute, userCancellationToken).ConfigureAwait(false))
                .Should().Throw<TimeoutRejectedException>();

            contextPassedToOnTimeout.Should().NotBeNull();
            contextPassedToOnTimeout.OperationKey.Should().Be(operationKey);
            contextPassedToOnTimeout.Should().BeSameAs(contextPassedToExecute);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func__optimistic(int programaticallyControlledDelay)
        {

            Func<TimeSpan> timeoutFunc = () => TimeSpan.FromMilliseconds(25*programaticallyControlledDelay);

            TimeSpan? timeoutPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                timeoutPassedToOnTimeout = span;
                return TaskHelper.EmptyTask;
            };

            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutFunc, TimeoutStrategy.Optimistic, onTimeoutAsync);
            var userCancellationToken = CancellationToken.None;

            policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
                {
                    await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);
                    return ResultPrimitive.WhateverButTooLate;
                }, userCancellationToken).ConfigureAwait(false))
                .Should().Throw<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutFunc());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func_influenced_by_context__optimistic(int programaticallyControlledDelay)
        {
            Func<Context, TimeSpan> timeoutProvider = ctx => (TimeSpan)ctx["timeout"];

            TimeSpan? timeoutPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                timeoutPassedToOnTimeout = span;
                return TaskHelper.EmptyTask;
            };

            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutProvider, TimeoutStrategy.Optimistic, onTimeoutAsync);
            var userCancellationToken = CancellationToken.None;

            // Supply a programatically-controlled timeout, via the execution context.
            Context context = new Context("SomeOperationKey")
            {
                ["timeout"] = TimeSpan.FromMilliseconds(25 * programaticallyControlledDelay)
            };

            policy.Awaiting(async p => await p.ExecuteAsync(async (ctx, ct) =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }, context, userCancellationToken).ConfigureAwait(false))
                .Should().Throw<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutProvider(context));
        }

        [Fact]
        public void Should_call_ontimeout_but_not_with_task_wrapping_abandoned_action__optimistic()
        {
            Task taskPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                taskPassedToOnTimeout = task;
                return TaskHelper.EmptyTask;
            };

            TimeSpan timeout = TimeSpan.FromMilliseconds(250);
            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Optimistic, onTimeoutAsync);
            var userCancellationToken = CancellationToken.None;

            policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }, userCancellationToken).ConfigureAwait(false))
            .Should().Throw<TimeoutRejectedException>();

            taskPassedToOnTimeout.Should().BeNull();
        }

        [Fact]
        public void Should_call_ontimeout_with_timing_out_exception__optimistic()
        {
            TimeSpan timeoutPassedToConfiguration = TimeSpan.FromMilliseconds(250);

            Exception exceptionPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync = (ctx, span, task, exception) =>
            {
                exceptionPassedToOnTimeout = exception;
                return TaskHelper.EmptyTask;
            };

            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutPassedToConfiguration, TimeoutStrategy.Optimistic, onTimeoutAsync);
            var userCancellationToken = CancellationToken.None;

            policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
                {
                    await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);
                    return ResultPrimitive.WhateverButTooLate;
                }, userCancellationToken).ConfigureAwait(false))
                .Should().Throw<TimeoutRejectedException>();

            exceptionPassedToOnTimeout.Should().NotBeNull();
            exceptionPassedToOnTimeout.Should().BeOfType(typeof(OperationCanceledException));
        }

        #endregion

    }
}